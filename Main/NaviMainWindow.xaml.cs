using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.ConverterPix;
using SCS_Mod_Helper.Locale.Localization;
using SCS_Mod_Helper.Modding.PaintJob;
using SCS_Mod_Helper.Setting;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Wpf.Ui.Input;

namespace SCS_Mod_Helper.Main;
/// <summary>
/// NaviMainWindow.xaml 的交互逻辑
/// </summary>
public partial class NaviMainWindow: BaseWindow {
	public MainBinding Binding;
	public NaviMainWindow() {
		InitializeComponent();
		Binding = new(() => this, OpenWindow, Test);
		GridMain.DataContext = Binding;

	}

	protected override void OnContentRendered(EventArgs e) {
		base.OnContentRendered(e);
		VehiclesIO.CheckDBMigration(this);
		_ = Task.Delay(100).ContinueWith(_ => SystemVerification.StorageAsync());

		Binding.AutoCheckUpdate();
	}

	private void OpenWindow(string? type) {
		if (type == null)
			return;
		Window window;
		switch (type) {
			case "ModLocalization":
				window = new ModLocalizationWindow();
				break;
			case "PaintJob":
				window = new PaintJobWindow();
				break;
			case "ConverterPIX":
				var PixExe = Instances.ConverterPixExe;
				if (string.IsNullOrEmpty(PixExe) || !File.Exists(PixExe)) {
					var result = MessageBox.Show(GetString("MessagePixNotSet"), GetString("MessageTitleNotice"));
					if (result == MessageBoxResult.OK) {
						Navigation.Navigate("Settings");
					}
					return;
				}
				if (Settings.Default.ConverterPixNotice) {
					ConverterNoticeBox notice = new() {
						Owner = this
					};
					if (notice.ShowDialog() == true) {
						if (notice.DontShowAgain) {
							Settings.Default.ConverterPixNotice = false;
							Settings.Default.Save();
						}
					} else
						return;
				}
				window = new ConverterPixWindow();
				break;
			default:
				return;
		}
		window.Owner = this;
		window.ShowDialog();
	}

	private void Test(string? value) {
		throw new Exception("test");
	}

	private void NaviMainWindowLoaded(object sender, RoutedEventArgs e) => Navigation.Navigate("ModManifest");
}

public partial class MainBinding(Func<IBaseVisual> visualFunc, Action<string?> windowAction, Action<string?> testAction): WindowBinding(visualFunc) {

	public RelayCommand<string> WindowCommand { get; } = new RelayCommand<string>(windowAction);

	public RelayCommand<string> TestCommand { get; } = new RelayCommand<string>(testAction);

	public static Visibility TestVisibility => Debugger.IsAttached ? Visibility.Visible : Visibility.Collapsed;
}
