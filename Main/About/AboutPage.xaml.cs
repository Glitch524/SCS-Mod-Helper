using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Main.About.AvatarNameService;
using SCS_Mod_Helper.Main.About.VersionCheck;
using SCS_Mod_Helper.Manifest;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Data;
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

		Loaded += OnLoaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e) {
		NaviMainWindow mainWindow = (NaviMainWindow)Window.GetWindow(this);
		var navigation = mainWindow.Navigation;
		GridMeasureSize.SetBinding(HeightProperty, new Binding(nameof(ActualHeight)) {//page在navigationview里会获得无限高度，所以任何可以滚动的控件都会无限拉长，因此将page高度绑定到navigationview的高度（并-1，nv的上面有一条border）即可限定高度
			Source = navigation,
			Mode = BindingMode.OneWay,
			Converter = new NavigationHeightConverter()
		});
	}

	private void HyperlinkClick(object sender, RequestNavigateEventArgs e) {
		Process.Start(new ProcessStartInfo {
			FileName = e.Uri.ToString(),
			UseShellExecute = true
		});
		e.Handled = true;
	}
}