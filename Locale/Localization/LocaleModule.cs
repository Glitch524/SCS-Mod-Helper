using SCS_Mod_Helper.Class.Base;
using System.Collections.ObjectModel;

namespace SCS_Mod_Helper.Locale.Localization;
public class LocaleModule: BaseBinding {
	public bool Focusable {
		get;
	} = false;
	public bool Expanded {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;
	public bool Selected {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;

	public string ModuleName {
		get;
		set {
			field = value;
			InvokeChange();
		}
	}

	public readonly ObservableCollection<ModLocale> LocaleList;

	public readonly Dictionary<string, ModLocale> LocaleDict;

	public ObservableCollection<LocalePair> UniversalDict { 
		get; 
		set {
			field = value;
			InvokeChange();
		}
	}

	public LocaleModule(string moduleName) {
		ModuleName = moduleName;

		LocaleList = ModLocale.GetLocales(out ModLocale universal);
		LocaleDict = LocaleList.ToDictionary(l => l.LocaleValue);
		UniversalDict = universal.Dictionary;

	}

	public ModLocale GetLocale(string locale) {
		if (LocaleDict.TryGetValue(locale, out var v)) 
			return v;
		throw new ArgumentException("wrong locale: " + locale);
	}

	public override string ToString() => $"LocaleModule={ModuleName}";
}

public class ModLocale(string localeValue, string localeDisplay): Locale(localeValue, localeDisplay) {
	/// <summary>
	/// 返回所有受支持的 ModLocale 实例的集合，并输出通用语言实例。
	/// </summary>
	/// <param name="universal">输出参数，接收通用语言的 ModLocale 实例。</param>
	/// <returns>包含所有受支持语言的 ModLocale 集合。</returns>
	public static ObservableCollection<ModLocale> GetLocales(out ModLocale universal) {
		ObservableCollection<ModLocale> locales = [];
		ModLocale? uni = null;
		foreach (var (localeValue, localeDisplay) in SupportedLocales) {
			ModLocale modLocale = new(localeValue, localeDisplay);
			locales.Add(modLocale);
			if (localeValue == LocaleValueUni)
				uni = modLocale;
		}
		universal = uni!;
		return locales;
	}

	public readonly ObservableCollection<LocalePair> mDictionary = [];
	public ObservableCollection<LocalePair> Dictionary => mDictionary;

	public void AddPair(string key, string value) => Dictionary.Add(new(key, value));

	public string? GetValue(string key) {
		foreach (var pair in Dictionary) {
			if (pair.Key == key)
				return pair.Value;
		}
		return null;
	}

	public void ClearDict() => Dictionary.Clear();

	public override string ToString() => $"ModLocale={LocaleDisplay}";
}


public class LocalePair(string key, string value): BaseBinding {
	public bool Focusable {
		get;
	} = true;
	public bool Expanded {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;
	public bool Selected {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;

	public string Key {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = key;

	public string Value {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = value;
}
