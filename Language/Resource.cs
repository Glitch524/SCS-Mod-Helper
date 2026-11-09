using SCS_Mod_Helper.Utils;
using System.Windows;

namespace SCS_Mod_Helper.Language; 
public static class Resource {

	private static object? GetResource(string key, Window? window = null) {
		if (window == null)
			return Instances.IsDebug() ? Application.Current.FindResource(key) : Application.Current.TryFindResource(key);
		else
			return Instances.IsDebug() ? window.FindResource(key) : window.TryFindResource(key);
	}

	public static string GetString(string key, params object[] args) => GetString(key, null, args);
	public static string GetString(this Window window, string key, params object[] args) => GetString(key, window, args);
	private static string GetString(string key, Window? window, params object[] args) {
		var obj = GetResource(key, window);
		if (obj == null)
			return key;
		string res = obj.ToString()!;
		if (args.Length > 0) {
			res = string.Format(res, args);
		}
		return res;
	}

	private const int Zero = 0;
	private const int One = 1;
	private const int Two = 2;
	private const int Few = 3;
	private const int Many = 4;
	private const int Other = 5;
	public static string GetQuantityString(string key, int quantity, params object[] args) => GetQuantityString(key, quantity, null, args);
	public static string GetQuantityString(this Window window, string key, int quantity, params object[] args) => GetQuantityString(key, quantity, window, args);
	private static string GetQuantityString(string key, int quantity, Window? window, params object[] args) {
		var obj = GetResource(key, window);
		if (obj == null)
			return key;
		string?[] plural = (string?[])obj;
		var localized = GetLocaleQuantityString(quantity, plural) ?? key;
		if (args.Length > 0)
			return string.Format(localized, args);
		return localized;
	}

	/// <summary>
	/// 根据locale采用不同的数字规则，获取对应的字符串
	/// </summary>
	/// <param name="quantity"></param>
	/// <param name="plural"></param>
	/// <returns></returns>
	private static string? GetLocaleQuantityString(int quantity, string?[] plural) {
		var lang = System.Globalization.CultureInfo.InstalledUICulture.Name;
		var split = lang.Split('-');
		switch (split[0]) {
			case "zh":
			case "en":
				if (IsInArea(quantity, 0))
					return plural[Zero] ?? plural[Other];
				if (IsInArea(quantity, 1))
					return plural[One] ?? plural[Other];
				return plural[Other];
			case "pl"://查到顺便写上的
				if (IsInArea(quantity, 0))
					return plural[Zero] ?? plural[Other];
				if (IsInArea(quantity, 1))
					return plural[One] ?? plural[Other];
				if (IsInArea(quantity, 2, 4))
					return plural[Few] ?? plural[Other];
				if (IsInArea(quantity, 5, 21))
					return plural[Many] ?? plural[Other];
				return plural[Other];
			case "ar"://查到顺便写上的
				if (IsInArea(quantity, 0))
					return plural[Zero] ?? plural[Other];
				if (IsInArea(quantity, 1))
					return plural[One] ?? plural[Other];
				if (IsInArea(quantity, 2))
					return plural[Two] ?? plural[Other];
				if (IsInArea(quantity, 3, 10))
					return plural[Few] ?? plural[Other];
				if (IsInArea(quantity, 11, 99))
					return plural[Many] ?? plural[Other];
				return plural[Other];
		}
		return null;
	}

	private static bool IsInArea(int quantity, int start, int? end = null) => end == null ? quantity == start : quantity >= start && quantity <= end;

	public static string GetFilter(string key) => GetFilter(key, null);
	public static string GetFilter(this Window window, string key) => GetFilter(key, window);
	private static string GetFilter(string key, Window? window = null) => $"{GetString(key, window)}|{GetString("FilterAllFiles", window)}";
}
