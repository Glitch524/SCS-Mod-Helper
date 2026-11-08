using SCS_Mod_Helper.Locale.StringRes;
using SCS_Mod_Helper.Setting;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SCS_Mod_Helper.Utils;
public static partial class Instances {
	public static string GameVersion = "1.62";

	public static bool IsDebug() => Debugger.IsAttached;

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
		get => mConverterPixPath;
		set {
			mConverterPixPath = value;
			mConverterPixExe = null;
			Settings.Default.ConverterPixPath = value;
			Settings.Default.Save();
		}
	}

	private static string? mConverterPixExe = null;
	public static string? ConverterPixExe {
		get {
			if (mConverterPixExe == null) {
				if (mConverterPixPath.Length == 0)
					return null;
				mConverterPixExe = Path.Combine(mConverterPixPath, @"bin", Environment.Is64BitOperatingSystem ? "win_x64" : "win_x86", "converter_pix.exe");
			}
			return mConverterPixExe;
		}
	}

	private static string mConversionToolsPath = Settings.Default.ConversionToolsPath;
	public static string ConversionToolsPath {
		get {
			return mConversionToolsPath;
		}

		set {
			mConversionToolsPath = value;
			Settings.Default.ConversionToolsPath = value;
			Settings.Default.Save();
		}
	}

	private static string mCommonModPath = Settings.Default.CommonModPath;
	public static string CommonModPath {
		get => mCommonModPath;
		set {
			mCommonModPath = value;
			Settings.Default.CommonModPath = value;
			Settings.Default.Save();
		}
	}

	private static string mETS2GamePath = Settings.Default.ETS2GamePath;
	public static string ETS2GamePath {
		get => mETS2GamePath;
		set {
			mETS2GamePath = value;
			Settings.Default.ETS2GamePath = value;
			Settings.Default.Save();
		}
	}

	private static string mATSGamePath = Settings.Default.ATSGamePath;
	public static string ATSGamePath {
		get => mATSGamePath;
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

	private static string mHistoryModelPath = string.Empty;
	public static string HistoryModelPath {
		get {
			if (mHistoryModelPath == string.Empty) {
				mHistoryModelPath = History.Default.ModelPath;
			}
			return mHistoryModelPath;
		}
		set {
			mHistoryModelPath = value;
			History.Default.ModelPath = value;
			History.Default.Save();
		}
	}

	private static string mHistoryTexturePath = string.Empty;
	public static string HistoryTexturePath {
		get {
			if (mHistoryTexturePath == string.Empty) {
				mHistoryTexturePath = History.Default.TexturePath;
			}
			return mHistoryTexturePath;
		}
		set {
			mHistoryTexturePath = value;
			History.Default.TexturePath = value;
			History.Default.Save();
		}
	}

	public static string HistoryPixExportPath {
		get {
			if (field == string.Empty) {
				field = History.Default.PixExportPath;
			}
			return field;
		}
		set {
			field = value;
			History.Default.PixExportPath = value;
			History.Default.Save();
		}
	} = string.Empty;
}