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

	public static void CleanLocaleModules() {
		localeModules = null;
		mLocaleDict = null;
	}



	private static Dictionary<string, List<string>>? mLocaleDict = null;
	public static Dictionary<string, List<string>> LocaleDict {
		get {
			if (mLocaleDict == null || ProjectLocation != projectLocationOfDict) {
				var projectLocation = ProjectLocation;
				if (mLocaleDict == null)
					mLocaleDict = [];
				else
					mLocaleDict.Clear();
				projectLocationOfDict = projectLocation;
				CheckLocaleDict(mLocaleDict);
			}
			return mLocaleDict;
		}
	}

	public static void LocaleDictAdd(string key, string value) {
		if (LocaleDict.TryGetValue(key, out List<string>? list)) {
			list.Add(value);
		} else {
			list = [value];
			LocaleDict.Add(key, list);
		}
	}

	/// <summary>
	/// 合并所有本地化模块中的字典条目到指定的字典中，每个键对应的值为所有相关值的列表。
	/// </summary>
	/// <param name="localeDict">用于合并本地化条目的目标字典，键为字符串，值为字符串列表。</param>
	public static void CheckLocaleDict(Dictionary<string, List<string>> localeDict) {
		var localeModules = LocaleModules;
		foreach (var module in localeModules) {
			foreach (var dict in module.LocaleList) {
				if (dict.Dictionary.Count == 0)
					continue;
				foreach (var pair in dict.Dictionary) {
					if (localeDict.TryGetValue(pair.Key, out List<string>? valueDict)) {
						valueDict.Add(pair.Value);
					} else {
						valueDict = [];
						localeDict.Add(pair.Key, valueDict);
						valueDict.Add(pair.Value);
					}
				}
			}
		}
	}
}


