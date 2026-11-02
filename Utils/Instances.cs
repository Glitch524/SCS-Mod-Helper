using SCS_Mod_Helper.Locale.StringRes;
using SCS_Mod_Helper.Setting;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SCS_Mod_Helper.Utils;
public static partial class Instances {
	public static string GameVersion = "1.62";

	private static string? mAppVersionString = null;
	public static string AppVersionString {
		get {
			if (mAppVersionString == null) {
				var assembly = Assembly.GetEntryAssembly();
				mAppVersionString = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "Unknown";
			}
			return mAppVersionString;
		}
	}

	public static Version AppVersion {
		get {
			var versionString = VersionRegex().Replace(AppVersionString, "");
			return Version.Parse(versionString);
		}
	}

	[GeneratedRegex(@"[^0-9.]")]
	public static partial Regex VersionRegex();

	public static string CurrentLanguage {
		get {
			var l = Settings.Default.Language;
			if (l.Length == 0)
				l = System.Globalization.CultureInfo.InstalledUICulture.Name;
			return l;
		}
		set {
			Settings.Default.Language = value;
			Settings.Default.Save();
		}
	}

	public static string CurrentTheme {
		get => Settings.Default.Theme;
		set {
			Settings.Default.Theme = value;
			Settings.Default.Save();
		}
	}

	public static bool FollowSystem => CurrentTheme == "FollowSystem";

	public static string UpdateCheckFrequency {
		get => Settings.Default.UpdateCheckFrequency;
		set {
			Settings.Default.UpdateCheckFrequency = value;
			Settings.Default.Save();
		}
	}

	public static bool ShowUpdateAvailable {
		get => Settings.Default.ShowUpdateAvailable;
		set {
			Settings.Default.ShowUpdateAvailable = value;
			Settings.Default.Save();
		}
	}

	public static int StoredExceptionCount {
		get => Settings.Default.StoredExcpCount;
		set {
			Settings.Default.StoredExcpCount = value;
			Settings.Default.Save();
		}
	}

	private static string mConverterPixPath = Settings.Default.ConverterPixPath;
	public static string ConverterPixPath {
		get {
			if (mConverterPixPath.Length == 0)
				return Util.GetString("StatusEmpty");
			return mConverterPixPath;
		}

		set {
			mConverterPixPath = value;
			Settings.Default.ConverterPixPath = value;
			Settings.Default.Save();
		}
	}

	private static string mConversionToolsPath = Settings.Default.ConversionToolsPath;
	public static string ConversionToolsPath {
		get {
			if (mConversionToolsPath.Length == 0)
				return Util.GetString("StatusEmpty");
			return mConversionToolsPath;
		}

		set {
			mConversionToolsPath = value;
			Settings.Default.ConversionToolsPath = value;
			Settings.Default.Save();
		}
	}

	private static string mETS2GamePath = Settings.Default.ETS2GamePath;
	public static string ETS2GamePath {
		get {
			if (mETS2GamePath.Length == 0)
				return Util.GetString("StatusEmpty");
			return mETS2GamePath;
		}

		set {
			mETS2GamePath = value;
			Settings.Default.ETS2GamePath = value;
			Settings.Default.Save();
		}
	}

	private static string mATSGamePath = Settings.Default.ATSGamePath;
	public static string ATSGamePath {
		get {
			if (mATSGamePath.Length == 0)
				return Util.GetString("StatusEmpty");
			return mATSGamePath;
		}

		set {
			mATSGamePath = value;
			Settings.Default.ATSGamePath = value;
			Settings.Default.Save();
		}
	}


	private static string mProjectLocation = Settings.Default.ProjectLocation;
	public static string ProjectLocation {
		get => mProjectLocation;
		set {
			mProjectLocation = value;
			Settings.Default.ProjectLocation = value;
			Settings.Default.Save();
			StringResUtil.CleanStringResList();
			localeModules = null;
		}
	}
}