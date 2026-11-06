using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Modding.PaintJob;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Vehicles;

public class Vehicle(
	string vehicleID,
	int productionYear,
	string vehicleName,
	string description,
	string vehicleType,
	string manufacturer,
	bool ets2 = false): BaseBinding, IComparable {

	private string mVehicleID = vehicleID;
	public string VehicleID {
		get => mVehicleID;
		set {
			mVehicleID = value;
			InvokeChange();
		}
	}

	private bool mDefaultVehicle = false;
	public bool DefaultVehicle {
		get => mDefaultVehicle;
		set {
			mDefaultVehicle = value;
			InvokeChange();
		}
	}

	public const string VEHICLE_TYPE_TRUCK = "truck";
	public const string VEHICLE_TYPE_TRAILER = "trailer";
	public const string VEHICLE_TYPE_CAR = "car";
	public const string VEHICLE_TYPE_COACH = "coach";
	private readonly string mVehicleType = vehicleType;
	public string VehicleType => mVehicleType;
	public string VehicleTypeName => VehicleType.ToLower() switch {
		VEHICLE_TYPE_TRUCK => GetString("VehicleTypeTruck"),
		VEHICLE_TYPE_TRAILER => GetString("VehicleTypeTrailer"),
		VEHICLE_TYPE_CAR => GetString("VehicleTypeCar"),
		VEHICLE_TYPE_COACH => GetString("VehicleTypeCoach"),
		_ => VehicleType
	};


	private string mManufacturer = manufacturer;
	public string Manufacturer => mManufacturer;
	private string? mVehicleIDName = null;
	private string VehicleIDName {
		get {
			if (mVehicleIDName == null) {
				var vehicleIDDot = VehicleID.IndexOf('.');
				mManufacturer = vehicleIDDot == -1 ? "" : VehicleID[..vehicleIDDot];
				mVehicleIDName = vehicleIDDot == -1 ? VehicleID : VehicleID[(vehicleIDDot + 1)..];
			}
			return mVehicleIDName;
		}
	}

	private int mProductionYear = productionYear;
	public int ProductionYear {
		get => mProductionYear;
		set {
			mProductionYear = value;
			InvokeChange();
		}
	}

	private string mVehicleName = vehicleName;
	public string VehicleName {
		get => mVehicleName;
		set {
			mVehicleName = value;
			InvokeChange();
		}
	}
	private string mDescription = description;
	public string Description {
		get => mDescription;
		set {
			mDescription = value;
			InvokeChange();
		}
	}

	private bool mIsETS2 = ets2;
	public bool IsETS2 {
		get => mIsETS2;
		set {
			mIsETS2 = value;
			InvokeChange();
		}
	}

	public int CompareTo(object? obj) {
		if (obj is Vehicle other) {
			var c = Manufacturer.CompareTo(other.Manufacturer);
			if (c == 0) {
				c = ProductionYear.CompareTo(other.ProductionYear);
				if (c == 0)
					c = VehicleIDName.CompareTo(other.VehicleIDName);
			}
			return c;
		} else {
			throw new ArgumentException("Object is not a Vehicle");
		}
	}

	public VehiclePackage? VehiclePackage {
		get {
			if (field == null)
				GetCabinUV();
			return field;
		}
		set {
			field = value;
			InvokeChange();
		}
	} = null;


	bool UVLoaded = false;
	public void GetCabinUV() {
		if (UVLoaded)
			return;
		UVLoaded = true;//不要放在readvehiclepackage后面，方法赋值给vehiclepackage后，会到getter，然后如果为空就会再次调用getuv导致无限循环
		void recycle() => VehiclePackage = null;
		VehiclePackage = new VehiclePackageIO().ReadVehiclePackage(VehicleID, recycle);
	}

	private List<Cabin>? mCabins = null;
	public List<Cabin> Cabins {
		get {
			mCabins ??= VehiclesIO.LoadCabin(IsETS2, VehicleID);
			return mCabins;
		}
		set {
			mCabins = value;
			InvokeChange();
		}
	}

	public List<Accessory>? mAccessories = null;
	public List<Accessory> Accessories {
		get {
			mAccessories ??= VehiclesIO.LoadAccessories(IsETS2, mVehicleID);
			return mAccessories;
		}
		set {
			mAccessories = value;
			InvokeChange();
		}
	}

	private string? mCabinNames = null;
	public string CabinIDs {
		get {
			mCabinNames ??= string.Join(", ", Cabins.Select(c => c.CabinID));
			return mCabinNames;
		}
	}
	private string? mAccessoryNames = null;
	public string AccessoryIDs {
		get {
			mAccessoryNames ??= string.Join(", ", Accessories.Select(c => c.AccID));
			return mAccessoryNames;
		}
	}

	public override string ToString() => $"{VehicleID} {VehicleName} {ProductionYear}";
}

public class Cabin(string vehicleID, string cabinID, string cabinName, bool defaultCabin): BaseBinding {
	public string mVehicleID = vehicleID;
	public string VehicleID {
		get => mVehicleID;
		set {
			mVehicleID = value;
			InvokeChange();
		}
	}
	public string mCabinID = cabinID;
	public string CabinID {
		get => mCabinID;
		set {
			mCabinID = value;
			InvokeChange();
		}
	}
	public string mCabinName = cabinName;
	public string CabinName {
		get => mCabinName;
		set {
			mCabinName = value;
			InvokeChange();
		}
	}

	private bool mDefaultCabin = defaultCabin;
	public bool DefaultCabin {
		get => mDefaultCabin;
		set {
			mDefaultCabin = value;
			InvokeChange();
		}
	}
}

public class Accessory(string vehicleID, string accID, string accName, string accType, int unlockLevel, bool defaultAcc): BaseBinding {
	private PaintJobOverrideData? mBelongingOverride = null;
	public PaintJobOverrideData? BelongingOverride {
		get => mBelongingOverride;
		set {
			mBelongingOverride = value;
			InvokeChange();
		}
	}

	private Visibility mIndexVisibility = Visibility.Visible;
	public Visibility IndexVisibility {
		get => mIndexVisibility;
		set {
			mIndexVisibility = value;
			InvokeChange();
		}
	}

	public bool IndexVisible {
		get => IndexVisibility == Visibility.Visible;
		set {
			if (BelongingOverride != null && value)
				IndexVisibility = Visibility.Visible;
			else
				IndexVisibility = Visibility.Hidden;
		}
	}

	private bool mCheck = false;
	public bool Check {
		get => mCheck;
		set {
			mCheck = value;
			if (value || BelongingOverride == null) {
				IndexVisible = false;
			} else
				IndexVisible = true;
			InvokeChange();
		}
	}




	private string mVehicleID = vehicleID;
	public string VehicleID {
		get => mVehicleID;
		set {
			mVehicleID = value;
			InvokeChange();
		}
	}

	private string mAccID = accID;
	public string AccID {
		get => mAccID;
		set {
			mAccID = value;
			InvokeChange();
		}
	}
	private string mAccName = accName;
	public string AccName {
		get => mAccName;
		set {
			mAccName = value;
			InvokeChange();
		}
	}
	private string mAccType = accType;
	public string AccType {
		get => mAccType;
		set {
			mAccType = value;
			InvokeChange();
		}
	}
	private int mUnlockLevel = unlockLevel;
	public int UnlockLevel {
		get => mUnlockLevel;
		set {
			mUnlockLevel = value;
			InvokeChange();
		}
	}

	private bool mDefaultAcc = defaultAcc;
	public bool DefaultAcc {
		get => mDefaultAcc;
		set {
			mDefaultAcc = value;
			InvokeChange();
		}
	}
}

public enum VehicleType {
	[Description("truck")]
	Truck,
	[Description("trailer_owned")]
	Trailer,
	[Description("car")]
	Car,
	[Description("coach")]
	Coach
}
public static class VehicleTypeExtension {
	public static string GetTypeValue(this VehicleType type) {
		return type switch {
			VehicleType.Truck => "truck",
			VehicleType.Trailer => "trailer_owned",
			VehicleType.Car => "car",
			VehicleType.Coach => "coach",
			_ => throw new ArgumentException("")
		};
	}
	public static VehicleType GetTypeFromValue(string type) {
		return type switch {
			"truck" => VehicleType.Truck,
			"trailer_owned" => VehicleType.Trailer,
			"car" => VehicleType.Car,
			"coach" => VehicleType.Coach,
			_ => throw new ArgumentException("")
		};
	}
	public static string GetLocalizedName(this VehicleType type) {
		return Util.GetString(type switch {
			VehicleType.Truck => "VehicleTypeTruck",
			VehicleType.Trailer => "VehicleTypeTrailer",
			VehicleType.Car => "VehicleTypeCar",
			VehicleType.Coach => "VehicleTypeCoach",
			_ => throw new ArgumentException("")
		});
	}
}
	}
}
