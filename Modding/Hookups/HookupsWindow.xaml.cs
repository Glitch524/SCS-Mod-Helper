using Microsoft.Win32;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SCS_Mod_Helper.Modding.Hookups;
/// <summary>
/// CreateHookup.xaml 的交互逻辑
/// </summary>
public partial class HookupsWindow: BaseWindow {
	private readonly HookupBinding Binding;

	public string ProjectLocation {
		get => Binding.ProjectLocation; set => Binding.ProjectLocation = value;
	}
	private bool IsAnimHookup {
		get => Binding.IsAnimHookup; set => Binding.IsAnimHookup = value;
	}
	public string HookupName {
		get => Binding.HookupName; set => Binding.HookupName = value;
	}
	public string ModelLocation {
		get => Binding.ModelLocation; set => Binding.ModelLocation = value;
	}
	public string AnimationLocation {
		get => Binding.AnimationLocation; set => Binding.AnimationLocation = value;
	}

	public HookupsWindow() {
		InitializeComponent();

		Binding = new(() => this);
		ProjectLocation = Instances.ProjectLocation;
		PanelMain.DataContext = Binding;
	}

	private void ButtonChooseFile(object sender, RoutedEventArgs e) {
		if (sender is Button button) {
			var ChooseModel = button == ButtonChooseModel;

			var fileDialog = new OpenFileDialog {
				Multiselect = false,
				InitialDirectory = Paths.IntDecorsDir(ProjectLocation),
				DefaultExt = "pmd",
				Title = GetString(ChooseModel ? "DialogTitleModel" : "DialogTitleAnim"),
				Filter = Util.GetFilter(ChooseModel ? "DialogFilterModel" : "DialogFilterAnim")
			};
			if (fileDialog.ShowDialog() != true)
				return;
			try {
				string path = fileDialog.FileName;
				if (!path.StartsWith(ProjectLocation) || path.Length <= ProjectLocation.Length)
					throw new(GetString("MessageFileErrOutside"));
				if (ChooseModel) {
					if (!path.EndsWith(".pim") && !path.EndsWith(".pmd"))
						throw new(GetString("MessageFileErrNotModel"));
				} else if (!path.EndsWith(".pia") && !path.EndsWith(".pma"))
					throw new(GetString("MessageFileErrNotAnim"));
				var pmdExist = false;
				var pmaExist = false;
				path = path[..^4];
				if ((ChooseModel || ModelLocation.Length == 0) && (File.Exists(path + ".pim") || File.Exists(path + ".pmd")))
					pmdExist = true;
				if (File.Exists(path + ".pia") || File.Exists(path + ".pma"))
					pmaExist = true;
				path = path.Replace(ProjectLocation, "");
				path = path.Replace('\\', '/');
				if (pmdExist)
					ModelLocation = path + ".pmd";
				if (pmaExist)
					AnimationLocation = path + ".pma";
				if (HookupName.Length == 0 && (ChooseModel || pmdExist)) {
					var filename = fileDialog.SafeFileName;
					filename = filename[..filename.LastIndexOf('.')];
					if (filename.EndsWith("_o"))
						filename = filename[..^2];
					HookupName = filename;
				}
			} catch (Exception ex) {
				MessageBox.Show(this, ex.Message);
			}
		}
	}

	private void ButtonResult(object sender, RoutedEventArgs e) {
		if (sender == ButtonOK) {
			if (HookupName.Length == 0 || ModelLocation.Length == 0 || (IsAnimHookup && AnimationLocation.Length == 0)) {
				MessageBox.Show(this, GetString("MessageCreateErrNotFilled"));
				return;
			}
			var hookupBase = Paths.HookupFile(ProjectLocation, HookupName);
			using StreamWriter sw = new(hookupBase);
			sw.WriteLine("SiiNunit");
			sw.WriteLine("{");
			sw.WriteLine($"\t{(IsAnimHookup ? "animated_hookup" : "compass_hookup")} : {HookupName}");
			sw.WriteLine("\t{");
			sw.WriteLine($"\t\tmodel: \"{ModelLocation}\"");
			if (IsAnimHookup)
				sw.WriteLine($"\t\tanimation: \"{AnimationLocation}\"");
			sw.WriteLine("\t}");
			sw.WriteLine("}");

			MessageBox.Show(this, GetString("MessageCreateSuccess"));
		}
		Close();
	}
}


