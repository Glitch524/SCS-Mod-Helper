using SCS_Mod_Helper.Class.Base;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Modding.Accessories {
	public abstract class AccessoryData(): BaseBinding {
		public AccessoryData(
				string modelName,
				string displayName,
				long? price,
				uint? unlockLevel,
				string iconName,
				string partType): this() {
			mModelName = modelName;
			mDisplayName = displayName;
			mPrice = price;
			mUnlockLevel = unlockLevel;
			mIconName = iconName;
			mPartType = partType;
		}

		protected string mModelName = "";
		public string ModelName {
			get => mModelName;
			set {
				mModelName = value;
				InvokeChange();

				InvokeChange(nameof(NameOver12));
			}
		}

		public bool NameOver12 => ModelName.Length > 12;

		protected string mDisplayName = "";
		public string DisplayName {
			get => mDisplayName;
			set {
				mDisplayName = value;
				InvokeChange();
			}
		}

		protected string? mLocalizedDisplayName = null;
		public string? LocalizedDisplayName {
			get => mLocalizedDisplayName;
			set {
				mLocalizedDisplayName = value;
				InvokeChange();
			}
		}


		protected long? mPrice = null;
		public long? Price {
			get => mPrice;
			set {
				mPrice = value;
				InvokeChange();
				InvokeChange(nameof(PriceString));
			}
		}
		public string PriceString {
			get => Price.ToString() ?? "";
			set {
				if (long.TryParse(value, out long res)) {
					Price = res;
				} else
					Price = null;
				InvokeChange();
				InvokeChange(nameof(Price));
			}
		}

		protected uint? mUnlockLevel = null;
		public uint? UnlockLevel {
			get => mUnlockLevel;
			set {
				mUnlockLevel = value;
				InvokeChange();
				InvokeChange(nameof(UnlockLevelString));
			}
		}
		public string UnlockLevelString {
			get => UnlockLevel.ToString() ?? "";
			set {
				if (uint.TryParse(value, out uint res)) {
					UnlockLevel = res;
				} else
					UnlockLevel = null;
				InvokeChange();
				InvokeChange(nameof(UnlockLevel));
			}
		}

		protected string mIconName = "";
		public string IconName {
			get => mIconName;
			set {
				mIconName = value;
				InvokeChange();
				InvokeChange(nameof(ModelIcon));
			}
		}

		private string? LoadedIconName = null;
		protected BitmapSource? mModelIcon = null;
		public BitmapSource? ModelIcon {
			get => mIconName == LoadedIconName ? mModelIcon : null;
			set {
				mModelIcon?.Freeze();
				mModelIcon = value;
				LoadedIconName = mIconName;
				InvokeChange();
			}
		}

		public void ChooseIcon(Window window) {
			var iconPath = AccessoryDataUtil.ChooseIconPath(window);
			if (iconPath != null)
				CheckIcon(window, iconPath);
		}

		public void CheckIcon(Window window, string path) {
			var icon = AccessoryDataUtil.CheckIconPath(window, path, out string? iconPath);
			if (icon != null) {
				IconName = icon;
				ModelIcon = AccessoryDataUtil.LoadModelIcon(iconPath);
			}
		}

		protected string mPartType = "unknown";
		public string PartType {
			get => mPartType;
			set {
				mPartType = value;
				InvokeChange();
			}
		}

		private ObservableCollection<string> data = [];
		private ObservableCollection<string> suitableFor = [];
		private ObservableCollection<string> conflictWith = [];
		private ObservableCollection<string> defaults = [];
		private ObservableCollection<string> overrides = [];
		private ObservableCollection<string> require = [];
		public ObservableCollection<string> Data { get => data; set => data = value; }
		public ObservableCollection<string> SuitableFor { get => suitableFor; set => suitableFor = value; }
		public ObservableCollection<string> ConflictWith { get => conflictWith; set => conflictWith = value; }
		public ObservableCollection<string> Defaults { get => defaults; set => defaults = value; }
		public ObservableCollection<string> Overrides { get => overrides; set => overrides = value; }
		public ObservableCollection<string> Require { get => require; set => require = value; }
		public string DataListContent => JoinList(Data);
		public string SuitableForListContent => JoinList(SuitableFor);
		public string ConflictWithListContent => JoinList(ConflictWith);
		public string DefaultsListContent => JoinList(Defaults);
		public string OverridesListContent => JoinList(Overrides);
		public string RequireListContent => JoinList(Require);

		private static string JoinList(ObservableCollection<string>? list) {
			if (list == null || list.Count == 0)
				return string.Empty;
			return string.Join(", ", list);
		}

		private string? mOpeningList = null;

		public string? OpeningList {
			get => mOpeningList;
			set {
				mOpeningList = value;
				InvokeChange(nameof(PopupCollection));
			}
		}
		public ObservableCollection<string>? PopupCollection {
			get {
				return OpeningList switch {
					"TextData" => Data,
					"TextSuitableFor" => SuitableFor,
					"TextConflictWith" => ConflictWith,
					"TextDefaults" => Defaults,
					"TextOverrides" => Overrides,
					"TextRequire" => Require,
					_ => null,
				};
			}
		}
	}

	public abstract class AccessoryIntData: AccessoryData {
		public AccessoryIntData() : base() {

		}
		public AccessoryIntData(
			string modelName,
			string displayName,
			long? price,
			uint? unlockLevel,
			string iconName,
			string partType,
			string modelColl,
			string look,
			string variant,
			string electricType) : base(modelName, displayName, price, unlockLevel, iconName, partType) {
			mModelColl = modelColl;
			mLook = look;
			mVariant = variant;
			mElectricType = electricType;
		}

		protected string mModelColl = "";
		public string CollPath {
			get => mModelColl;
			set {
				mModelColl = value;
				InvokeChange();
			}
		}

		protected string mLook = "";
		public string Look {
			get => mLook;
			set {
				mLook = value;
				InvokeChange();
			}
		}

		protected string mVariant = "";
		public string Variant {
			get => mVariant;
			set {
				mVariant = value;
				InvokeChange();
			}
		}

		private string mElectricType = "vehicle";
		public string ElectricType {
			get => mElectricType;
			set {
				mElectricType = value;
				InvokeChange();
			}
		}
	}
}
