using Microsoft.Data.Sqlite;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SCS_Mod_Helper.Vehicles; 
/// <summary>
/// DBMigrationWindow.xaml 的交互逻辑
/// </summary>
public partial class DBMigrationWindow: BaseWindow {
	private readonly DBMigrationBinding Binding;
	private readonly bool VeEdited, VaEdited;

	public Dictionary<string, Vehicle> KeepVehiclesETS2 => Binding.KeepETS2;
	public Dictionary<string, Vehicle> KeepVehiclesATS => Binding.KeepATS;

	public DBMigrationWindow(int oldVersion, int newVersion, bool veEdited, bool vaEdited) {
		InitializeComponent();
		Binding = new(() => this, oldVersion, newVersion, veEdited, vaEdited);
		GridMain.DataContext = Binding;
		VeEdited = veEdited;
		VaEdited = vaEdited;

		Closing += OnWindowClosing;
	}

	private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e) {
		var result = MessageBox.Show(this, GetString("MessageDBClose"), GetString("MessageTitleWarning"), MessageBoxButton.YesNo);
		if (result == MessageBoxResult.No) {
			e.Cancel = true;
		}
	}

	protected override void OnContentRendered(EventArgs e) {
		base.OnContentRendered(e);
		if (VeEdited) {
			VehiclesIO.LoadVehiclesUpdate(true, Binding.OldVehiclesETS2, false, true);
			VehiclesIO.LoadVehiclesUpdate(true, Binding.NewVehiclesETS2, true, false);
		}
		if (VaEdited) {
			VehiclesIO.LoadVehiclesUpdate(false, Binding.OldVehiclesATS, false, true);
			VehiclesIO.LoadVehiclesUpdate(false, Binding.NewVehiclesATS, true, false);
		}
	}

	private void ButtonAddRemoveClick(object sender, RoutedEventArgs e) {
		if (Binding.ETS2Selected) {
			AddRemoveVehicle((Button)sender, Binding.OldVehiclesETS2, Binding.NewVehiclesETS2, Binding.KeepETS2);
		} else {
			AddRemoveVehicle((Button)sender, Binding.OldVehiclesATS, Binding.NewVehiclesATS, Binding.KeepATS);
		}
	}


	private void AddRemoveVehicle(
		Button sender,
		ObservableCollection<Vehicle> oldVehicles,
		ObservableCollection<Vehicle> newVehicles,
		Dictionary<string, Vehicle> keepVehicles) {
		if (sender == ButtonAdd) {
			if (oldVehicles.Count == 0)
				return;
			TableNew.SelectedItems.Clear();
			var items = TableOld.SelectedItems;
			Vehicle? lastV = null;
			while (items.Count > 0) {
				Vehicle vehicle = (Vehicle)items[0]!;
				oldVehicles.Remove(vehicle);

				keepVehicles[vehicle.VehicleID] = vehicle;
				newVehicles.Add(vehicle);
				TableNew.SelectedItems.Add(vehicle);
				lastV = vehicle;
			}
			if (lastV != null)
				TableNew.ScrollIntoView(lastV);
		} else if (sender == ButtonRemove) {
			if (keepVehicles.Count == 0)
				return;
			var items = TableNew.SelectedItems;
			Vehicle? lastV = null;
			while (items.Count > 0) {
				Vehicle vehicle = (Vehicle)items[0]!;
				if (keepVehicles.Remove(vehicle.VehicleID)) {

					newVehicles.Remove(vehicle);

					oldVehicles.Add(vehicle);
					lastV = vehicle;

					TableOld.SelectedItems.Add(vehicle);
				} else
					items.RemoveAt(0);
			}
			if (lastV != null)
				TableOld.ScrollIntoView(lastV);
		} else if (sender == ButtonAddAll) {
			if (oldVehicles.Count == 0)
				return;
			foreach (var vehicle in oldVehicles) {
				newVehicles.Add(vehicle);
				keepVehicles.Add(vehicle.VehicleID, vehicle);
				TableNew.SelectedItems.Add(vehicle);
			}
			oldVehicles.Clear();
			TableNew.ScrollIntoView(keepVehicles.Last().Value);
		} else if (sender == ButtonRemoveAll) {
			if (keepVehicles.Count == 0)
				return;
			foreach (var vehicle in keepVehicles.Values) {
				newVehicles.Remove(vehicle);
				oldVehicles.Add(vehicle);
				TableOld.SelectedItems.Add(vehicle);
			}
			keepVehicles.Clear();
			TableOld.ScrollIntoView(oldVehicles.Last());
		}
		if (Binding.ETS2Selected)
			Binding.InvokeChange(nameof(Binding.KeepETS2Count));
		else
			Binding.InvokeChange(nameof(Binding.KeepATSCount));
	}

	private void ButtonFinishClick(object sender, RoutedEventArgs e) {
		StringBuilder message = new(GetString("MessageDBFinish1"));
		if (Binding.OldVehiclesETS2.Count + Binding.OldVehiclesATS.Count > 0)
			message.Append('\n').Append(GetString("MessageDBFinishWithUnselected"));

		var result = MessageBox.Show(this, message.ToString(), GetString("TitleVehicleMigration"), MessageBoxButton.YesNo);
		if (result == MessageBoxResult.Yes) {
			DialogResult = true;
			Close();
		}
	}
}

public partial class VehiclesIO {

	/// <summary>
	/// 重置数据库，只要删除当前db，然后走一遍migration也是一样
	/// </summary>
	/// <param name="window"></param>
	public static void ClearVehicleList(Window window) {
		var dbPath = Paths.VehiclesDBPath();
		SqliteConnection.ClearAllPools();
		File.Delete(dbPath);
		DBCheckBeforeUse();
	}


	//For DB Update
	public static void CheckDBMigration(Window window) {
		ExportDB();
		string currentDBPath = Paths.VehiclesDBPath();
		if (!File.Exists(currentDBPath))
			return;
		string customDBPath = Paths.CustomDBPath();
		bool useCustomDB = File.Exists(customDBPath);
		string source = useCustomDB ? CustomDBSource : EmbDBSource;
		int dbVersion = GetDBVersion(source);
		int currentVersion = GetDBVersion(VehicleDBSource);
		if (dbVersion > currentVersion) {
			bool veEdited = false, vaEdited = false;
			try {
				using var conn = new SqliteConnection(VehicleDBSource);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"Select * from {TABLE_EDITED}";
				using var reader = cmd.ExecuteReader();
				if (reader.Read()) {
					veEdited = reader.GetBool(COLUMN_VEHICLES_ETS2);
					vaEdited = reader.GetBool(COLUMN_VEHICLES_ATS);
				}
				cmd.Dispose();
			} catch {
			}
			Dictionary<string, Vehicle>? keepVehiclesETS2 = null, keepVehiclesATS = null;
			if (veEdited || vaEdited) {
				var result = MessageBox.Show(window, Util.GetString("MessageDBMigration"), Util.GetString("TitleVehicleMigration"), MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes) {
					DBMigrationWindow updateWindow = new(currentVersion, dbVersion, veEdited, vaEdited) {
						Owner = window
					};
					if (updateWindow.ShowDialog() == true) {
						keepVehiclesETS2 = updateWindow.KeepVehiclesETS2;
						keepVehiclesATS = updateWindow.KeepVehiclesATS;
					}
				}
			}
			SqliteConnection.ClearAllPools();
			if (File.Exists(currentDBPath))
				File.Delete(currentDBPath);
			File.Copy(Paths.EmbDBFile(), currentDBPath);
			if (veEdited || vaEdited) {
				string message;
				try {
					if (keepVehiclesETS2 != null)
						InsertKeepVehicles(true, keepVehiclesETS2);
					if (keepVehiclesATS != null)
						InsertKeepVehicles(false, keepVehiclesATS);
					message = Util.GetString("MessageDBMigrationResultSuccess");
				} catch {
					message = Util.GetString("MessageDBMigrationResultFailed");//建议重命名旧的数据库文件，如果途中出错，就复原，成功就删除。
				}
				MessageBox.Show(window, message, Util.GetString("MessageTitleNotice"));
			}
		}
	}

	private static int GetDBVersion(string source) {
		int version;
		using (var conn = new SqliteConnection(source)) {
			conn.Open();
			var cmd = conn.CreateCommand();
			cmd.CommandText = "PRAGMA user_version";
			version = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
			SqliteConnection.ClearAllPools();
			cmd.Dispose();
		}
		return version;
	}


	private static void InsertKeepVehicles(bool isETS2, Dictionary<string, Vehicle> keepVehicles) {
		string table = TableNameVehicle(isETS2);
		using var conn = new SqliteConnection(VehicleDBSource);
		conn.Open();
		SqliteCommand cmd;
		using var transaction = conn.BeginTransaction();
		cmd = conn.CreateCommand();
		cmd.CommandText = @$"Insert or replace into {table} ({COLUMN_VEHICLE_ID}, {COLUMN_MANIFATURER}, {COLUMN_PROD_YEAR}, {COLUMN_VEHICLE_NAME}, {COLUMN_DESCRIPTION}, {COLUMN_DEFAULT_VEHICLE})
							values(@id, @mani, @prodYear, @tname, @desc, @default)";
		cmd.Parameters.Add(new SqliteParameter("@id", null));
		cmd.Parameters.Add(new SqliteParameter("@mani", null));
		cmd.Parameters.Add(new SqliteParameter("@prodYear", null));
		cmd.Parameters.Add(new SqliteParameter("@tname", null));
		cmd.Parameters.Add(new SqliteParameter("@desc", null));
		cmd.Parameters.Add(new SqliteParameter("@default", null));
		foreach (var vehicle in keepVehicles.Values) {
			cmd.Parameters["@id"].Value = vehicle.VehicleID;
			cmd.Parameters["@mani"].Value = vehicle.Manifaturer;
			cmd.Parameters["@prodYear"].Value = vehicle.ProductionYear;
			cmd.Parameters["@tname"].Value = vehicle.VehicleName;
			cmd.Parameters["@desc"].Value = vehicle.Description;
			cmd.Parameters["@default"].Value = 0;
			cmd.ExecuteNonQuery();
		}
		cmd.Dispose();

		//Cabin
		table = TableNameCabin(isETS2);
		cmd = conn.CreateCommand();
		cmd.CommandText = @$"Insert into {table} ({COLUMN_VEHICLE_ID}, {COLUMN_CABIN_ID}, {COLUMN_CABIN_NAME}, {COLUMN_DEFAULT_CABIN})
							values(@vID, @cID, @cName, @default)";
		cmd.Parameters.Add(new SqliteParameter("@vID", null));
		cmd.Parameters.Add(new SqliteParameter("@cID", null));
		cmd.Parameters.Add(new SqliteParameter("@cName", null));
		cmd.Parameters.Add(new SqliteParameter("@default", null));
		foreach (var vehicle in keepVehicles.Values) {
			foreach (var cabin in vehicle.Cabins) {
				if (cabin.DefaultCabin)
					continue;
				cmd.Parameters["@vID"].Value = cabin.VehicleID;
				cmd.Parameters["@cID"].Value = cabin.CabinID;
				cmd.Parameters["@cName"].Value = cabin.CabinName;
				cmd.Parameters["@default"].Value = 0;
				cmd.ExecuteNonQuery();
			}
		}
		cmd.Dispose();

		//Accessory
		table = TableNameAcc(isETS2);
		cmd = conn.CreateCommand();
		cmd.CommandText = @$"Insert into {table} ({COLUMN_VEHICLE_ID}, {COLUMN_ACC_ID}, {COLUMN_ACC_NAME}, {COLUMN_ACC_TYPE}, {COLUMN_UNLOCK_LEVEL}, {COLUMN_DEFAULT_ACC})
							values(@vID, @aID, @aName, @aType, @level, @default)";
		cmd.Parameters.Add(new SqliteParameter("@vID", null));
		cmd.Parameters.Add(new SqliteParameter("@aID", null));
		cmd.Parameters.Add(new SqliteParameter("@aName", null));
		cmd.Parameters.Add(new SqliteParameter("@aType", null));
		cmd.Parameters.Add(new SqliteParameter("@level", null));
		cmd.Parameters.Add(new SqliteParameter("@default", null));
		foreach (var vehicle in keepVehicles.Values) {
			foreach (var acc in vehicle.Accessories) {
				if (acc.DefaultAcc)
					continue;
				cmd.Parameters["@vID"].Value = acc.VehicleID;
				cmd.Parameters["@aID"].Value = acc.AccID;
				cmd.Parameters["@aName"].Value = acc.AccName;
				cmd.Parameters["@aType"].Value = acc.AccType;
				cmd.Parameters["@level"].Value = acc.UnlockLevel;
				cmd.Parameters["@default"].Value = 0;
				cmd.ExecuteNonQuery();
			}
		}
		cmd.Dispose();
		transaction.Commit();
	}

	/// <summary>
	/// 读取所有的车辆，如果customVehicles为true，就只读取所有更改过或新增的车辆
	/// 一般只用于DBMigration，在默认数据库有更新时才完全读取使用
	/// </summary>
	/// <param name="isETS2"></param>
	/// <param name="vehicles"></param>
	/// <param name="defaultDB">选择是否读取默认数据库，True时读取默认数据库</param>
	/// <param name="customVehicle">True时只读更改过的或新增的内容</param>
	public static void LoadVehiclesUpdate(bool isETS2, ObservableCollection<Vehicle> vehicles, bool defaultDB, bool customVehicle) {
		DBCheckBeforeUse();
		string table = TableNameVehicle(isETS2);
		string customDBPath = Paths.CustomDBPath();
		bool useCustomDB = File.Exists(customDBPath);
		string source = defaultDB ? (useCustomDB ? CustomDBSource : EmbDBSource) : VehicleDBSource;
		using var conn = new SqliteConnection(source);
		conn.Open();
		using var cmd = conn.CreateCommand();

		var cabinDict = LoadCabins(cmd, isETS2, customVehicle);
		var accDict = LoadAccessories(cmd, isETS2, customVehicle);

		StringBuilder command = new();
		command.Append($"Select * from {table}");
		if (customVehicle)
			command.Append($" where {COLUMN_DEFAULT_VEHICLE} = 0");
		command.Append($" order by {COLUMN_VEHICLE_TYPE}, {COLUMN_MANIFATURER}, {COLUMN_PROD_YEAR}, {COLUMN_VEHICLE_ID}");
		cmd.CommandText = command.ToString();
		using var reader = cmd.ExecuteReader();
		while (reader.Read()) {
			string vehicleID = reader.GetString(COLUMN_VEHICLE_ID);
			string vehicleType = reader.GetString(COLUMN_VEHICLE_TYPE);
			string manifaturer = reader.GetString(COLUMN_MANIFATURER);
			int prodYear = reader.GetInt(COLUMN_PROD_YEAR);
			string vehicleName = reader.TryGetString(COLUMN_VEHICLE_NAME, GetVehicleName(vehicleID));
			string desc = reader.TryGetString(COLUMN_DESCRIPTION, GetVehicleDesc(vehicleID));
			Vehicle vehicle = new(vehicleID, prodYear, vehicleName, desc, vehicleType, manifaturer, isETS2);
			vehicles.Add(vehicle);
			if (cabinDict.TryGetValue(vehicleID, out var cabins)) {
				vehicle.Cabins = cabins;
			}
			if (accDict.TryGetValue(vehicleID, out var accessories)) {
				vehicle.Accessories = accessories;
			}
		}
	}
	/// <summary>
	/// 读取所有的驾驶室，如果customVehicles为true，就只读取所有更改过或新增的驾驶室
	/// 一般只用于DBMigration，在默认数据库有更新时才完全读取使用
	/// </summary>
	/// <param name="cmd"></param>
	/// <param name="isETS2"></param>
	/// <param name="customVehicle">True时只读更改过的或新增的内容</param>
	/// <returns></returns>
	public static Dictionary<string, List<Cabin>> LoadCabins(SqliteCommand cmd, bool isETS2, bool customVehicle = false) {
		DBCheckBeforeUse();
		string table = TableNameCabin(isETS2);
		StringBuilder command = new();
		command.Append($"Select * from {table}");
		if (customVehicle)
			command.Append($" where {COLUMN_DEFAULT_CABIN} = 0");
		command.Append($" order by {COLUMN_VEHICLE_ID}");
		cmd.CommandText = command.ToString();
		using var reader = cmd.ExecuteReader();
		Dictionary<string, List<Cabin>> dict = [];
		while (reader.Read()) {
			string vehicleID = reader.GetString(COLUMN_VEHICLE_ID);
			string cabinID = reader.GetString(COLUMN_CABIN_ID);
			string cabinName = reader.TryGetString(COLUMN_CABIN_NAME, GetCabinName(cabinID));
			bool defaultCabin = reader.GetBool(COLUMN_DEFAULT_CABIN);
			List<Cabin>? cabins;
			if (dict.TryGetValue(vehicleID, out List<Cabin>? value)) {
				cabins = value;
			} else {
				cabins = [];
				dict[vehicleID] = cabins;
			}
			cabins.Add(new(vehicleID, cabinID, cabinName, defaultCabin));
		}
		return dict;
	}
	/// <summary>
	/// 读取所有的饰品，如果customVehicles为true，就只读取所有更改过或新增的饰品
	/// 一般只用于DBMigration，在默认数据库有更新时完全读取使用
	/// </summary>
	/// <param name="cmd"></param>
	/// <param name="isETS2"></param>
	/// <param name="customVehicle">True时只读更改过的或新增的内容</param>
	/// <returns></returns>
	public static Dictionary<string, List<Accessory>> LoadAccessories(SqliteCommand cmd, bool isETS2, bool customVehicles = false) {
		DBCheckBeforeUse();
		string table = TableNameAcc(isETS2);
		StringBuilder command = new();
		command.Append($"Select * from {table}");
		if (customVehicles)
			command.Append($" where {COLUMN_DEFAULT_ACC} = 0");
		command.Append($" order by {COLUMN_VEHICLE_ID}");
		cmd.CommandText = command.ToString();
		using var reader = cmd.ExecuteReader();
		Dictionary<string, List<Accessory>> dict = [];
		while (reader.Read()) {
			string vehicleID = reader.GetString(COLUMN_VEHICLE_ID);
			string accID = reader.GetString(COLUMN_ACC_ID);
			string accName = reader.TryGetString(COLUMN_ACC_NAME, GetAccessoryName(vehicleID, accID));
			string accType = reader.GetString(COLUMN_ACC_TYPE);
			int unlockLevel = reader.GetInt(COLUMN_UNLOCK_LEVEL);
			bool defaultAcc = reader.GetBool(COLUMN_DEFAULT_ACC);
			List<Accessory>? accessories;
			if (dict.TryGetValue(vehicleID, out List<Accessory>? value)) {
				accessories = value;
			} else {
				accessories = [];
				dict[vehicleID] = accessories;
			}
			accessories.Add(new(vehicleID, accID, accName, accType, unlockLevel, defaultAcc));
		}
		return dict;
	}
}
