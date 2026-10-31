using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Wpf.Ui.Input;

namespace SCS_Mod_Helper.Manifest {
	public class ManifestBinding: WindowBinding {
		private static ManifestBinding? mBinding = null;
		public static ManifestBinding GetInstance(BasePage page) {
			if (mBinding == null) {
				mBinding = new(() => page);
				mBinding.LoadFiles();
			} else if (mBinding.CurrentProjectLocation != Instances.ProjectLocation)
				mBinding.LoadFiles();
			return mBinding;
		}

		public ICommand ColorCommand { get; }

		public delegate void IInsertColor(string? color);

		public IInsertColor? InsertColor = null;

		public ManifestBinding(Func<IBaseVisual> visualFunc): base(visualFunc) {
			mLocales = DescLocale.GetLocales(out Dictionary<string, DescLocale> localeDict);
			LocaleDict = localeDict;
			ResourceUtil.ChangeLanguage += ChangeLocales;

			ColorCommand = new RelayCommand<string>((color) => InsertColor?.Invoke(color));
		}

		public void InitData() {
			Version = "";
			ModDisplayName = "";
			Author = "";
			MPOptional = false;
			IconName = "";
			ModIcon = null;
			ClearCategories();
			DescriptionName = "";
			foreach (var l in Locales) {
				l.DescContent = "";
			}
		}

		public void LoadFiles() {
			InitData();
			var manifest = Paths.ManifestFile(ProjectLocation);
			if (!File.Exists(manifest))
				return;
			ManifestIO.LoadManifest(this);
		}

		private string? CurrentProjectLocation = null;
		public string ProjectLocation {
			get {
				CurrentProjectLocation ??= Instances.ProjectLocation;
				return CurrentProjectLocation;
			}
			set {
				CurrentProjectLocation = value;
				Instances.ProjectLocation = value;
				InvokeChange();
				InvokeChange(nameof(ProjectExist));
			}
		}
		public bool ProjectExist => Directory.Exists(ProjectLocation);

		private string mVersion = "";
		public string Version {
			get => mVersion;
			set {
				mVersion = value;
				InvokeChange();
			}
		}

		private string mModDisplayName = "";
		public string ModDisplayName {
			get => mModDisplayName;
			set {
				mModDisplayName = value;
				InvokeChange();
			}
		}

		private string mAuthor = "";
		public string Author {
			get => mAuthor;
			set {
				mAuthor = value;
				InvokeChange();
			}
		}

		private bool mMPOptional = false;

		public bool MPOptional {
			get => mMPOptional;
			set {
				mMPOptional = value;
				InvokeChange();
			}
		}

		private string mIconName = "";
		public string IconName {
			get => mIconName;
			set {
				mIconName = value;
				InvokeChange();
			}
		}

		public string? OldIconName = null;
		private BitmapImage? mModIcon = null;
		public BitmapImage? ModIcon {
			get => mModIcon;
			set {
				mModIcon?.Freeze();
				mModIcon = value;
				InvokeChange();
			}
		}
		public bool NewIcon = false;

		public ObservableCollection<string> SelectedCategories = [];
		public bool GetCategory(string cate) => SelectedCategories.Contains(cate);
		public void SetCategory(bool value, string cate) {
			if (value) {
				SelectedCategories.Add(cate);
				if (SelectedCategories.Count > 2) {
					var last = SelectedCategories[0];
					SelectedCategories.RemoveAt(0);
					InvokeChange($"Check_{last}");
				}
			} else {
				SelectedCategories.Remove(cate);
			}
			InvokeChange($"Check_{cate}");
		}

		public void ClearCategories() {
			while (SelectedCategories.Count > 0) {
				SetCategory(false, SelectedCategories[0]);
			}
		}

		public bool Check_truck { get => GetCategory("truck"); set => SetCategory(value, "truck"); }

		public bool Check_trailer { get => GetCategory("trailer"); set => SetCategory(value, "trailer"); }

		public bool Check_interior { get => GetCategory("interior"); set => SetCategory(value, "interior"); }

		public bool Check_tuning_parts { get => GetCategory("tuning_parts"); set => SetCategory(value, "tuning_parts"); }

		public bool Check_ai_traffic { get => GetCategory("ai_traffic"); set => SetCategory(value, "ai_traffic"); }

		public bool Check_sound { get => GetCategory("sound"); set => SetCategory(value, "sound"); }

		public bool Check_paint_job { get => GetCategory("paint_job"); set => SetCategory(value, "paint_job"); }

		public bool Check_cargo_pack { get => GetCategory("cargo_pack"); set => SetCategory(value, "cargo_pack"); }

		public bool Check_map { get => GetCategory("map"); set => SetCategory(value, "map"); }

		public bool Check_ui { get => GetCategory("ui"); set => SetCategory(value, "ui"); }

		public bool Check_weather_setup { get => GetCategory("weather_setup"); set => SetCategory(value, "weather_setup"); }

		public bool Check_physics { get => GetCategory("physics"); set => SetCategory(value, "physics"); }

		public bool Check_graphics { get => GetCategory("graphics"); set => SetCategory(value, "graphics"); }

		public bool Check_models { get => GetCategory("models"); set => SetCategory(value, "models"); }

		public bool Check_movers { get => GetCategory("movers"); set => SetCategory(value, "movers"); }

		public bool Check_walkers { get => GetCategory("walkers"); set => SetCategory(value, "walkers"); }

		public bool Check_prefabs { get => GetCategory("prefabs"); set => SetCategory(value, "prefabs"); }

		public bool Check_other { get => GetCategory("other"); set => SetCategory(value, "other"); }

		public string? OldDescriptionName = null;
		private string mDescriptionName = "";
		public string DescriptionName {
			get => mDescriptionName;
			set {
				mDescriptionName = value;
				InvokeChange();
			}
		}

		private readonly ObservableCollection<DescLocale> mLocales;
		public ObservableCollection<DescLocale> Locales => mLocales;
		public Dictionary<string, DescLocale> LocaleDict {
			get;
			private set;
		}

		private DescLocale? mCurrentLocale = null;
		public DescLocale CurrentLocale {
			get {
				mCurrentLocale ??= Locales.First();
				return mCurrentLocale;
			}
			set {
				mCurrentLocale = value ?? Locales.First();
				InvokeChange();

				InvokeChange(nameof(DescContent));
				InvokeChange(nameof(CopyFronUniversalVisibility));
			}
		}

		public Visibility CopyFronUniversalVisibility {
			get => CurrentLocale.LocaleValue == "universal" ? Visibility.Hidden : Visibility.Visible;
		}

		public string DescContent {
			get => CurrentLocale.DescContent;
			set => CurrentLocale.DescContent = value;
		}

		public void ChangeLocales() {
			foreach (var locale in Locales) {
				locale.RefreshName();
			}
		}
	}
}
