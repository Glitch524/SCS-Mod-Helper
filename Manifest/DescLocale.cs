using System.Collections.ObjectModel;

namespace SCS_Mod_Helper.Manifest {

	public class DescLocale(string localeValue, string localeDisplay): Locale.Localization.Locale(localeValue, localeDisplay) {

		public static ObservableCollection<DescLocale> GetLocales(out Dictionary<string, DescLocale> localeDict) {
			ObservableCollection<DescLocale> l = [];
			localeDict = [];
			foreach (var (localeValue, localeDisplay) in SupportedLocales) {
				DescLocale descLocale = new(localeValue, localeDisplay);
				l.Add(descLocale);
				localeDict.Add(localeValue, descLocale);
			}
			return l;
		}

		public bool HasDesc {
			get => DescContent.Length > 0;
			set {
				InvokeChange(nameof(HasDesc));
			}
		}

		private string mDescContent = "";
		public string DescContent {
			get => mDescContent;
			set {
				mDescContent = value;
				InvokeChange(nameof(HasDesc));
				InvokeChange(nameof(DescContent));
			}
		}
	}
}
