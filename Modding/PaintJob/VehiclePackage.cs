using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Vehicles;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Modding.PaintJob;

public class VehiclePackage(string vehicleID): BaseBinding {

	//todo 车辆模型
	public string VehicleID { get; } = vehicleID;

	public List<LayerGroup> LayerGroups { get; } = [];
	public LayerGroup? this[int index] {
		get => LayerGroups[index];
		set {
			LayerGroups[index] = value!;
			InvokeChange();
		}
	}
	public void Add(LayerGroup group) => LayerGroups.Add(group);

	public List<VehicleUV> AllUVs { get; } = [];
}

public class LayerGroup(string groupType): BaseBinding {

	public string GroupType { get; } = groupType;

	public string GroupTypeLocalized { get; } = VehiclesIO.GetPartName(groupType.ToLower());

	public List<VehicleUV> UVLayers { get; } = [];

	public VehicleUV? this[int index] {
		get => UVLayers[index];
		set {
			UVLayers[index] = value!;
			InvokeChange();
		}
	}

	public void Add(VehicleUV uv) {
		uv.InvokeVisible = UVCheck;
		uv.index = UVLayers.Count;
		UVLayers.Add(uv);
	}

	int VisibleIndex = -1;

	private void UVCheck(int index, bool visible) {
		if (visible) {
			if(VisibleIndex == -1) {
				VisibleIndex = index;
			} else {
				UVLayers[VisibleIndex].Visible = false;
				VisibleIndex = index;
			}
		} else {
			if (VisibleIndex == index) {
				VisibleIndex = -1;
			}
		}
	}
}

public class VehicleUV(string vehicleID, bool isCabin, string layerName, BitmapImage layer): BaseBinding {
	public int index = -1;

	public Action<int, bool>? InvokeVisible = null;

	public bool Visible {
		get;
		set {
			field = value;
			InvokeVisible?.Invoke(index, value);
			InvokeChange();
			InvokeChange(nameof(UVVisibility));
		}
	} = false;

	public Visibility UVVisibility => Visible ? Visibility.Visible : Visibility.Collapsed;

	public string VehicleID = vehicleID;
	public bool IsCabin = isCabin;

	public string LayerName {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = layerName;

	public string LayerNameLocalized { get; } = isCabin? VehiclesIO.GetCabinName(layerName) : VehiclesIO.GetAccessoryName(vehicleID, layerName);

	public BitmapImage Layer {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = layer;
}