using SCS_Mod_Helper.Class.Base;
using System.Windows;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon;
/// <summary>
/// CreateIconFiles.xaml 的交互逻辑
/// </summary>
public partial class IconDefWIndow: BaseWindow {
	bool NotInAcc = false;
	public IconDefWIndow(bool notInAcc, bool atAcc)// notInAcc表示图标没有在配件文件夹内
{
		InitializeComponent();
		if (notInAcc) {
			TextNotInAcc.Visibility = Visibility.Visible;
		} else if (atAcc)
			BoxCreateOnAccessory.Visibility = Visibility.Collapsed;
		else {
			BoxCreateOnAccessory.Visibility = Visibility.Visible;
			BoxCreateOnAccessory.IsChecked = AccAddonHistory.Default.IconDefCreateOnAccessory;
		}
	}

	public bool CreateOnAccessory => BoxCreateOnAccessory.IsChecked == true || NotInAcc;

	private void Button_Click(object sender, RoutedEventArgs e) {
		AccAddonHistory.Default.IconDefCreateOnAccessory = BoxCreateOnAccessory.IsChecked == true;
		AccAddonHistory.Default.Save();
		DialogResult = sender == ButtonYes;
	}
}
