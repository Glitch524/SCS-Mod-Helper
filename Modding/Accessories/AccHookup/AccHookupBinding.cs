using Microsoft.Win32;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Modding.Accessories.AccAddon;
using SCS_Mod_Helper.Modding.Accessories.Physics;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Modding.Accessories.AccHookup;

public class AccHookupBinding(Func<IBaseVisual> visualFunc): WindowBinding(visualFunc), IListDataInterface {

	public static string ProjectLocation => Instances.ProjectLocation;

	private string mStorageName = "";
	public string StorageName {
		get => mStorageName;
		set {
			mStorageName = value;
			InvokeChange();
		}
	}

	public readonly ObservableCollection<SuiItem> mSuiItems = [];
	public ObservableCollection<SuiItem> SuiItems => mSuiItems;

	private SuiItem? mCurrentSuiItem;
	public SuiItem? CurrentSuiItem {
		get => mCurrentSuiItem;
		set {
			mCurrentSuiItem = value;
			InvokeChange();

			InvokeChange(nameof(Hookups));
			if (Hookups != null && Hookups.Count > 0)
				CurrentHookupItem = Hookups.FirstOrDefault();

			SuiChanged?.Invoke(value);
		}
	}

	public delegate void OnSuiItemChanged(SuiItem? suiItem);
	public OnSuiItemChanged? SuiChanged = null;

	public ObservableCollection<AccessoryHookupData>? Hookups => CurrentSuiItem?.HookupItems;

	private AccessoryHookupData? mCurrentHookupItem;
	public AccessoryHookupData? CurrentHookupItem {
		get => mCurrentHookupItem;
		set {
			mCurrentHookupItem = value;
			if (value != null && ModelIcon == null) {
				value.ModelIcon = AccessoryDataUtil.LoadModelIconByIconName(value.IconName);
			}
			InvokeChange();

			InvokeChange(nameof(PopupCollection));
			UpdateOthersChecked();
		}
	}
	public string ModelName {
		get => CurrentHookupItem?.ModelName ?? "";
		set {
			if (CurrentHookupItem != null) {
				CurrentHookupItem.ModelName = value;
				InvokeChange();
			}
		}
	}
	public string DisplayName {
		get => CurrentHookupItem?.DisplayName ?? "";
		set {
			if (CurrentHookupItem != null) {
				CurrentHookupItem.DisplayName = value;
				InvokeChange();
			}
		}
	}

	public string IconName {
		get => CurrentHookupItem?.IconName ?? "";
		set {
			if (CurrentHookupItem != null) {
				CurrentHookupItem.IconName = value;
				InvokeChange();
			}
		}
	}

	public BitmapSource? ModelIcon {
		get => CurrentHookupItem?.ModelIcon;
		set {
			if (CurrentHookupItem != null) {
				CurrentHookupItem.ModelIcon = value;
				InvokeChange();
			}
		}
	}

	public string ModelPath {
		get => CurrentHookupItem?.ModelPath ?? "";
		set {
			if (CurrentHookupItem != null) {
				CurrentHookupItem.ModelPath = value;
				InvokeChange(nameof(ModelPath));
			}
		}
	}
	public string CollPath {
		get => CurrentHookupItem?.CollPath ?? "";
		set {
			if (CurrentHookupItem != null) {
				CurrentHookupItem.CollPath = value;
				InvokeChange();
			}
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
	private void UpdateOthersChecked() {
		if (CurrentHookupItem == null)
			return;
		OthersChecked =
			CurrentHookupItem.ElectricType != "vehicle" ||
			CurrentHookupItem.ConflictWith.Count > 0 ||
			CurrentHookupItem.Defaults.Count > 0 ||
			CurrentHookupItem.Overrides.Count > 0 ||
			CurrentHookupItem.Require.Count > 0;
	}

	public ObservableCollection<string>? Data => CurrentHookupItem?.Data;
	public ObservableCollection<string>? SuitableFor => CurrentHookupItem?.SuitableFor;
	public ObservableCollection<string>? ConflictWith => CurrentHookupItem?.ConflictWith;
	public ObservableCollection<string>? Defaults => CurrentHookupItem?.Defaults;
	public ObservableCollection<string>? Overrides => CurrentHookupItem?.Overrides;
	public ObservableCollection<string>? Require => CurrentHookupItem?.Require;

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
		CurrentHookupItem?.InvokeChange(nameof(DataListContent));
	}

	public void AddNewData(PhysicsData? physicsData) {
		if (physicsData == null)
			return;
		var physName = physicsData.PhysicsName;
		if (!physName.EndsWith(AccSCSIO.NamePSuffix))
			physName += AccSCSIO.NamePSuffix;
		AddNewData(physName);
	}

	public void AddNewSuitableFor() {
		AddListItem(SuitableFor, NewSuitableFor);
		NewSuitableFor = string.Empty;
		CurrentHookupItem?.InvokeChange(nameof(SuitableForListContent));
	}

	public void AddNewConflictWith() {
		AddListItem(ConflictWith, NewConflictWith);
		NewConflictWith = string.Empty;
		CurrentHookupItem?.InvokeChange(nameof(ConflictWithListContent));
	}

	public void AddNewDefaults() {
		AddListItem(Defaults, NewDefaults);
		NewDefaults = string.Empty;
		CurrentHookupItem?.InvokeChange(nameof(DefaultsListContent));
	}

	public void AddNewOverrides() {
		AddListItem(Overrides, NewOverrides);
		NewOverrides = string.Empty;
		CurrentHookupItem?.InvokeChange(nameof(OverridesListContent));
	}

	public void AddNewRequire() {
		AddListItem(Require, NewRequire);
		NewRequire = string.Empty;
		CurrentHookupItem?.InvokeChange(nameof(RequireListContent));
	}
	public string DataListContent => CurrentHookupItem?.DataListContent ?? "";
	public string SuitableForListContent => CurrentHookupItem?.SuitableForListContent ?? "";
	public string ConflictWithListContent => CurrentHookupItem?.ConflictWithListContent ?? "";
	public string DefaultsListContent => CurrentHookupItem?.DefaultsListContent ?? "";
	public string OverridesListContent => CurrentHookupItem?.OverridesListContent ?? "";
	public string RequireListContent => CurrentHookupItem?.RequireListContent ?? "";

	public string? OpeningList {
		get => CurrentHookupItem?.OpeningList;
		set {
			CurrentHookupItem?.OpeningList = value;
			InvokeChange(nameof(PopupCollection));
		}
	}
	public ObservableCollection<string>? PopupCollection => CurrentHookupItem?.PopupCollection;

	public void DeleteItem(string item) {
		if (CurrentHookupItem == null)
			return;
		PopupCollection?.Remove(item);
		switch (OpeningList) {
			case "TextData":
				CurrentHookupItem.InvokeChange(nameof(DataListContent));
				break;
			case "TextSuitableFor":
				CurrentHookupItem.InvokeChange(nameof(SuitableForListContent));
				break;
			case "TextConflictWith":
				CurrentHookupItem.InvokeChange(nameof(ConflictWithListContent));
				break;
			case "TextDefaults":
				CurrentHookupItem.InvokeChange(nameof(DefaultsListContent));
				break;
			case "TextOverrides":
				CurrentHookupItem.InvokeChange(nameof(OverridesListContent));
				break;
			case "TextRequire":
				CurrentHookupItem.InvokeChange(nameof(RequireListContent));
				break;
		}
		;
	}

	public static void AddListItem(ObservableCollection<string>? list, string newItem) {
		if (list == null || newItem.Length == 0 || list.Contains(newItem))
			return;
		list.Add(newItem);
	}

	public ListDataUC? ListDataUC = null;

	public void LoadFiles() => AccHookupDataIO.LoadAddonHookup(this);
	public void SaveFiles() => AccHookupDataIO.SaveAddonHookup(BindingWindow, this);

	//模型、碰撞体
	public void ChooseModel(ChooseModelType chooseType) {
		string currentPath;
		switch (chooseType) {
			case ChooseModelType.Model:
				currentPath = ModelPath;
				break;
			case ChooseModelType.Coll:
				currentPath = CollPath;
				break;
			default:
				return;
		}
		var path = AccessoryDataUtil.ChooseModelPath(chooseType, currentPath);
		if (path == null)
			return;

		string inProjectPath = path.Replace(ProjectLocation, "");
		string?
			modelPath = null,
			modelColl = null;
		switch (chooseType) {
			case ChooseModelType.Model:
				modelColl = inProjectPath.Replace('\\', '/');
				modelColl = modelColl[..^4];
				if (File.Exists(path[..^4] + ".pic") || File.Exists(path[..^4] + ".pmc"))
					modelPath = modelColl + ".pmd";
				modelColl += ".pmc";
				break;
			case ChooseModelType.Coll:
				modelPath = inProjectPath.Replace('\\', '/');
				modelPath = modelPath[..^4];
				if (File.Exists(path[..^4] + ".pic") || File.Exists(path[..^4] + ".pmc"))
					modelColl = modelPath + ".pmc";
				modelPath += ".pmd";
				break;
			default:
				return;
		}

		if (modelPath != null)
			ModelPath = modelPath;
		if (modelColl != null)
			CollPath = modelColl;
	}

	public void ChooseIcon() => CurrentHookupItem?.ChooseIcon(BindingWindow);
}
