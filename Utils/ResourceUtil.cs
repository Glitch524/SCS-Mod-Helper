using System.IO;
using System.Windows;
using Wpf.Ui.Appearance;

namespace SCS_Mod_Helper.Utils; 

class ResourceUtil {
	public static readonly Dictionary<string, (string languageName, ResourceDictionary dictionary)> langs = [];
	public static readonly Dictionary<string, ResourceDictionary> themes = [];

	public static string ExtLangPreffix = "ext_";
	private static void CollectDictionaries() {
		var md = Application.Current.Resources.MergedDictionaries;
		foreach (var dict in md) {
			var name = dict.Source.OriginalString;
			var start = name.LastIndexOf('/');
			var end = name.LastIndexOf('.');
			if (name.StartsWith("/Language")) {
				name = name[(start + 1)..end];
				var s = name.Split('_');
				langs.Add(s[0], new(s[1], dict));
			} else if (name.StartsWith("/Theme")) {
				name = name[(start + 1)..end];
				themes.Add(name, dict);
			}
		}
	}

	public static void SetupDictionary() {
		if (langs.Count == 0 && themes.Count == 0) {
			CollectDictionaries();
			var langDir = new DirectoryInfo(Paths.LanguageDir());
			if (langDir.Exists) {
				foreach (var file in langDir.GetFiles()) {
					if (file.Extension.Equals(".xaml")) {
						var locale = file.Name;
						var end = locale.LastIndexOf('.');
						locale = locale[0..end];
						var s = locale.Split('_');
						var dict = new ResourceDictionary {
							Source = new Uri(file.FullName)
						};
						langs.Add(ExtLangPreffix + s[0], new("Ext:" + s[1], dict));
					}
				}
			}
		}
	}

	public delegate void ChangeLanguageDelegate();
	public static event ChangeLanguageDelegate? ChangeLanguage;
	public static void SetLanguage(string? locale = null) {
		SetupDictionary();
		if (!langs.TryGetValue(locale ?? Instances.CurrentLanguage, out var selected)) {
			selected = langs["en-US"];
			Instances.CurrentLanguage = "en-US";
		}
		var md = Application.Current.Resources.MergedDictionaries;
		md.Remove(selected.dictionary);
		md.Add(selected.dictionary);
		ChangeLanguage?.Invoke();
	}

	public static void SetTheme(string? theme = null) {
		theme ??= Instances.CurrentTheme;
		if (theme == "FollowSystem")
			theme = GetSystemTheme();
		switch (theme) {
			case "Light":
				ApplicationThemeManager.Apply(ApplicationTheme.Light);
				UpdateThemeDict(theme);
				break;
			case "Dark":
				ApplicationThemeManager.Apply(ApplicationTheme.Dark);
				UpdateThemeDict(theme);
				break;
		}
	}

	public static void UpdateThemeDict(string theme) {
		SetupDictionary();
		if (themes.TryGetValue(theme ?? Instances.CurrentTheme, out var selected)) {
			var md = Application.Current.Resources.MergedDictionaries;
			md.Remove(selected);
			md.Add(selected);
		}
	}

	public static string GetSystemTheme() {
		SystemThemeManager.UpdateSystemThemeCache();
		return SystemThemeManager.GetCachedSystemTheme().ToString();
	}
}
