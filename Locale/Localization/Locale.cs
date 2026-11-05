using SCS_Mod_Helper.Class.Base;

namespace SCS_Mod_Helper.Locale.Localization;

public abstract class Locale(string localeValue, string localeDisplay): BaseBinding {
	public static readonly string LocaleValueUni = "universal";

	public bool IsUniversal => LocaleValue == LocaleValueUni;

	private string mLocaleValue = localeValue;
	public string LocaleValue {
		get => mLocaleValue;
		set {
			mLocaleValue = value;
			InvokeChange();
		}
	}

	private string mLocaleDisplay = localeDisplay;
	public string LocaleDisplay {
		get => mLocaleDisplay;
		set {
			mLocaleDisplay = value;
			InvokeChange();
		}
	}

	public void RefreshName() {
		LocaleDisplay = GetString($"locale.{LocaleValue}");
	}

	protected static List<(string localeValue, string localeDisplay)> SupportedLocales {
		get => GetLocalePair(
			LocaleValueUni,
			"en_gb", "en_us", "zh_cn", "zh_tw", "ja_jp", "bg_bg",
			"ca_es", "cs_cz", "da_dk", "de_de", "el_gr", "es_es",
			"es_la", "et_ee", "eu_es", "fi_fi", "fr_fr", "gl_es",
			"hr_hr", "hu_hu", "it_it", "ka_ge", "ko_kr", "lt_lt",
			"lv_lv", "mk_mk", "nl_nl", "no_no", "pl_pl", "pl_si",
			"pt_br", "pt_pt", "ro_ro", "ru_ru", "sk_sk", "sl_sl",
			"sr_sp", "sr_sr", "sv_se", "tr_tr", "uk_uk", "vi_vn");
	}

	private static List<(string localeValue, string localeDisplay)> GetLocalePair(params string[] locales) {
		List<(string localeValue, string localeDisplay)> localeList = [];
		foreach (var locale in locales) {
			localeList.Add((locale, GetString($"locale.{locale}")));
		}
		return localeList;
	}
}
