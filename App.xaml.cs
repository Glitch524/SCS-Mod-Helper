using Microsoft.Win32;
using SCS_Mod_Helper.Modding.Accessories.Physics;
using SCS_Mod_Helper.Utils;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SCS_Mod_Helper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App: Application {
	App() {
		Startup += App_Startup;
	}

	private void App_Startup(object sender, StartupEventArgs e) {
		ResourceUtil.SetLanguage();
		SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
		ResourceUtil.SetTheme();
	}

	private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e) {
		if (Instances.FollowSystem) {
			ResourceUtil.UpdateThemeDict(ResourceUtil.GetSystemTheme());
		}
	}

	private void UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
		var exceptionPath = Paths.ExcpPath();
		using StreamWriter sw = new(exceptionPath);
		sw.WriteLine(e.Exception.Message);
		sw.Write(e.Exception.StackTrace);
	}

	private void OnExit(object sender, ExitEventArgs e) {
		new AccessoryPhysicsIO().SavePhysics();
	}
}