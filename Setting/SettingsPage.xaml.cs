using Microsoft.Win32;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace SCS_Mod_Helper.Setting;
/// <summary>
/// SettingsPage.xaml 的交互逻辑
/// </summary>
public partial class SettingsPage: BasePage {
	private readonly SettingsBinding Binding;

	public SettingsPage() {
		InitializeComponent();
		Binding = new(() => this);
		Binding.ISetThemeWatcher += (watch) => {
			var window = (BaseWindow)Window.GetWindow(this);
			window.SetThemeWatcher(watch);
		};
		PanelMain.DataContext = Binding;

		Binding.LoadLanguage();
	}

	private void ButtonClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonDirLanguage) {
			var psi = new ProcessStartInfo(Paths.LanguageDir()) { UseShellExecute = true };
			Process.Start(psi);
		} else if (sender == ButtonCheckPath) {
			Binding.AutoDetectGamePath();
		} else if (sender == ButtonBrowsePathETS2) {
			Binding.ChooseGamePath(true);
		} else if (sender == ButtonBrowsePathATS) {
			Binding.ChooseGamePath(false);
		} else if (sender == ButtonConverterPix) {
			Binding.ChooseConverterPixPath();
		} else if (sender == ButtonConversionTools) {
			Binding.ChooseConversionToolsPath();
		}
	}

	private void HyperlinkClick(object sender, RequestNavigateEventArgs e) {
		Process.Start(new ProcessStartInfo {
			FileName = e.Uri.ToString(),
			UseShellExecute = true
		});
		e.Handled = true;
	}

	private void FilePathClick(object sender, RoutedEventArgs e) {
		if (sender is Hyperlink hyperlink) {
			var path = hyperlink.Tag as string;
			Process.Start("explorer.exe", $"/select,\"{path}\"");
		}
	}
}

public class SettingsBinding(Func<IBaseVisual> windowFunc): WindowBinding(windowFunc) {

	private readonly ObservableCollection<LanguageItem> mLanguages = [];
	public ObservableCollection<LanguageItem> Languages => mLanguages;

	public string CurrentLanguage {
		get => Instances.CurrentLanguage;
		set {
			Instances.CurrentLanguage = value;
			InvokeChange();
			ResourceUtil.SetLanguage(value);
		}
	}

	public void LoadLanguage() {
		foreach (var l in ResourceUtil.langs) {
			Languages.Add(new(l.Key, l.Value.languageName, l.Value.dictionary));
		}
	}

	public delegate void SetThemeWatcher(bool watch);
	public SetThemeWatcher? ISetThemeWatcher = null;
	public string CurrentTheme {
		get => Instances.CurrentTheme;
		set {
			Instances.CurrentTheme = value;
			InvokeChange();
			ISetThemeWatcher?.Invoke(value == "FollowSystem");
			ResourceUtil.SetTheme(value);
		}
	}

	public string ETS2GamePath {
		get => Instances.ETS2GamePath;
		set {
			Instances.ETS2GamePath = value;
			InvokeChange();
		}
	}

	public string ATSGamePath {
		get => Instances.ATSGamePath;
		set {
			Instances.ATSGamePath = value;
			InvokeChange();
		}
	}

	public void AutoDetectGamePath() {
		string? steamPath = GetInstallPath(@"SOFTWARE\WOW6432Node\Valve\Steam");
		steamPath ??= GetInstallPath(@"SOFTWARE\Valve\Steam");
		if (steamPath == null) { //注册表读取不到路径时取消查找
			MessageBox.Show(GetString("MessageAutoDetectNoSteamRegistry"));
			return;
		}
		string ets2Path = Path.Combine(steamPath, @"steamapps\common", "Euro Truck Simulator 2");
		string atsPath = Path.Combine(steamPath, @"steamapps\common", "American Truck Simulator");
		bool ets2Get = false, atsGet = false;
		if (Directory.Exists(ets2Path)) {//尝试先确定游戏在不在安装目录的common内，没有再读vdf
			ETS2GamePath = ets2Path;
			ets2Get = true;
		}
		if (Directory.Exists(atsPath)) {
			ATSGamePath = atsPath;
			atsGet = true;
		}
		if (ets2Get && atsGet)
			return;
		//不在主库时 读取安装路径内的steamapps\libraryfolders.vdf得知游戏安装在哪个库里
		string vdfFile = Path.Combine(steamPath, @"steamapps", "libraryfolders.vdf");
		var (pathETS2, pathATS) = GetPathFromVDF(steamPath, vdfFile, ets2Get, atsGet);
		if (pathETS2 != null) {
			pathETS2 = Path.Combine(pathETS2, @"steamapps\common", "Euro Truck Simulator 2");
			if (Directory.Exists(pathETS2))
				ETS2GamePath = pathETS2;
		}
		if (pathATS != null) {
			pathATS = Path.Combine(pathATS, @"steamapps\common", "American Truck Simulator");
			if (Directory.Exists(pathATS))
				ATSGamePath = pathATS;
		}
		if (ets2Get && atsGet)
			return;
		List<string> message = [];
		message.Add(GetString("MessageAutoDetectResultFail"));
		if (!ets2Get)
			message.Add("        " + GetString("ETS2"));
		if (!atsGet)
			message.Add("        " + GetString("ATS"));
		MessageBox.Show(BindingWindow, string.Join('\n', message));
	}
	private static string? GetInstallPath(string path) {
		RegistryKey? steam = Registry.LocalMachine.OpenSubKey(path);
		if (steam == null)
			return null;
		using (steam) {
			string? installPath = steam.GetValue("installpath")?.ToString();
			if (string.IsNullOrEmpty(installPath))
				return null;
			return installPath;
		}
	}

	private static (string? pathETS, string? pathATS) GetPathFromVDF(string installPath, string vdfFile, bool ets2Get, bool atsGet) {
		using StreamReader sr = new(vdfFile);
		string? line = null;
		string? pathETS2 = null, pathATS = null;
		while ((line = sr.ReadLine()) != null) {
			if (line.StartsWith("\t\"")) {//说明在library序号那行
				string? path = null;
				sr.ReadLine();
				while((line = sr.ReadLine()) != null) {
					if (line.StartsWith("\t\t}"))
						break;
					if (line.Contains("\"path\"")) {
						var pathHead = "\t\t\"path\"";
						int start = line.IndexOf('\"', pathHead.Length) + 1;
						int end = line.LastIndexOf('\"');
						path = line[start..end];
						path = path.Replace(@"\\", @"\");
						if (path == installPath)
							break;
						continue;
					}
					if (line.Contains("\"apps\"")) {
						sr.ReadLine();
						while ((line = sr.ReadLine()) != null) {
							if (line.Contains('}'))
								break;
							if (line.Contains("227300") && !ets2Get) {
								pathETS2 = path;
								ets2Get = true;
							} else if (line.Contains("270880") && !atsGet) {
								pathATS = path;
								atsGet = true;
							}
							if (ets2Get && atsGet)
								return (pathETS2, pathATS);
						}
						continue;
					}
				}
			}
		}
		return (null, null);
	}

	public void ChooseGamePath(bool ets2) {
		var start = ets2 ? ETS2GamePath : ATSGamePath;
		if (start.Length > 0) {
			if (!File.Exists(start))
				start = string.Empty;
		}
		var folderDialog = new OpenFolderDialog {
			Multiselect = false,
			InitialDirectory = start,
			Title = ets2 ? GetString("ETS2") : GetString("ATS"),
		};
		if (folderDialog.ShowDialog() == true) {
			var path = folderDialog.FolderName;
			if (ets2)
				ETS2GamePath = path;
			else
				ATSGamePath = path;
		}
	}

	public string ConverterPixPath {
		get => Instances.ConverterPixPath;
		set {
			Instances.ConverterPixPath = value;
			InvokeChange();
		}
	}
	public void ChooseConverterPixPath() {
		string start = ConverterPixPath;
		if (start.Length > 0)
			start = new DirectoryInfo(start).Parent?.FullName ?? "";
		var fileDialog = new OpenFileDialog {
			Multiselect = false,
			InitialDirectory = start,
			DefaultExt = "converter_pix.exe",
			Title = "ConverterPix" + GetString("ExternalToolsPath"),
			Filter = Util.GetFilter("FilterCPix")
		};
		if (fileDialog.ShowDialog() == true) {
			if (fileDialog.SafeFileName != "converter_pix.exe") {
				MessageBox.Show(BindingWindow, "MessageErrNotCPix");
				return;
			}
			var path = fileDialog.FileName;
			var parent = Directory.GetParent(path)!;
			var is64Bit = Environment.Is64BitOperatingSystem;
			switch (parent.Name) {
				case "win_x86":
					if (is64Bit) {
						path = path.Replace("win_x86", "win_x64");
					}
					break;
				case "win_x64":
					if (!is64Bit) {
						path = path.Replace("win_x64", "win_x86");
					}
					break;
				default:
					var result = MessageBox.Show(
						GetString("MessageErrDifferentStructure"),
						GetString("MessageTitleNotice"),
						MessageBoxButton.YesNo);
					if (result != MessageBoxResult.Yes) {
						return;
					}
					break;
			}
			ConverterPixPath = path;
		}
	}
	public string ConversionToolsPath {
		get => Instances.ConversionToolsPath;
		set {
			Instances.ConversionToolsPath = value;
			InvokeChange();
		}
	}
	public void ChooseConversionToolsPath() {
		string start = ConversionToolsPath;
		var folderDialog = new OpenFolderDialog {
			Multiselect = false,
			InitialDirectory = start,
			Title = "Conversion Tools" + GetString("ExternalToolsPath"),
		};
		if (folderDialog.ShowDialog() == true) {
			var path = folderDialog.FolderName;
			ConversionToolsPath = path;
		}
	}
}
public class LanguageItem(string locale, string languageName, ResourceDictionary dictionary) {
	private string mLocale = locale;
	private string mLanguageName = languageName;
	private ResourceDictionary mDictionary = dictionary;

	public string Locale {
		get => mLocale; set => mLocale = value;
	}
	public string LanguageName {
		get => mLanguageName; set => mLanguageName = value;
	}
	public ResourceDictionary Dictionary {
		get => mDictionary; set => mDictionary = value;
	}
}