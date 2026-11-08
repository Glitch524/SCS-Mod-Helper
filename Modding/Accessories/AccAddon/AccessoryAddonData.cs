using SCS_Mod_Helper.Modding.Accessories.Physics;
using System.Collections.ObjectModel;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon;
public class AccessoryAddonData: AccessoryIntData {

	private string mModelPath = "";
	public string ModelPath {
		get => mModelPath;
		set {
			mModelPath = value;
			InvokeChange();
		}
	}

	private string mModelPathUK = "";
	public string ModelPathUK {
		get => mModelPathUK;
		set {
			mModelPathUK = value;
			InvokeChange();
		}
	}

	private string mExtModelPath = "";
	public string ExtModelPath {
		get => mExtModelPath;
		set {
			mExtModelPath = value;
			InvokeChange();
		}
	}

	private string mExtModelPathUK = "";
	public string ExtModelPathUK {
		get => mExtModelPathUK;
		set {
			mExtModelPathUK = value;
			InvokeChange();
		}
	}

	private string mModelType = "";
	public string AccType {
		get => mModelType;
		set {
			mModelType = value;
			InvokeChange();
		}
	}

	public List<PhysicsData> PhysicsList = [];

	public ObservableCollection<string> LookList = [];
	public ObservableCollection<string> VariantList = [];


	protected uint mHideIn;
	public uint HideIn {
		get => mHideIn;
		set {
			mHideIn = value;
			InvokeChange();
		}
	}

	//卡车列表
	public ObservableCollection<AccessoryVehicle> mVehiclesETS2 = [];
	public ObservableCollection<AccessoryVehicle> VehiclesETS2 => mVehiclesETS2;
	public ObservableCollection<AccessoryVehicle> mVehiclesATS = [];
	public ObservableCollection<AccessoryVehicle> VehiclesATS => mVehiclesATS;
}
