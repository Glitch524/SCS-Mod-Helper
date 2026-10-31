using SCS_Mod_Helper.Modding.Accessories.Physics;
using System.Collections.ObjectModel;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon;
public class AccessoryAddonData: AccessoryIntData {

	private string mModelPath = "";
	public string ModelPath {
		get => mModelPath;
		set {
			mModelPath = value;
			InvokeChange(nameof(ModelPath));
		}
	}

	private string mModelPathUK = "";
	public string ModelPathUK {
		get => mModelPathUK;
		set {
			mModelPathUK = value;
			InvokeChange(nameof(ModelPathUK));
		}
	}

	private string mExtModelPath = "";
	public string ExtModelPath {
		get => mExtModelPath;
		set {
			mExtModelPath = value;
			InvokeChange(nameof(ExtModelPath));
		}
	}

	private string mExtModelPathUK = "";
	public string ExtModelPathUK {
		get => mExtModelPathUK;
		set {
			mExtModelPathUK = value;
			InvokeChange(nameof(ExtModelPathUK));
		}
	}

	private string mModelType = "";
	public string ModelType {
		get => mModelType;
		set {
			mModelType = value;
			InvokeChange(nameof(ModelType));
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
			InvokeChange(nameof(HideIn));
		}
	}

	//卡车列表
	public ObservableCollection<AccessoryVehicle> mVehiclesETS2 = [];
	public ObservableCollection<AccessoryVehicle> VehiclesETS2 => mVehiclesETS2;
	public ObservableCollection<AccessoryVehicle> mVehiclesATS = [];
	public ObservableCollection<AccessoryVehicle> VehiclesATS => mVehiclesATS;
}
