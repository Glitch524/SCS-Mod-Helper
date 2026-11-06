using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
		"Cabin",
		"cabin",
		"r_deflector",
		"s_deflector",
		"cabdefl",
		"filter",
		"sunshield",
		"sunshld",
		"s_guard",
		"cor_def",
		"deflector",
		"f_light",
		"mirror",
		"f_mirror",
		"s_mirror",
		"fl_mirror",
		"fr_mirror",
		"f_equip",
		"f_logo",
		"hl_guard",
		"hood",
		"f_bumper",
		"bumper",
		"airdam",
		"f_grill",
		"b_grill",
		"r_grill",
		"grill",
		"f_fender_chs",
		"f_fender",
		"fenders",
		"f_mudflap",
		"doorhndl",
		"exhaust_m",
		"exhaust",
		"doorstep",
		"s_strip",
		"sideskirt",
		"sideskrt_bar",
		"tank",
		"r_chs_cover",
		"r_panel",
		"r_fender",
		"r_fendr_top",
		"r_light",
		"r_bumper",
		"r_mudflap",
		"mudflaps",
	];

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
	public string GroupTypeLocalized { get; } = VehiclesIO.GetPartName(groupType.ToLower());

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
			InvokeVisible = UVCheck
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
	public int index = -1;
	public readonly string VehicleID = vehicleID;
	public bool IsCabin = isCabin;


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

	public string LayerName {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = layerName;

	public string LayerNameLocalized { get; } = isCabin ? VehiclesIO.GetCabinName(layerName) : VehiclesIO.GetAccessoryName(vehicleID, layerName);

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