using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Language;
using SCS_Mod_Helper.Modding.PaintJob;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Vehicles;

public class Vehicle(
	string vehicleID,
	int productionYear,
	string vehicleName,
	string description,
	VehicleType vehicleType,
	string manufacturer,
	bool ets2 = false): BaseBinding, IComparable {
	public bool Focusable { get; } = true;
	public bool Expanded {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;

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

	private readonly VehicleType mVehicleType = vehicleType;
	public VehicleType VehicleType => mVehicleType;
	public string VehicleTypeName => VehicleType.GetLocalizedName();


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
	[Description("bus")]
	Bus
}
public static class VehicleTypeExtension {
	public static string GetTypeValue(this VehicleType type) {
		return type switch {
			VehicleType.Truck => "truck",
			VehicleType.Trailer => "trailer_owned",
			VehicleType.Car => "car",
			VehicleType.Bus => "bus",
			_ => throw new ArgumentException("")
		};
	}
	public static VehicleType GetTypeFromValue(string type) {
		return type switch {
			"truck" => VehicleType.Truck,
			"trailer_owned" => VehicleType.Trailer,
			"car" => VehicleType.Car,
			"bus" => VehicleType.Bus,
			_ => throw new ArgumentException("")
		};
	}
	public static string GetLocalizedName(this VehicleType type) {
		return Resource.GetString(type switch {
			VehicleType.Truck => "VehicleTypeTruck",
			VehicleType.Trailer => "VehicleTypeTrailer",
			VehicleType.Car => "VehicleTypeCar",
			VehicleType.Bus => "VehicleTypeBus",
			_ => throw new ArgumentException("")
		});
	}
}

public class VehicleGroups {
	private readonly VehicleGroup GroupTruckETS2 = new(VehicleType.Truck, true);
	readonly VehicleGroup GroupTruckATS = new(VehicleType.Truck, false);
	readonly VehicleGroup GroupTrailerETS2 = new(VehicleType.Trailer, true);
	readonly VehicleGroup GroupTrailerATS = new(VehicleType.Trailer, false);
	readonly VehicleGroup GroupCarETS2 = new(VehicleType.Car, true);
	readonly VehicleGroup GroupCarATS = new(VehicleType.Car, false);
	readonly VehicleGroup GroupBusETS2 = new(VehicleType.Bus, true);
	readonly VehicleGroup GroupBusATS = new(VehicleType.Bus, false);
	public ObservableCollection<VehicleGroup> GroupList { get; } = [];

	public void Add(bool isETS2, VehicleType vehicleType, Vehicle vehicle) {
		int index;
		VehicleGroup group;
		switch (vehicleType) {
			case VehicleType.Truck:
				if (isETS2) {
					group = GroupTruckETS2;
					index = 0;
				} else {
					group = GroupTruckATS;
					index = 1;
				}
				break;
			case VehicleType.Trailer:
				if (isETS2) {
					group = GroupTrailerETS2;
					index = 2;
				} else {
					group = GroupTrailerATS;
					index = 3;
				}
				break;
			case VehicleType.Car:
				if (isETS2) {
					group = GroupCarETS2;
					index = 4;
				} else {
					group = GroupCarATS;
					index = 5;
				}
				break;
			case VehicleType.Bus:
				if (isETS2) {
					group = GroupBusETS2;
					index = 6;
				} else {
					group = GroupBusATS;
					index = 7;
				}
				break;
			default:
				return;
		}
		group.Add(vehicle);
		AddToList(index, group);
	}

	private readonly int[] Added = [0, 0, 0, 0, 0, 0, 0, 0];
	private void AddToList(int index, VehicleGroup group) {
		if (Added[index] == 1 || group.Vehicles.Count == 0)
			return;
		int insertIndex = 0;
		for (int i = 0; i <= index; i++) {
			if (Added[i] == 1) 
				insertIndex++;
		}
		group.InvokeVisible = ExpandCheck;
		group.Index = insertIndex;//将index记在变量内，方便展开收起时的计算
		if (insertIndex == 0) {//确保永远是第一个被打开
			group.Expanded = true;
			GroupList.FirstOrDefault()?.Expanded = false;
		}
		GroupList.Insert(insertIndex, group);
		for (int i = insertIndex + 1; i < GroupList.Count; i++) {//因为插入了新的值，这个值后面变量的index需要+1才能和列表的index同步
			GroupList[i].Index++;
		}
		Added[index] = 1;
	}

	int OpeningIndex = -1;
	private void ExpandCheck(int index, bool visible) {
		if (visible) {
			if (OpeningIndex == -1) {
				OpeningIndex = index;
			} else {
				GroupList[OpeningIndex].Expanded = false;
				OpeningIndex = index;
			}
		} else {
			if(OpeningIndex == index)//因为新的展开会让旧的变量收起，expanded被设为false时也会触发一次expandcheck，避免直接把openingindex的赋值取消
				OpeningIndex = -1;
		}
	}
}

public class VehicleGroup(VehicleType vehicleType, bool isETS2): BaseBinding {
	public int Index = -1;
	public bool Focusable { get; } = false;
	private readonly VehicleType VehicleType = vehicleType;
	private readonly bool IsETS2 = isETS2;

	public Action<int, bool>? InvokeVisible = null;

	public bool Expanded {
		get;
		set {
			field = value;
			InvokeVisible?.Invoke(Index, value);
			InvokeChange();
		}
	} = false;

	public string GroupNameVisual => $"{VehicleType.GetLocalizedName()}({Resource.GetString(IsETS2 ? "ETS2Short" : "ATSShort")})";

	public List<Vehicle> Vehicles { get; } = [];

	public void Add(Vehicle vehicle) {
		Vehicles.Add(vehicle);
	}
}