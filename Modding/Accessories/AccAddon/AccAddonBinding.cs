using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Locale.StringRes;
using SCS_Mod_Helper.Modding.Accessories.AccAddon.Popup;
using SCS_Mod_Helper.Modding.Accessories.Physics;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon;
public partial class AccAddonBinding(Func<IBaseVisual> visualFunc): WindowBinding(visualFunc), IListDataInterface {
	private readonly AccessoryAddonData mAddonItem = new();
	public AccessoryAddonData AddonItem => mAddonItem;


	private bool mLoading = true;
	public bool Loading {
		get => mLoading;
		set {
			mLoading = value;
			InvokeChange();
		}
	}

	public void LoadHistory() {
		DisplayName = AccAddonHistory.Default.DisplayName;
		ModelName = AccAddonHistory.Default.ModelName;
		PartType = AccAddonHistory.Default.PartType;
		PriceString = AccAddonHistory.Default.Price;
		UnlockLevelString = AccAddonHistory.Default.UnlockLevel;
		IconName = AccAddonHistory.Default.IconName;
		ModelPath = AccAddonHistory.Default.ModelPath;
		ModelPathUK = AccAddonHistory.Default.ModelPathUK;
		ExtModelPath = AccAddonHistory.Default.ExtModelPath;
		ExtModelPathUK = AccAddonHistory.Default.ExtModelPathUK;
		CollPath = AccAddonHistory.Default.CollisionPath;
		ModelType = AccAddonHistory.Default.ModelType;
		Look = AccAddonHistory.Default.Look;
		Variant = AccAddonHistory.Default.Variant;
		ElectricType = AccAddonHistory.Default.ElectricType;
		LoadList(AccAddonHistory.Default.DataList, Data);
		if (Data.Count > 0)
			InvokeChange(nameof(DataListContent));
		LoadList(AccAddonHistory.Default.SuitableForList, SuitableFor);
		if (SuitableFor.Count > 0)
			InvokeChange(nameof(SuitableForListContent));
		LoadList(AccAddonHistory.Default.ConflictWithList, ConflictWith);
		if (ConflictWith.Count > 0)
			InvokeChange(nameof(ConflictWithListContent));
		LoadList(AccAddonHistory.Default.DefaultsList, Defaults);
		if (Defaults.Count > 0)
			InvokeChange(nameof(DefaultsListContent));
		LoadList(AccAddonHistory.Default.OverridesList, Overrides);
		if (Overrides.Count > 0)
			InvokeChange(nameof(OverridesListContent));
		LoadList(AccAddonHistory.Default.RequireList, Require);
		if (Require.Count > 0)
			InvokeChange(nameof(RequireListContent));
		ReadVehiclesHistory(AccAddonHistory.Default.VehiclesETS2, VehiclesETS2);
		ReadVehiclesHistory(AccAddonHistory.Default.VehiclesATS, VehiclesATS);
	}
	private static void LoadList(StringCollection? strings, ObservableCollection<string> list) {
		if (strings == null)
			return;
		list.Clear();
		foreach (var s in strings) {
			if (s == null)
				continue;
			list.Add(s);
		}
	}

	protected static int ReadVehiclesHistory(StringCollection? historyList, Collection<AccessoryVehicle> vehicles) {
		Dictionary<string, AccessoryVehicle> vehicleDict = vehicles.ToDictionary(t => t.VehicleID);
		int selected = 0;
		if (historyList == null)
			return selected;
		foreach (var h in historyList) {
			if (h == null)
				continue;
			string[] split = h.Split(",");
			string vehicleID = split[1];
			if (vehicleDict.TryGetValue(vehicleID, out var vehicle)) {
				vehicle.Check = bool.Parse(split[0]);
				if (vehicle.Check)
					selected++;
				vehicle.ModelType = split[2];
				vehicle.Look = split[3];
				vehicle.Variant = split[4];
			}
		}
		return selected;
	}

	public void SaveHistory() {
		AccAddonHistory.Default.DisplayName = DisplayName;
		AccAddonHistory.Default.ModelName = ModelName;
		AccAddonHistory.Default.PartType = PartType;
		AccAddonHistory.Default.Price = PriceString;
		AccAddonHistory.Default.UnlockLevel = UnlockLevelString;
		AccAddonHistory.Default.IconName = IconName;
		AccAddonHistory.Default.ModelPath = ModelPath;
		AccAddonHistory.Default.ModelPathUK = ModelPathUK;
		AccAddonHistory.Default.ExtModelPath = ExtModelPath;
		AccAddonHistory.Default.ExtModelPathUK = ExtModelPathUK;
		AccAddonHistory.Default.CollisionPath = CollPath;
		AccAddonHistory.Default.ModelType = ModelType;
		AccAddonHistory.Default.Look = Look;
		AccAddonHistory.Default.Variant = Variant;
		AccAddonHistory.Default.ElectricType = ElectricType;
		var dataList = Data.ToArray();
		var suitableForList = SuitableFor.ToArray();
		var conflictWithList = ConflictWith.ToArray();
		var defaultsList = Defaults.ToArray();
		var overridesList = Overrides.ToArray();
		var requireList = Require.ToArray();
		var vehicleListETS2 = VehiclesETS2.ToArray();
		var vehicleListATS = VehiclesATS.ToArray();
		Task.Run(() => {
			AccAddonHistory.Default.DataList = ObservableCollectionToStringCollection(dataList);
			AccAddonHistory.Default.SuitableForList = ObservableCollectionToStringCollection(suitableForList);
			AccAddonHistory.Default.ConflictWithList = ObservableCollectionToStringCollection(conflictWithList);
			AccAddonHistory.Default.DefaultsList = ObservableCollectionToStringCollection(defaultsList);
			AccAddonHistory.Default.OverridesList = ObservableCollectionToStringCollection(overridesList);
			AccAddonHistory.Default.RequireList = ObservableCollectionToStringCollection(requireList);
			AccAddonHistory.Default.VehiclesETS2 = VehiclesToStrings(vehicleListETS2);
			AccAddonHistory.Default.VehiclesATS = VehiclesToStrings(vehicleListATS);
			AccAddonHistory.Default.Save();
		});
	}
	private static StringCollection ObservableCollectionToStringCollection(string[] collection) {
		var strings = new StringCollection();
		strings.AddRange([.. collection]);
		return strings;
	}
	private static StringCollection VehiclesToStrings(AccessoryVehicle[] vehicles) {
		var strings = new StringCollection();
		foreach(var vehicle in vehicles) {
			strings.Add($"{vehicle.Check},{vehicle.VehicleID},{vehicle.ModelType},{vehicle.Look},{vehicle.Variant}");
		}
		return strings;
	}


	public async Task LoadVehicles() {
		var vETS2 = VehiclesIO.LoadAccessoryVehicle(true, VehiclesETS2);
		await vETS2;
		var vATS = VehiclesIO.LoadAccessoryVehicle(false, VehiclesATS);
		await vATS;
	}

	public void ReinitVehicleList() {
		VehiclesIO.ClearVehicleList();
		VehiclesETS2.Clear();
		VehiclesATS.Clear();
		_ = LoadVehicles();
	}

	public static string ProjectLocation => Instances.ProjectLocation;
	public static bool ProjectExist => Directory.Exists(ProjectLocation);

	public string DisplayName {
		get => AddonItem.DisplayName;
		set {
			AddonItem.DisplayName = value;
			InvokeChange();
		}
	}

	public string? LocalizedDisplayName {
		get => AddonItem.LocalizedDisplayName;
		set {
			AddonItem.LocalizedDisplayName = value;
			InvokeChange();
		}
	}

	public string ModelName {
		get => AddonItem.ModelName;
		set {
			AddonItem.ModelName = value;
			InvokeChange();

			InvokeChange(nameof(NameOver12));
		}
	}

	public bool NameOver12 => ModelName.Length > 12;

	public string PartType {
		get => AddonItem.PartType;
		set {
			AddonItem.PartType = value;
			InvokeChange();
		}
	}

	public long? Price {
		get => AddonItem.Price;
		set {
			AddonItem.Price = value;
			InvokeChange();
			InvokeChange(nameof(PriceString));
		}
	}
	public string PriceString {
		get => AddonItem.PriceString;
		set {
			AddonItem.PriceString = value;
			InvokeChange();
			InvokeChange(nameof(Price));
		}
	}

	public uint? UnlockLevel {
		get => AddonItem.UnlockLevel;
		set {
			AddonItem.UnlockLevel = value;
			InvokeChange();
			InvokeChange(nameof(UnlockLevelString));
		}
	}
	public string UnlockLevelString {
		get => AddonItem.UnlockLevelString;
		set {
			AddonItem.UnlockLevelString = value;
			InvokeChange();
			InvokeChange(nameof(UnlockLevel));
		}
	}

	public string IconName {
		get => AddonItem.IconName;
		set {
			AddonItem.IconName = value;
			InvokeChange();
			InvokeChange(nameof(IconNameClearVisibility));
		}
	}
	public Visibility IconNameClearVisibility => IconName.Length > 0 ? Visibility.Visible : Visibility.Collapsed;


	public BitmapSource? ModelIcon {
		get => AddonItem.ModelIcon;
		set {
			AddonItem.ModelIcon = value;
			InvokeChange();
		}
	}

	public string ModelPath {
		get => AddonItem.ModelPath;
		set {
			AddonItem.ModelPath = value;
			InvokeChange();

			InvokeChange(nameof(ModelPathClearVisibility));
		}
	}
	public Visibility ModelPathClearVisibility => ModelPath.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

	public string ModelPathUK {
		get => AddonItem.ModelPathUK;
		set {
			AddonItem.ModelPathUK = value;
			InvokeChange();
			InvokeChange(nameof(ModelPathUKClearVisibility));
		}
	}
	public Visibility ModelPathUKClearVisibility => ModelPathUK.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

	private bool? mExteriorPath = null;
	public bool EnableExterior {
		get {
			mExteriorPath ??= ExtModelPath.Length > 0 || ExtModelPathUK.Length > 0;
			return (bool)mExteriorPath;
		}
		set {
			mExteriorPath = value;
			InvokeChange();
		}
	}

	public string ExtModelPath {
		get => AddonItem.ExtModelPath;
		set {
			AddonItem.ExtModelPath = value;
			InvokeChange();

			InvokeChange(nameof(EnableExterior));
			InvokeChange(nameof(ExtModelPathClearVisibility));
		}
	}
	public Visibility ExtModelPathClearVisibility => ExtModelPath.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

	public string ExtModelPathUK {
		get => AddonItem.ExtModelPathUK;
		set {
			AddonItem.ExtModelPathUK = value;
			InvokeChange();

			InvokeChange(nameof(EnableExterior));
			InvokeChange(nameof(ExtModelPathUKClearVisibility));
		}
	}
	public Visibility ExtModelPathUKClearVisibility => ExtModelPathUK.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

	public string CollPath {
		get => AddonItem.CollPath;
		set {
			AddonItem.CollPath = value;
			InvokeChange();

			UseCollPath = value.Length > 0;

			InvokeChange(nameof(CollPathClearVisibility));
		}
	}
	public Visibility CollPathClearVisibility => CollPath.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

	private bool mUseCollPath = false;
	public bool UseCollPath {
		get => mUseCollPath;
		set {
			mUseCollPath = value;
			InvokeChange();
		}
	}

	public string ModelType {
		get => AddonItem.ModelType;
		set {
			AddonItem.ModelType = value;
			if (CurrentModelType != null && value != CurrentModelType.Accessory) {
				mCurrentModelType = null;
			}
			InvokeChange();

			bool? flag = null;
			if (value == $"{AccessoryType.TypeFlagL}/{AccessoryType.TypeFlagFL}") {
				flag = true;
			} else if (value == $"{AccessoryType.TypeFlagR}/{AccessoryType.TypeFlagFR}") {
				flag = false;
			}
			if (flag != null) {
				var result = MessageBox.Show(Util.GetString("MessageModelTypeFlag"), Util.GetString("MessageTitleNotice"), MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes) {
					if (flag == true) {
						ModelPath = "/vehicle/truck/upgrade/flag/flag_left.pmd";
						CollPath = "/vehicle/truck/upgrade/flag/flag_left.pmc";
					} else if (flag == false) {
						ModelPath = "/vehicle/truck/upgrade/flag/flag_right.pmd";
						CollPath = "/vehicle/truck/upgrade/flag/flag_right.pmc";
					}
				}
			}
		}
	}

	//配件类型、look、variant列表以及菜单
	private ObservableCollection<AccessoryType>? mModelTypes = null;
	public ObservableCollection<AccessoryType> ModelTypes {
		get {
			mModelTypes ??= AccessoryType.ModelTypes;
			return mModelTypes;
		}
	}

	private readonly AccessoryType emptyAcc = new();

	public AccessoryType? mCurrentModelType = null;
	public AccessoryType CurrentModelType {
		get {
			if (mCurrentModelType == null) {
				foreach (var type in ModelTypes) {
					if (ModelType == type.Accessory) {
						mCurrentModelType = type;
						return mCurrentModelType;
					}
				}
				return emptyAcc;
			}
			return mCurrentModelType;
		}
		set {
			mCurrentModelType = value ?? emptyAcc;
			InvokeChange();
		}
	}

	public ObservableCollection<string> LookList => AddonItem.LookList;
	public ObservableCollection<string> VariantList => AddonItem.VariantList;
	public string Look {
		get => AddonItem.Look;
		set {
			AddonItem.Look = value;
			InvokeChange();
		}
	}
	public string Variant {
		get => AddonItem.Variant;
		set {
			AddonItem.Variant = value;
			InvokeChange();
		}
	}

	public List<PhysicsData> PhysicsList => AddonItem.PhysicsList;

	public void LoadLooksAndVariants(string? path = null) {
		try {
			string oldLook = Look, oldVariant = Variant;
			if (path == null) {
				if (ModelPath.Length > 0)
					path = ModelPath;
				else if (ModelPathUK.Length > 0)
					path = ModelPathUK;
				else
					return;
				path = path.Replace('/', '\\');
				if (path.Length < 4)
					return;
				path = path[..^4] + ".pit";
				path = ProjectLocation + path;
			} else if (path.EndsWith(".pim")) {
				if (path.Length < 4)
					return;
				path = path[..^4] + ".pit";
			}
			Application.Current.Dispatcher.Invoke(() => {
				LookList.Clear();
				VariantList.Clear();
			}, DispatcherPriority.DataBind);

			AccSCSIO.ReadLookAndVariant(path, LookList, VariantList);

			static void setValue(ObservableCollection<string> list, string oldValue, Action<string> set) {
				if (list.Count > 0) {
					if (list.Contains(oldValue)) {
						set(oldValue);
					} else
						set(list[0]);
				}
			}
			setValue(LookList, oldLook, (set) => Look = set);
			setValue(VariantList, oldVariant, (set) => Variant = set);
		} catch (Exception ex) {
			MessageBox.Show(BindingWindow, Util.GetString("MessageLoadDEDErrFail") + "\n" + ex.Message);
		}
	}



	private bool othersChecked = false;
	public bool OthersChecked {
		get => othersChecked;
		set {
			othersChecked = value;
			InvokeChange();
		}
	}
	public void UpdateOthersChecked() {
		OthersChecked = HideIn != 0
			|| ElectricType != "vehicle"
			|| Data.Count > 0
			|| SuitableFor.Count > 0
			|| ConflictWith.Count > 0
			|| Defaults.Count > 0
			|| Overrides.Count > 0
			|| Require.Count > 0;
	}

	public uint HideIn {
		get => AddonItem.HideIn;
		set {
			AddonItem.HideIn = value;
			InvokeChange(nameof(HideInString));
		}
	}

	public string HideInString => $"0x{Convert.ToInt64(HideIn):X}";

	public HideInUC? HideInUC = null;

	public string ElectricType {
		get => AddonItem.ElectricType;
		set {
			AddonItem.ElectricType = value;
			InvokeChange();
		}
	}

	public ObservableCollection<string> Data => AddonItem.Data;
	public ObservableCollection<string> SuitableFor => AddonItem.SuitableFor;
	public ObservableCollection<string> ConflictWith => AddonItem.ConflictWith;
	public ObservableCollection<string> Defaults => AddonItem.Defaults;
	public ObservableCollection<string> Overrides => AddonItem.Overrides;
	public ObservableCollection<string> Require => AddonItem.Require;

	private string mNewData = "";
	public string NewData {
		get {
			return mNewData;
		}
		set {
			mNewData = value;
			InvokeChange();
		}
	}
	private string mNewSuitableFor = "";
	public string NewSuitableFor {
		get => mNewSuitableFor;
		set {
			mNewSuitableFor = value;
			InvokeChange();
		}
	}
	private string mNewConflictWith = "";
	public string NewConflictWith {
		get => mNewConflictWith;
		set {
			mNewConflictWith = value;
			InvokeChange();
		}
	}
	private string mNewDefaults = "";
	public string NewDefaults {
		get => mNewDefaults;
		set {
			mNewDefaults = value;
			InvokeChange();
		}
	}
	private string mNewOverrides = "";
	public string NewOverrides {
		get => mNewOverrides;
		set {
			mNewOverrides = value;
			InvokeChange();
		}
	}
	private string mNewRequire = "";
	public string NewRequire {
		get => mNewRequire;
		set {
			mNewRequire = value;
			InvokeChange();
		}
	}
	public void AddNewData(string? data = null) {
		AddListItem(Data, data ?? NewData);
		NewData = string.Empty;
		InvokeChange(nameof(DataListContent));
	}

	public void AddNewSuitableFor() {
		AddListItem(SuitableFor, NewSuitableFor);
		NewSuitableFor = string.Empty;
		InvokeChange(nameof(SuitableForListContent));
	}

	public void AddNewConflictWith() {
		AddListItem(ConflictWith, NewConflictWith);
		NewConflictWith = string.Empty;
		InvokeChange(nameof(ConflictWithListContent));
	}

	public void AddNewDefaults() {
		AddListItem(Defaults, NewDefaults);
		NewDefaults = string.Empty;
		InvokeChange(nameof(DefaultsListContent));
	}

	public void AddNewOverrides() {
		AddListItem(Overrides, NewOverrides);
		NewOverrides = string.Empty;
		InvokeChange(nameof(OverridesListContent));
	}

	public void AddNewRequire() {
		AddListItem(Require, NewRequire);
		NewRequire = string.Empty;
		InvokeChange(nameof(RequireListContent));
	}

	public static void AddListItem(ObservableCollection<string> list, string newItem) {
		if (newItem.Length == 0 || list.Contains(newItem))
			return;
		list.Add(newItem);
	}
	public string DataListContent => AddonItem.DataListContent;
	public string SuitableForListContent => AddonItem.SuitableForListContent;
	public string ConflictWithListContent => AddonItem.ConflictWithListContent;
	public string DefaultsListContent => AddonItem.DefaultsListContent;
	public string OverridesListContent => AddonItem.OverridesListContent;
	public string RequireListContent => AddonItem.RequireListContent;

	public string? OpeningList {
		get => AddonItem.OpeningList;
		set {
			AddonItem.OpeningList = value;
			InvokeChange(nameof(PopupCollection));
		}
	}

	public ObservableCollection<string>? PopupCollection => AddonItem.PopupCollection;

	public void DeleteItem(string item) {
		PopupCollection?.Remove(item);
		switch (OpeningList) {
			case "TextData":
				InvokeChange(nameof(DataListContent));
				break;
			case "TextSuitableFor":
				InvokeChange(nameof(SuitableForListContent));
				break;
			case "TextConflictWith":
				InvokeChange(nameof(ConflictWithListContent));
				break;
			case "TextDefaults":
				InvokeChange(nameof(DefaultsListContent));
				break;
			case "TextOverrides":
				InvokeChange(nameof(OverridesListContent));
				break;
			case "TextRequire":
				InvokeChange(nameof(RequireListContent));
				break;
		}
	}

	public void UpdateAllContent() {
		InvokeChange(nameof(DataListContent));
		InvokeChange(nameof(SuitableForListContent));
		InvokeChange(nameof(ConflictWithListContent));
		InvokeChange(nameof(DefaultsListContent));
		InvokeChange(nameof(OverridesListContent));
		InvokeChange(nameof(RequireListContent));
	}

	private void ClearAllText() {
		ModelName = "";
		DisplayName = "";
		PriceString = "";
		UnlockLevelString = "";
		IconName = "";
		PartType = "unknown";
		ModelPath = "";
		ModelPathUK = "";
		ExtModelPath = "";
		ExtModelPathUK = "";
		CollPath = "";
		ModelType = "";
		Look = "";
		Variant = "";
		HideIn = 0;
		ElectricType = "vehicle";
		Data.Clear();
		SuitableFor.Clear();
		ConflictWith.Clear();
		Defaults.Clear();
		Overrides.Clear();
		Require.Clear();
	}

	public ListDataUC? ListDataUC = null;


	private bool mVehicleExpandedETS2 = true;
	public bool VehicleExpandedETS2 {
		get => mVehicleExpandedETS2;
		set {
			mVehicleExpandedETS2 = value;
			InvokeChange();

			PopupAddVehicleOpen = false;
		}
	}
	private bool mVehicleExpandedATS = true;
	public bool VehicleExpandedATS {
		get => mVehicleExpandedATS;
		set {
			mVehicleExpandedATS = value;
			InvokeChange();

			PopupAddVehicleOpen = false;
		}
	}

	private bool mPopupAddVehicleOpen = false;
	public bool PopupAddVehicleOpen {
		get => mPopupAddVehicleOpen;
		set {
			mPopupAddVehicleOpen = value;
			InvokeChange();
		}
	}

	public bool? SelectAllETS2 {
		get {
			if (SelectedCountETS2 == 0)
				return false;
			else if (SelectedCountETS2 == VehiclesETS2.Count)
				return true;
			else
				return null;
		}
		set {
			if (SelectedCountETS2 == VehiclesETS2.Count) {
				SelectedCountETS2 = 0;
				SelectAllVehicle(VehiclesETS2, false);
			} else {
				SelectedCountETS2 = VehiclesETS2.Count;
				SelectAllVehicle(VehiclesETS2, true);
			}
			InvokeChange();
		}
	}
	private int mSelectedCountETS2 = 0;
	public int SelectedCountETS2 {
		get => mSelectedCountETS2;
		set {
			mSelectedCountETS2 = value;

			InvokeChange(nameof(SelectAllETS2));
		}
	}

	public bool? SelectAllATS {
		get {
			if (SelectedCountATS == 0)
				return false;
			else if (SelectedCountATS == VehiclesATS.Count)
				return true;
			else
				return null;
		}
		set {
			if (SelectedCountATS == VehiclesATS.Count) {
				SelectedCountATS = 0;
				SelectAllVehicle(VehiclesATS, false);
			} else {
				SelectedCountATS = VehiclesATS.Count;
				SelectAllVehicle(VehiclesATS, true);
			}
			InvokeChange();
		}
	}

	private int mSelectedCountATS = 0;
	public int SelectedCountATS {
		get => mSelectedCountATS;
		set {
			mSelectedCountATS = value;

			InvokeChange(nameof(SelectAllATS));
		}
	}

	public void SetSelected(AccessoryVehicle vehicle) {
		bool check = vehicle.Check;
		if (vehicle.IsETS2) {
			if (check)
				SelectedCountETS2++;
			else
				SelectedCountETS2--;
		} else {
			if (check)
				SelectedCountATS++;
			else
				SelectedCountATS--;
		}
	}

	public void SelectAllVehicle(ObservableCollection<AccessoryVehicle> vehicles, bool check) {
		foreach (AccessoryVehicle vehicle in vehicles) {
			vehicle.Check = check;
			OverwriteEmpty(vehicle, check);
		}
	}

	public void OverwriteEmpty(AccessoryVehicle vehicle, bool check) {
		if (check) {
			if (vehicle.ModelType.Length == 0)
				vehicle.ModelType = ModelType;
			if (vehicle.Look.Length == 0)
				vehicle.Look = Look;
			if (vehicle.Variant.Length == 0)
				vehicle.Variant = Variant;
		}
	}

	public ObservableCollection<AccessoryVehicle> VehiclesETS2 => AddonItem.VehiclesETS2;
	public ObservableCollection<AccessoryVehicle> VehiclesATS => AddonItem.VehiclesATS;

	public ICommand SaveDEDCommand => new RelayCommand(SaveDED);
	string? LoadedFilename = null;
	public void SaveDED() {
		PopupAddVehicleOpen = false;
		SaveFileDialog saveFileDialog = new() {
			Title = Util.GetString("SaveDED"),
			AddExtension = true,
			DefaultDirectory = Paths.DefaultDEDDir(),
			InitialDirectory = GetDEDInitDir(),
			DefaultExt = "ded",
			Filter = Util.GetFilter("DialogFilterDED"),
		};
		if (LoadedFilename == null) {
			if (LocalizedDisplayName == null) {
				StringResUtil.GetStringResResults(DisplayName, out var localizedName);
				LocalizedDisplayName = localizedName;
			}
			saveFileDialog.FileName = $"{LocalizedDisplayName} {ModelType} {ModelName}";
		} else
			saveFileDialog.FileName = LoadedFilename;
		if (saveFileDialog.ShowDialog() == true) {
			SaveDedLocation(saveFileDialog.FileName);

			new AddonAppIO(saveFileDialog.FileName).SaveAddon(this);
			MessageBox.Show(BindingWindow, Util.GetString("MessageSaveDED"));
		}
	}

	public ICommand LoadDEDCommand => new RelayCommand(LoadDED);
	public void LoadDED() {
		try {
			PopupAddVehicleOpen = false;
			OpenFileDialog openFileDialog = new() {
				Title = Util.GetString("LoadDED"),
				Multiselect = false,
				AddExtension = true,
				DefaultDirectory = Paths.DefaultDEDDir(),
				InitialDirectory = GetDEDInitDir(),
				DefaultExt = "ded",
				Filter = Util.GetFilter("DialogFilterDED"),
			};
			if (openFileDialog.ShowDialog() == true) {
				SaveDedLocation(openFileDialog.FileName);
				LoadedFilename = openFileDialog.SafeFileName;

				ClearAllText();

				new AddonAppIO(openFileDialog.FileName).LoadAddon(this);
				UpdateOthersChecked();
			}
			LoadLooksAndVariants();
		} catch (Exception ex) {
			MessageBox.Show(BindingWindow, Util.GetString("MessageLoadDEDErrFail") + "\n" + ex.Message);
		}
	}

	private static string GetDEDInitDir() {
		var dedPath = AccAddonHistory.Default.DEDLocation;
		if (dedPath.Length == 0) {
			dedPath = Paths.DefaultDEDDir();
			Directory.CreateDirectory(dedPath);
		}
		return dedPath;
	}

	private static void SaveDedLocation(string filename) {
		var dir = new DirectoryInfo(filename).Parent!.FullName;
		if (dir == Paths.DefaultDEDDir()) {
			AccAddonHistory.Default.DEDLocation = "";
		} else {
			AccAddonHistory.Default.DEDLocation = dir;
		}
		AccAddonHistory.Default.Save();
	}

	public static void DeleteVehicle(bool ets2, ObservableCollection<AccessoryVehicle> Vehicles, DataGrid Table) {
		List<string> vehicleIDs = [];
		while (Table.SelectedIndex != -1) {
			var selected = Vehicles[Table.SelectedIndex];
			Vehicles.Remove(selected);
			vehicleIDs.Add(selected.VehicleID);
		}
		Table.UnselectAll();
		if (vehicleIDs.Count > 0)
			VehiclesIO.DeleteVehicle(ets2, vehicleIDs);
	}


	private bool mDeleteUnchecked;
	public bool DeleteUnchecked {
		get => mDeleteUnchecked;
		set {
			mDeleteUnchecked = value;
			InvokeChange();
		}
	}

	public ICommand StartCommand => new RelayCommand(StartCreateSii);
	public void StartCreateSii() {
		try {
			PopupAddVehicleOpen = false;
			if (!VehicleExpandedETS2 && !VehicleExpandedATS)
				throw new(Util.GetString("MessageCreateSiiNoExpanded"));
			if (ProjectLocation.Length == 0 ||
				DisplayName.Length == 0 ||
				Price == null ||
				UnlockLevel == null ||
				ModelPath.Length == 0 ||
				ModelName.Length == 0 ||
				IconName.Length == 0) {
				throw new(Util.GetString("MessageCreateSiiNotFilled"));
			}
			try {
				if (Price == 0)
					throw new(Util.GetString("ModelPriceErrNot0"));
			} catch {
				throw new(Util.GetString("ModelPriceErrNoNumber"));
			}
			var created = AccAddonSCSIO.CreateAccAddonSii(this);
			MessageBox.Show(BindingWindow, Util.GetString(created == 0 ? "MessageCreateSiiZero" : "MessageCreateSiiResult"));
		} catch (Exception ex) {
			MessageBox.Show(BindingWindow, ex.Message);
		}
	}

	public void ChooseIcon() => AddonItem.ChooseIcon(BindingWindow);


	public const int MODEL = 0;
	public const int MODEL_UK = 1;
	public const int EXT_MODEL = 2;
	public const int EXT_MODEL_UK = 3;
	public const int MODEL_COLL = 4;
	//模型、模型UK、碰撞体
	public void ChooseModel(int type) {
		if (ProjectLocation.Length == 0)
			throw new(Util.GetString("MessageProjectLocationFirst"));
		string title, filter, defaultExt;
		switch (type) {
			case MODEL:
			case EXT_MODEL:
				title = Util.GetString("DialogTitleChooseModel");
				filter = Util.GetFilter("DialogFilterChooseModel");
				defaultExt = "pmd";
				break;
			case MODEL_UK:
			case EXT_MODEL_UK:
				title = Util.GetString("DialogTitleChooseModelUK");
				filter = Util.GetFilter("DialogFilterChooseModel");
				defaultExt = "pmd";
				break;
			case MODEL_COLL:
				title = Util.GetString("DialogTitleChooseColl");
				filter = Util.GetFilter("DialogFilterChooseColl");
				defaultExt = "pmc";
				break;
			default:
				return;
		}
		var fileDialog = new OpenFileDialog {
			Multiselect = false,
			DefaultDirectory = ProjectLocation,
			DefaultExt = defaultExt,
			Title = title,
			Filter = filter,
			InitialDirectory = AccessoryDataUtil.GetInitialPath([ModelPath, ModelPathUK, CollPath])
		};
		if (fileDialog.ShowDialog() != true)
			return;
		string path = fileDialog.FileName;
		if (!path.StartsWith(ProjectLocation) || path.Length == ProjectLocation.Length)
			throw new(Util.GetString("MessageModelOutsideProject"));
		if (type == MODEL_COLL) {
			if (!path.EndsWith(".pic") && !path.EndsWith(".pmc"))
				throw new(Util.GetString("MessageInvalidExtColl"));
		} else if (!path.EndsWith(".pim") && !path.EndsWith(".pmd")) {
			throw new(Util.GetString("MessageInvalidExt"));
		}
		AccAddonHistory.Default.ChooseModelHistory = new DirectoryInfo(fileDialog.FileName).Parent!.FullName;
		AccAddonHistory.Default.Save();
		LoadLooksAndVariants(path);//修改模型路径后，look和variant都不同，需要重新读取
		string inProjectPath = path.Replace(ProjectLocation, "");
		var s = inProjectPath.Split('\\');

		string?
			modelName = null,
			modelType = null,
			modelPath = null,
			modelPathUK = null,
			collPath = null;

		for (int i = s.Length - 2; i > 0; i--) {
			foreach (var info in ModelTypes) {//根据路径猜测模型的类型
				if (s[i] == info.AccessoryETS2 || s[i] == info.AccessoryATS)
					modelType = info.Accessory;
			}
		}
		if (type == MODEL || type == EXT_MODEL) {
			modelPath = inProjectPath.Replace('\\', '/');
			modelPath = modelPath[..^4];
			modelName = modelPath[(modelPath.LastIndexOf('/') + 1)..];
			if (File.Exists(path[..^4] + "_uk.pim") || File.Exists(path[..^4] + "_uk.pmd"))
				modelPathUK = modelPath + "_uk.pmd";
			if (File.Exists(path[..^4] + ".pic") || File.Exists(path[..^4] + ".pmc"))
				collPath = modelPath + ".pmc";
			modelPath += ".pmd";
		} else if (type == MODEL_UK || type == EXT_MODEL_UK) {
			modelPathUK = inProjectPath.Replace('\\', '/');
			modelPathUK = modelPathUK[..^4];
			if (modelPathUK.EndsWith("_uk")) {
				modelName = modelPathUK[(modelPathUK.LastIndexOf('/') + 1)..^3];
				if (File.Exists(path[..^7] + ".pim") || File.Exists(path[..^7] + ".pmd"))
					modelPath = modelPathUK[..^3] + ".pmd";
				if (File.Exists(path[..^7] + ".pic") || File.Exists(path[..^7] + ".pmc"))
					collPath = modelPathUK[..^3] + ".pmc";
			}
			modelPathUK += ".pmd";
		} else if (type == MODEL_COLL) {
			collPath = inProjectPath.Replace('\\', '/');
			collPath = collPath[..^4];
			modelName = collPath[(collPath.LastIndexOf('/') + 1)..];
			if (File.Exists(path[..^4] + "_uk.pim") || File.Exists(path[..^4] + "_uk.pmd"))
				modelPathUK = modelPath + "_uk.pmd";
			if (File.Exists(path[..^4] + ".pim") || File.Exists(path[..^4] + ".pmd"))
				modelPath = collPath + ".pmd";
			collPath += ".pmc";
		}
		bool showAR;


		if (ModelName == modelName)
			modelName = null;
		if (ModelType == modelType)
			modelType = null;
		if (type == MODEL) {
			ModelPath = modelPath!;
			if (ModelPathUK == modelPathUK)
				modelPathUK = null;

			if (CollPath == collPath)
				collPath = null;
			showAR = modelName != null || modelType != null || modelPathUK != null || collPath != null;
		} else if (type == EXT_MODEL) {
			ExtModelPath = modelPath!;
			if (ExtModelPathUK == modelPathUK)
				modelPathUK = null;

			if (CollPath == collPath)
				collPath = null;
			showAR = modelName != null || modelType != null || modelPathUK != null || collPath != null;
		} else if (type == MODEL_UK) {
			ModelPathUK = modelPathUK!;
			if (ModelPath == modelPath)
				modelPath = null;

			if (CollPath == collPath)
				collPath = null;
			showAR = modelName != null || modelType != null || modelPath != null || collPath != null;
		} else if (type == EXT_MODEL_UK) {
			ExtModelPathUK = modelPathUK!;
			if (ExtModelPath == modelPath)
				modelPath = null;

			if (CollPath == collPath)
				collPath = null;
			showAR = modelName != null || modelType != null || modelPath != null || collPath != null;
		} else if (type == MODEL_COLL) {
			CollPath = collPath!;
			if (ModelPath == modelPath)
				modelPath = null;
			if (ModelPathUK == modelPathUK)
				modelPathUK = null;
			showAR = modelName != null || modelType != null || modelPath != null || modelPathUK != null;
		} else
			return;
		if (showAR) {
			AutoFillSelectionWindow arWindow = new(type, modelName, modelType, modelPath, modelPathUK, collPath) { Owner = BindingWindow };
			var result = arWindow.ShowDialog();
			if (result == true) {
				if (modelName != null && arWindow.CheckModelName)
					ModelName = modelName;
				if (modelType != null && arWindow.CheckModelType) {
					mCurrentModelType = null;
					ModelType = modelType;
				}
				if (modelPath != null && arWindow.CheckModelPath) {
					if (type == EXT_MODEL || type == EXT_MODEL_UK)
						ExtModelPath = modelPath;
					else
						ModelPath = modelPath;
				}
				if (modelPathUK != null && arWindow.CheckModelPathUK) {
					if (type == EXT_MODEL || type == EXT_MODEL_UK)
						ExtModelPathUK = modelPathUK;
					else
						ModelPathUK = modelPathUK;
				}
				if (collPath != null && arWindow.CheckCollPath)
					CollPath = collPath;
			}
		}
	}
}
