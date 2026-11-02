using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Main.About.AvatarNameService;
using SCS_Mod_Helper.Main.About.VersionCheck;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Wpf.Ui.Input;

namespace SCS_Mod_Helper.Main.About;
/// <summary>
/// AboutWindow.xaml 的交互逻辑
/// </summary>
public partial class AboutPage: BasePage {

	private readonly MainBinding Binding;

	public AboutPage() {
		InitializeComponent();

		var naviMain = Application.Current.Windows.OfType<NaviMainWindow>().First();
		Binding = naviMain.Binding;

		GridMain.DataContext = Binding;
		Binding.SelectedTab = 0;
	}

	private void HyperlinkClick(object sender, RequestNavigateEventArgs e) {
		Process.Start(new ProcessStartInfo {
			FileName = e.Uri.ToString(),
			UseShellExecute = true
		});
		e.Handled = true;
	}
}