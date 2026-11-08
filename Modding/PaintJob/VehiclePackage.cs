using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Modding.PaintJob;

public class VehiclePackage(string vehicleID): BaseBinding {

	//todo 车辆模型
	public string VehicleID { get; } = vehicleID;

	public ObservableCollection<LayerGroup> LayerGroups { 
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = [];
	public LayerGroup? this[int index] {
		get => LayerGroups[index];
		set {
			LayerGroups[index] = value!;
			InvokeChange();
		}
	}
	public LayerGroup Add(string groupType) {
		var group = new LayerGroup(VehicleID, groupType, AllUVs.Add);
		LayerGroups.Add(group);
		return group;
	}

	readonly string[] GroupOrders = [
		"Cabin", "cabin", "airdam", "roof_addon", "roof_window", "sunshield", "sunshld", "f_window", "frntglss_mid",
		"f_wnd_frame", "windows", "hood", "deflector", "cabdefl", "cor_def", "r_deflector", "s_deflector", "headache_r",
		"sideflares", "cabflares", "r_panel", "s_panel", "rear_cab", "rear2_cab", "rear_window", "r_chs_cover", "s_vent",
		"slp_vent", "cab_door", "doorhndl", "slp_hndl", "cab_doorstep", "doorstep", "rearstep", "hndrail", "f_cab_trim",
		"windowtrim", "doortrim", "f_badge", "f_logo", "f_banner", "ornament", "r_badge", "r_logo", "s_badge", "s_logo",
		"decals", "s_reflector", "s_strip", "mirror", "rv_mirror", "s_mirror", "f_mirror", "fl_mirror", "fr_mirror", "grill",
		"c_grill", "b_grill", "f_inlay_cab", "f_inlay_chs", "f_intake_bar", "f_intake_cab", "f_intake_chs", "f_intk_b_cab",
		"f_intk_b_chs", "s_guard", "hl_guard", "bullbar", "f_grill", "f_equip", "bumper", "f_bumper", "r_bumper", "f_light",
		"f_light_chs", "f_light_mid", "f_light_top", "r_grill", "f_turn_light", "r_light", "beacon", "chs_beacon",
		"f_light_bmp", "fenders", "f_fender", "r_fender", "f_fender_cab", "f_fender_chs", "f_fendr_chs", "r_fender_top",
		"r_fendr_top", "mudguard", "lghfender", "mudflaps", "b_mudflap", "f_mudflap", "r_mudflap", "sideskirt",
		"sideskirt_bar", "sideskrt_bar", "tank", "fuelcap", "exhaust", "cab_exhaust", "exhaust_l", "exhaust_r", "exhaust_m",
		"spare_w", "trunk", "bed_addon", "wing", "b_detector", "filter", "filters", "filter_cover", "horns", "l_horn",
		"r_horn", "drv_plate", "codrv_plate", "flag_l", "flag_r", "flag_f_l", "flag_f_r"];

	public void SortGroups() {
		var sorted = LayerGroups.OrderBy(s => {
			int idx = Array.IndexOf(GroupOrders, s.GroupType);
			return idx < 0 ? int.MaxValue : idx;
		});
		LayerGroups = new ObservableCollection<LayerGroup>(sorted);
	}

	public List<VehicleUV> AllUVs { get; } = [];
}

public class LayerGroup(string vehicleID, string groupType, Action<VehicleUV> addUVToAll): BaseBinding {
	private readonly string VehicleID = vehicleID;

	public string GroupType { get; } = groupType;

	private readonly Action<VehicleUV> AddUVToAll = addUVToAll;

	private readonly bool IsCabin = groupType.Equals("cabin", StringComparison.CurrentCultureIgnoreCase);

	private string? mGroupTypeLocalized = null;
	public string GroupTypeLocalized { 
		get {
			if (mGroupTypeLocalized == null) {
				if (IsCabin)
					mGroupTypeLocalized = VehiclesIO.GetAccGroupName(GroupType.ToLower());
				else
					mGroupTypeLocalized = VehiclesIO.GetAccPartName(GroupType.ToLower());
			}
			return mGroupTypeLocalized;
		}
	}

	public List<VehicleUV> UVLayers { get; } = [];

	public VehicleUV? this[int index] {
		get => UVLayers[index];
		set {
			UVLayers[index] = value!;
			InvokeChange();
		}
	}

	public void Add(string layerName, string uvPath) {
		var vuv = new VehicleUV(VehicleID, IsCabin, layerName, uvPath) {
			InvokeVisible = UVCheck,
			Index = UVLayers.Count
		};
		if (IsCabin && UVLayers.Count == 0)
			vuv.Visible = true;
		UVLayers.Add(vuv);
		AddUVToAll(vuv);
	}

	int VisibleIndex = -1;


	private void UVCheck(int index, bool visible) {
		if (visible) {
			if (VisibleIndex == -1) {
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

	public override string ToString() => $"LayerGroup Name={GroupType}";
}

public class VehicleUV(string vehicleID, bool isCabin, string layerName, string uvPath): BaseBinding {
	public int Index = -1;
	public readonly string VehicleID = vehicleID;
	public bool IsCabin = isCabin;


	public Action<int, bool>? InvokeVisible = null;

	public bool Visible {
		get;
		set {
			field = value;
			InvokeVisible?.Invoke(Index, value);
			InvokeChange();
		}
	} = false;

	public string LayerName {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = layerName;

	public string LayerNameLocalized { get; } = isCabin ? VehiclesIO.GetVehicleCabinName(layerName) : VehiclesIO.GetVehicleAccName(vehicleID, layerName);

	private readonly string UVPath = uvPath;
	private bool UVLoaded = false;


	public BitmapImage? mUVLayer = null;
	public BitmapImage? UVLayer {
		get {
			if (mUVLayer == null)
				LoadUVLayer();
			return mUVLayer;
		}
	}

	private void LoadUVLayer() {
		if (UVLoaded)
			return;
		UVLoaded = true;
		var packageFile = Paths.VehiclesPackageDir(VehicleID);
		if (File.Exists(packageFile)) {
			using FileStream fs = new(packageFile, FileMode.Open);
			using ZipArchive pFile = new(fs, ZipArchiveMode.Read);
			var uvEntry = pFile.GetEntry(UVPath);
			if (uvEntry == null)
				return;
			try {
				byte[] data;
				using (var uvStream = uvEntry.Open()) {
					using var ms = new MemoryStream();
					uvStream.CopyTo(ms);
					data = ms.ToArray();
				}
				if (data.Length == 0) {
					throw new NullReferenceException($"Zip entry {UVPath} is empty");
				}
				var bitmap = new BitmapImage();
				bitmap.BeginInit();
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.StreamSource = new MemoryStream(data);
				bitmap.EndInit();
				if (bitmap.CanFreeze)
					bitmap.Freeze();
				mUVLayer = bitmap;
				return;
			} catch (Exception e) {
				Debug.WriteLine(e);
			}
		}
	}
}