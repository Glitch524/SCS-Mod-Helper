using SCS_Mod_Helper.Vehicles;

namespace SCS_Mod_Helper.Modding.Accessories;

public class AccessoryVehicle(
	string vehicleID,
	int productionYear,
	string vehicleName,
	string description,
	VehicleType vehicleType,
	string manufacturer,
	bool ets2 = false,
	bool check = false,
	string modelType = "",
	string look = "",
	string variant = ""): Vehicle(vehicleID, productionYear, vehicleName, description, vehicleType, manufacturer, ets2) {

	private bool mCheck = check;
	public bool Check {
		get => mCheck;
		set {
			mCheck = value;
			InvokeChange();
		}
	}
	private string mModelType = modelType;
	public string ModelType {
		get => mModelType;
		set {
			mModelType = value;
			InvokeChange();
		}
	}

	private string mLook = look;
	public string Look {
		get => mLook;
		set {
			mLook = value;
			InvokeChange();
		}
	}

	private string mVariant = variant;
	public string Variant {
		get => mVariant;
		set {
			mVariant = value;
			InvokeChange();
		}
	}
}
