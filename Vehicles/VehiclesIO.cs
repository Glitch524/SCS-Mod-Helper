using Microsoft.Data.Sqlite;
using SCS_Mod_Helper.Modding.Accessories;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace SCS_Mod_Helper.Vehicles;

public static partial class VehiclesIO {
	//数据库有版本号，例如1061260920，前四位是版本号对应游戏版本1.61，0是为了避免游戏子版本破百，后六位是日期
	private const string defaultDB = "SCS_Mod_Helper.Vehicles.DefaultVehicles.db";
	//private const int dbVersion = 1061261017;

	private static string EmbDBSource => "Data Source=" + Paths.EmbDBFile();
	private static string CustomDBSource => "Data Source=" + Paths.CustomDBPath();
	private static string VehicleDBSource => "Data Source=" + Paths.VehiclesDBPath();

	private const string TABLE_VEHICLES_ETS2 = "VehiclesETS2";
	private const string TABLE_VEHICLES_ATS = "VehiclesATS";
	private const string COLUMN_VEHICLE_ID = "VehicleID";
	private const string COLUMN_VEHICLE_TYPE = "VehicleType";
	private const string COLUMN_MANIFATURER = "Manifaturer";
	private const string COLUMN_PROD_YEAR = "ProductionYear";
	private const string COLUMN_VEHICLE_NAME = "VehicleName";
	private const string COLUMN_DESCRIPTION = "Description";
	private const string COLUMN_DEFAULT_VEHICLE = "DefaultVehicle";
	private const string COLUMN_V_TYPE_CODE = "VTypeCode";


	private const string TABLE_CABINS_ETS2 = "CabinsETS2";
	private const string TABLE_CABINS_ATS = "CabinsATS";
	private const string COLUMN_CABIN_ID = "CabinID";
	private const string COLUMN_CABIN_NAME = "CabinName";
	private const string COLUMN_DEFAULT_CABIN = "DefaultCabin";

	private const string TABLE_ACC_ETS2 = "AccessoriesETS2";
	private const string TABLE_ACC_ATS = "AccessoriesATS";
	private const string COLUMN_ACC_ID = "AccessoryID";
	private const string COLUMN_ACC_NAME = "AccessoryName";
	private const string COLUMN_ACC_TYPE = "AccessoryType";
	private const string COLUMN_UNLOCK_LEVEL = "UnlockLevel";
	private const string COLUMN_DEFAULT_ACC = "DefaultAccessory";

	private const string TABLE_ACCESSORY_ORDER = "AccessoryOrder";
	private const string COLUMN_ORDER = "Order";

	private const string TABLE_EDITED = "TableEdited";
	private const string COLUMN_VEHICLES_ETS2 = "VehiclesETS2";
	private const string COLUMN_VEHICLES_ATS = "VehiclesATS";


	private static string TableNameVehicle(bool isETS2) => isETS2 ? TABLE_VEHICLES_ETS2 : TABLE_VEHICLES_ATS;
	private static string TableNameCabin(bool isETS2) => isETS2 ? TABLE_CABINS_ETS2 : TABLE_CABINS_ATS;
	private static string TableNameAcc(bool isETS2) => isETS2 ? TABLE_ACC_ETS2 : TABLE_ACC_ATS;

	private static string GetString(this SqliteDataReader reader, string columnName) => reader.GetString(reader.GetOrdinal(columnName));
	private static int GetInt(this SqliteDataReader reader, string columnName) => reader.GetInt32(reader.GetOrdinal(columnName));
	private static bool GetBool(this SqliteDataReader reader, string columnName) => reader.GetInt32(reader.GetOrdinal(columnName)) == 1;
	private static string TryGetString(this SqliteDataReader reader, string columnName, string defaultValue) {
		int ordinal = reader.GetOrdinal(columnName);
		if (reader.IsDBNull(ordinal))
			return defaultValue;
		else
			return reader.GetString(ordinal);
	}





	private static ResourceDictionary? mVehicleDictionary = null;
	public static ResourceDictionary VehicleDictionary {
		get {
			if (mVehicleDictionary == null) {
				mVehicleDictionary = [];
				var lang = Instances.CurrentLanguage;
				Uri source;
				if (lang.StartsWith(ResourceUtil.ExtLangPreffix)) {
					lang = lang[ResourceUtil.ExtLangPreffix.Length..];
					var path = Path.Combine(Paths.VehiclesLanguageDir(), $"{lang}.xaml");
					if (File.Exists(path))
						source = new Uri(path);
					else
						source = new Uri("pack://application:,,,/Vehicles/en-US.xaml", UriKind.Absolute);
				} else {
					source = new Uri($"pack://application:,,,/Vehicles/{lang}.xaml", UriKind.Absolute);
				}
				mVehicleDictionary.Source = source;
			}
			return mVehicleDictionary;
		}
	}
	public static void ClearVehicleDict() => mVehicleDictionary = null;

	public static int GetProdYear(string vehicleID) => (int)(VehicleDictionary[$"prodYear.{vehicleID}"] ?? 0);
	public static string GetVehicleName(string vehicleID) => (string)(VehicleDictionary[$"name.{vehicleID}"] ?? vehicleID);
	public static string GetVehicleDesc(string vehicleID) => (string)(VehicleDictionary[$"desc.{vehicleID}"] ?? "");

	public static string GetCabinName(string cabinID) => (string)(VehicleDictionary[cabinID] ?? cabinID);
	public static string GetAccessoryName(string vehicleID, string accID) => (string)(VehicleDictionary[$"{vehicleID}.{accID}"] ?? accID);

	/// <summary>
	/// 从项目中的数据库资源导出到临时文件夹内，判断导出时生成的hash来判断导出的文件是否相同
	/// </summary>
	/// <exception cref="FileNotFoundException"></exception>
	private static void ExportDB() {
		var tempDB = Paths.EmbDBFile();
		string hashFile = Paths.EmbHashFile();
		var embResDBHash = GetEmbResDBHash();
		if (File.Exists(tempDB)) {
			string? tempDBHash;
			if (File.Exists(hashFile))
				tempDBHash = File.ReadAllText(hashFile);//获取文件内的hash
			else
				tempDBHash = null;
			//检查hash，如果hash不一样或者文件不存在，就覆盖。如果hash一样就直接返回不需要更新
			if (embResDBHash == tempDBHash)
				return;
			File.Delete(tempDB);
		}
		if (File.Exists(hashFile))
			File.Delete(hashFile);

		var assembly = Assembly.GetExecutingAssembly();
		using Stream? stream = assembly.GetManifestResourceStream(defaultDB) ?? throw new FileNotFoundException("Default not found");
		DirectoryInfo di = new(tempDB);
		di.Parent!.Create();
		using FileStream fs = new(tempDB, FileMode.Create);
		stream.CopyTo(fs);

		File.WriteAllText(hashFile, embResDBHash);
	}

	private static string GetEmbResDBHash() {
		using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(defaultDB) ?? throw new FileNotFoundException("Default not found");
		using var md5 = MD5.Create();
		var hash = md5.ComputeHash(stream);
		return Convert.ToBase64String(hash);
	}

	/// <summary>
	/// 在使用数据库前，先判断数据库是否存在，如果不存在就把默认数据库结构复制到数据库位置
	/// </summary>
	private static void DBCheckBeforeUse() {
		string dbPath = Paths.VehiclesDBPath();
		if (File.Exists(dbPath))
			return;
		if (File.Exists(Paths.CustomDBPath())) {
			File.Copy(Paths.CustomDBPath(), dbPath);
			return;
		}
		ExportDB();
		File.Copy(Paths.EmbDBFile(), dbPath);
	}




	public static void AddVehicle(Vehicle vehicle) {
		bool isETS2 = vehicle.IsETS2;
		string table = TableNameVehicle(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = @$"Insert into {table} ({COLUMN_VEHICLE_ID}, {COLUMN_VEHICLE_TYPE}, {COLUMN_MANIFATURER}, {COLUMN_PROD_YEAR}, {COLUMN_VEHICLE_NAME}, {COLUMN_DESCRIPTION})
							values($id, $type, $mani, $prodYear, $vname, $desc)";
		cmd.Parameters.AddWithValue("$id", vehicle.VehicleID);
		cmd.Parameters.AddWithValue("$type", vehicle.VehicleType);
		cmd.Parameters.AddWithValue("$mani", vehicle.Manifaturer);
		cmd.Parameters.AddWithValue("$prodYear", vehicle.ProductionYear);
		cmd.Parameters.AddWithValue("$vname", vehicle.VehicleName);
		cmd.Parameters.AddWithValue("$desc", vehicle.Description);
		cmd.ExecuteNonQuery();
	}

	public static void DeleteVehicle(Vehicle vehicle) {
		bool isETS2 = vehicle.IsETS2;
		string table = TableNameVehicle(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = $"Delete from {table} where {COLUMN_VEHICLE_ID} = $id";
		cmd.Parameters.AddWithValue("$id", vehicle.VehicleID);
		cmd.ExecuteNonQuery();
	}

	public static void DeleteVehicle(bool isETS2, List<string> vehicleIDs) {
		string table = TableNameVehicle(isETS2);
		string ins = string.Join(", ", vehicleIDs.Select(_ => "$id"));
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		using var transaction = conn.BeginTransaction();
		try {
			var cmd = conn.CreateCommand();
			cmd.CommandText = $"Delete from {table} where {COLUMN_VEHICLE_ID} in ({ins})";
			foreach (var id in vehicleIDs)
				cmd.Parameters.AddWithValue("$id", id);
			cmd.ExecuteNonQuery();
			transaction.Commit();
		} catch {
			transaction.Rollback();
			throw;
		}
	}

	public static void EditVehicle(Vehicle vehicle) {
		bool isETS2 = vehicle.IsETS2;
		string table = TableNameVehicle(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = @$"update {table} 
							set {COLUMN_VEHICLE_TYPE} = $type, {COLUMN_MANIFATURER} = $mani, {COLUMN_PROD_YEAR} = $prodYear, {COLUMN_VEHICLE_NAME} = $vname, {COLUMN_DESCRIPTION} = $desc, {COLUMN_DEFAULT_VEHICLE} = $default 
							where {COLUMN_VEHICLE_ID} = $id";
		cmd.Parameters.AddWithValue("$id", vehicle.VehicleID);
		cmd.Parameters.AddWithValue("$type", vehicle.VehicleType);
		cmd.Parameters.AddWithValue("$mani", vehicle.Manifaturer);
		cmd.Parameters.AddWithValue("$prodYear", vehicle.ProductionYear);
		cmd.Parameters.AddWithValue("$vname", vehicle.VehicleName);
		cmd.Parameters.AddWithValue("$desc", vehicle.Description);
		cmd.Parameters.AddWithValue("$default", 0);
		cmd.ExecuteNonQuery();
	}

	public static async Task LoadAccessoryVehicle(bool isETS2, ObservableCollection<AccessoryVehicle> vehicles) {
		DBCheckBeforeUse();
		string table = TableNameVehicle(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = $"Select *  from {table} order by {COLUMN_VEHICLE_TYPE}, {COLUMN_MANIFATURER}, {COLUMN_PROD_YEAR}, {COLUMN_VEHICLE_ID}";
		using var reader = cmd.ExecuteReader();
		List<AccessoryVehicle> loadedVehicles = [];
		while (reader.Read()) {
			string vehicleID = reader.GetString(COLUMN_VEHICLE_ID);
			string vehicleType = reader.GetString(COLUMN_VEHICLE_TYPE);
			string manifaturer = reader.GetString(COLUMN_MANIFATURER);
			int prodYear = reader.GetInt(COLUMN_PROD_YEAR);
			string vehicleName = reader.TryGetString(COLUMN_VEHICLE_NAME, GetVehicleName(vehicleID));
			string desc = reader.TryGetString(COLUMN_DESCRIPTION, GetVehicleDesc(vehicleID));
			AccessoryVehicle vehicle = new(vehicleID, prodYear, vehicleName, desc, vehicleType, manifaturer, isETS2);
			loadedVehicles.Add(vehicle);
		}
		await Application.Current.Dispatcher.InvokeAsync(() => {
			foreach (var v in loadedVehicles) {
				vehicles.Add(v);
			}
		});
	}

	public static void LoadPaintJobVehicles(ObservableCollection<Vehicle> vehicles) {
		LoadPaintJobVehicles(true, vehicles);
		LoadPaintJobVehicles(false, vehicles);
	}

	private static void LoadPaintJobVehicles(bool isETS2, ObservableCollection<Vehicle> vehicles) {
		DBCheckBeforeUse();
		string table = TableNameVehicle(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();

		cmd.CommandText = $"Select *  from {table} order by {COLUMN_MANIFATURER}, {COLUMN_PROD_YEAR}, {COLUMN_VEHICLE_ID}";
		using var reader = cmd.ExecuteReader();
		List<Vehicle> loadedvehicle = [];
		while (reader.Read()) {
			string vehicleID = reader.GetString(COLUMN_VEHICLE_ID);
			string vehicleType = reader.GetString(COLUMN_VEHICLE_TYPE);
			string manifaturer = reader.GetString(COLUMN_MANIFATURER);
			int prodYear = reader.GetInt(COLUMN_PROD_YEAR);
			string? vehicleName = reader.TryGetString(COLUMN_VEHICLE_NAME, GetVehicleName(vehicleID));
			string? desc = reader.TryGetString(COLUMN_DESCRIPTION, GetVehicleDesc(vehicleID));
			Vehicle vehicle = new(vehicleID, prodYear, vehicleName, desc, vehicleType, manifaturer, isETS2);
			loadedvehicle.Add(vehicle);
		}
		foreach (var v in loadedvehicle) {
			vehicles.Add(v);
		}
	}

	public static List<Cabin> LoadCabin(bool isETS2, string vehicleID) {
		DBCheckBeforeUse();
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();

		string table = TableNameCabin(isETS2);
		StringBuilder command = new();
		command.Append($"Select * from {table}");
		command.Append($" where {COLUMN_VEHICLE_ID} = $vId");
		cmd.CommandText = command.ToString();
		cmd.Parameters.AddWithValue("$vID", vehicleID);
		using var reader = cmd.ExecuteReader();
		List<Cabin> cabins = [];
		while (reader.Read()) {
			string cabinID = reader.GetString(COLUMN_CABIN_ID);
			string cabinName = reader.TryGetString(COLUMN_CABIN_NAME, GetCabinName(cabinID));
			bool defaultCabin = reader.GetBool(COLUMN_DEFAULT_CABIN);

			cabins.Add(new(vehicleID, cabinID, cabinName, defaultCabin));
		}
		return cabins;
	}
	public static List<Accessory> LoadAccessories(bool isETS2, string vehicleID) {
		DBCheckBeforeUse();
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();

		string table = TableNameAcc(isETS2);
		StringBuilder command = new();
		command.Append($"Select * from {table}");
		command.Append($" where {COLUMN_VEHICLE_ID} = $vId");
		cmd.CommandText = command.ToString();
		cmd.Parameters.AddWithValue("$vID", vehicleID);
		using var reader = cmd.ExecuteReader();
		List<Accessory> accessories = [];
		while (reader.Read()) {
			string accID = reader.GetString(COLUMN_ACC_ID);
			string accName = reader.TryGetString(COLUMN_ACC_NAME, GetAccessoryName(vehicleID, accID));
			string accType = reader.GetString(COLUMN_ACC_TYPE);
			int unlockLevel = reader.GetInt(COLUMN_UNLOCK_LEVEL);
			bool defaultAcc = reader.GetBool(COLUMN_DEFAULT_ACC);
			accessories.Add(new(vehicleID, accID, accName, accType, unlockLevel, defaultAcc));
		}
		return accessories;
	}



	public static void AddCabin(bool isETS2, Cabin cabin) {
		string table = TableNameCabin(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = @$"Insert into {table} ({COLUMN_VEHICLE_ID}, {COLUMN_CABIN_ID}, {COLUMN_CABIN_NAME})
							values($vID, $cID, $cName)";
		cmd.Parameters.AddWithValue("$vID", cabin.VehicleID);
		cmd.Parameters.AddWithValue("$cID", cabin.CabinID);
		cmd.Parameters.AddWithValue("$cName", cabin.CabinName);
		cmd.ExecuteNonQuery();
	}

	public static void DeleteCabin(bool isETS2, Cabin cabin) {
		string table = TableNameCabin(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = $"Delete from {table} where {COLUMN_VEHICLE_ID} = $vID and {COLUMN_CABIN_ID} = $cID";
		cmd.Parameters.AddWithValue("$vID", cabin.VehicleID);
		cmd.Parameters.AddWithValue("$cID", cabin.CabinID);
		cmd.ExecuteNonQuery();
	}

	public static void EditCabin(bool isETS2, Cabin cabin) {
		string table = TableNameCabin(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = @$"update {table} 
							set {COLUMN_CABIN_NAME} = $cName, {COLUMN_DEFAULT_CABIN} = $default 
							where {COLUMN_VEHICLE_ID} = $vID and {COLUMN_CABIN_ID} = $cID";
		cmd.Parameters.AddWithValue("$cName", cabin.CabinName);
		cmd.Parameters.AddWithValue("$default", 0);
		cmd.Parameters.AddWithValue("$vID", cabin.VehicleID);
		cmd.Parameters.AddWithValue("$cID", cabin.CabinID);
		cmd.ExecuteNonQuery();

		cmd = conn.CreateCommand();
		table = TableNameVehicle(isETS2);
		cmd.CommandText = @$"update {table} 
							set {COLUMN_DEFAULT_VEHICLE} = $default 
							where {COLUMN_VEHICLE_ID} = $vID";
		cmd.Parameters.AddWithValue("$default", 0);
		cmd.Parameters.AddWithValue("$vID", cabin.VehicleID);
		cmd.ExecuteNonQuery();
	}

	public static void LoadCabins(bool isETS2, string vehicleID, Collection<Cabin> cabins) {
		DBCheckBeforeUse();
		string table = TableNameCabin(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = $"select * from {table} where {COLUMN_VEHICLE_ID} = $id";
		cmd.Parameters.AddWithValue("$id", vehicleID);
		using var reader = cmd.ExecuteReader();
		while (reader.Read()) {
			string cabinID = reader.GetString(reader.GetOrdinal(COLUMN_CABIN_ID));
			string cabinName;
			if (reader.IsDBNull(reader.GetOrdinal(COLUMN_CABIN_NAME))) {
				cabinName = GetCabinName(cabinID);
			} else
				cabinName = reader.GetString(reader.GetOrdinal(COLUMN_CABIN_NAME));
			bool defaultCabin = reader.GetInt32(reader.GetOrdinal(COLUMN_DEFAULT_CABIN)) == 1;
			Cabin cabin = new(vehicleID, cabinID, cabinName, defaultCabin);
			cabins.Add(cabin);
		}
	}

	public static void AddAccessory(bool isETS2, Accessory accessory) {
		string table = TableNameAcc(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = @$"Insert into {table} ({COLUMN_VEHICLE_ID}, {COLUMN_ACC_ID}, {COLUMN_ACC_NAME}, {COLUMN_ACC_TYPE}, {COLUMN_UNLOCK_LEVEL})
							values($vID, $aID, $aName, $aType, $Level)";
		cmd.Parameters.AddWithValue("$vID", accessory.VehicleID);
		cmd.Parameters.AddWithValue("$aID", accessory.AccID);
		cmd.Parameters.AddWithValue("$aName", accessory.AccName);
		cmd.Parameters.AddWithValue("$aType", accessory.AccType);
		cmd.Parameters.AddWithValue("$Level", accessory.UnlockLevel);
		cmd.ExecuteNonQuery();
	}

	public static void DeleteAccessory(bool isETS2, Accessory accessory) {
		string table = TableNameAcc(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = $"Delete from {table} where {COLUMN_VEHICLE_ID} = $vID and {COLUMN_ACC_ID} = $aID";
		cmd.Parameters.AddWithValue("$vID", accessory.VehicleID);
		cmd.Parameters.AddWithValue("$aID", accessory.AccID);
		cmd.ExecuteNonQuery();
	}

	public static void EditAccessory(bool isETS2, Accessory accessory) {
		string table = TableNameAcc(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = @$"update {table} 
							set {COLUMN_ACC_NAME} = $aName, 
								{COLUMN_ACC_TYPE} = $aType, 
								{COLUMN_UNLOCK_LEVEL} = $Level, 
								{COLUMN_DEFAULT_ACC} = $default
							where {COLUMN_VEHICLE_ID} = $vID and {COLUMN_ACC_ID} = $aID";
		cmd.Parameters.AddWithValue("$aName", accessory.AccName);
		cmd.Parameters.AddWithValue("$aType", accessory.AccType);
		cmd.Parameters.AddWithValue("$Level", accessory.UnlockLevel);
		cmd.Parameters.AddWithValue("$default", 0);
		cmd.Parameters.AddWithValue("$vID", accessory.VehicleID);
		cmd.Parameters.AddWithValue("$aID", accessory.AccID);
		cmd.ExecuteNonQuery();

		cmd = conn.CreateCommand();
		table = TableNameVehicle(isETS2);
		cmd.CommandText = @$"update {table} 
							set {COLUMN_DEFAULT_VEHICLE} = $default 
							where {COLUMN_VEHICLE_ID} = $vID";
		cmd.Parameters.AddWithValue("$default", 0);
		cmd.Parameters.AddWithValue("$vID", accessory.VehicleID);
		cmd.ExecuteNonQuery();
	}

	public static void LoadAccessories(bool isETS2, string vehicleID, Collection<Accessory> accessories) {
		DBCheckBeforeUse();
		string table = TableNameAcc(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		var cmd = conn.CreateCommand();
		cmd.CommandText = $"select * from {table} where {COLUMN_VEHICLE_ID} = $id";
		cmd.Parameters.AddWithValue("$id", vehicleID);
		using var reader = cmd.ExecuteReader();
		while (reader.Read()) {
			string accID = reader.GetString(COLUMN_ACC_ID);
			string accName = reader.TryGetString(COLUMN_ACC_NAME, GetAccessoryName(vehicleID, accID));
			string accType = reader.GetString(COLUMN_ACC_TYPE);
			int unlockLevel = reader.GetInt(COLUMN_UNLOCK_LEVEL);
			bool defaultAcc = reader.GetBool(COLUMN_DEFAULT_ACC);
			Accessory accessory = new(vehicleID, accID, accName, accType, unlockLevel, defaultAcc);
			accessories.Add(accessory);
		}
	}
}