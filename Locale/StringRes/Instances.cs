using SCS_Mod_Helper.Locale.Localization;
using System.Collections.ObjectModel;

namespace SCS_Mod_Helper.Utils;

public partial class Instances {
	private static string projectLocationOfDict = "";
	private static ObservableCollection<LocaleModule>? localeModules = null;
	public static ObservableCollection<LocaleModule> LocaleModules {
		get {
			if (localeModules == null || ProjectLocation != projectLocationOfDict) {
				if (localeModules == null)
					localeModules = [];
				else
					localeModules.Clear();
				projectLocationOfDict = ProjectLocation;
				LoadLocaleDict(localeModules);
			}
			return localeModules;
		}
	}

	private static async void LoadLocaleDict(ObservableCollection<LocaleModule> localeModules) {
		var loaded = LocaleIO.ReadLocaleDict();
		localeModules.Clear();
		foreach (var l in loaded) {
			localeModules.Add(l);
		}
	}

	public static void UpdateLocaleModules(ObservableCollection<LocaleModule> moduleList) {
		LocaleModules.Clear();
		foreach(var module in moduleList) {
			LocaleModules.Add(module);
		}
	}
}


