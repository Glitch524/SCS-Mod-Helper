using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Modding.Accessories.AccHookup;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace SCS_Mod_Helper.Modding.Accessories.Physics;

class PhysicsBinding(Func<IBaseVisual> visualFunc): WindowBinding(visualFunc) {
	public static string ProjectLocation => Instances.ProjectLocation;

	private bool localPhysics = false;
	public bool LocalPhysics {
		get => localPhysics;
		set {
			localPhysics = value;
			InvokeChange(nameof(PhysicsItems));
		}
	}

	private bool mChooseMode = false;
	public bool ChooseMode {
		get => mChooseMode;
		set {
			mChooseMode = value;
			InvokeChange();
		}
	}

	private SuiItem? mCurrentSuiItem;
	public SuiItem? CurrentSuiItem {
		get => mCurrentSuiItem;
		set {
			mCurrentSuiItem = value;
			InvokeChange();

			InvokeChange(nameof(PhysicsItems));
			if (PhysicsItems != null && PhysicsItems.Count > 0)
				CurrentPhysicsItem = PhysicsItems.First();
		}
	}

	public ObservableCollection<PhysicsData> PhysicsItems {
		get {
			if (LocalPhysics)
				return AccessoryPhysicsIO.PhysicsItems;
			else if (CurrentSuiItem != null)
				return CurrentSuiItem.PhysicsItems;
			else
				return [];
		}
	}

	public delegate void CurrentPhysicsCallback(PhysicsData? physics);
	public CurrentPhysicsCallback? physicsCallback = null;

	private PhysicsData? mCurrentPhysicsItem;
	public PhysicsData? CurrentPhysicsItem {
		get => mCurrentPhysicsItem;
		set {
			mCurrentPhysicsItem = value;
			InvokeChange();

			physicsCallback?.Invoke(value);
			InvokeChange(nameof(CurrentToyData));
			InvokeChange(nameof(CurrentPatchData));

			InvokeChange(nameof(PhysicsType));
			InvokeChange(nameof(ToyVisibility));
			InvokeChange(nameof(PatchVisibility));
		}
	}

	public const string PhysicsTypeToy = "physics_toy_data";
	public const string PhysicsTypePatch = "physics_patch_data";

	public string PhysicsType {
		get {
			if (CurrentPhysicsItem is PhysicsToyData)
				return PhysicsTypeToy;
			else if (CurrentPhysicsItem is PhysicsPatchData)
				return PhysicsTypePatch;
			return "";
		}
	}

	public Visibility ToyVisibility => CurrentPhysicsItem is PhysicsToyData ? Visibility.Visible : Visibility.Hidden;
	public Visibility PatchVisibility => CurrentPhysicsItem is PhysicsPatchData ? Visibility.Visible : Visibility.Hidden;

	public PhysicsToyData? CurrentToyData {
		get => CurrentPhysicsItem is PhysicsToyData toyData ? toyData : null;
		set => CurrentPhysicsItem = value;
	}

	public void PhysicsListAdd(PhysicsData physics) {
		PhysicsItems.Add(physics);
		CurrentPhysicsItem = physics;
	}

	public string ModelPath {
		get => CurrentToyData?.ModelPath ?? "";
		set {
			if (CurrentToyData != null) {
				CurrentToyData.ModelPath = value;
				InvokeChange();
			}
		}
	}

	public string CollPath {
		get => CurrentToyData?.CollPath ?? "";
		set {
			if (CurrentToyData != null) {
				CurrentToyData.CollPath = value;
				InvokeChange();
			}
		}
	}
	public string ToyType {
		get => CurrentToyData?.ToyType ?? "";
		set {
			if (CurrentToyData != null) {
				CurrentToyData.ToyType = value;
				InvokeChange();
			}
		}
	}

	public string RopeMaterial {
		get => CurrentToyData?.RopeMaterial ?? "";
		set {
			if (CurrentToyData != null) {
				CurrentToyData.RopeMaterial = value;
				InvokeChange();
			}
		}
	}

	public PhysicsPatchData? CurrentPatchData {
		get => CurrentPhysicsItem is PhysicsPatchData patchData ? patchData : null;
		set => CurrentPhysicsItem = value;
	}

	public string PatchMaterial {
		get => CurrentPatchData?.Material ?? "";
		set {
			if (CurrentPatchData != null) {
				CurrentPatchData.Material = value;
				InvokeChange();
			}
		}
	}

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
}
