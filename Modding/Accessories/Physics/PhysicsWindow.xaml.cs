using SCS_Mod_Helper.Class.Base;
using System.Windows;

namespace SCS_Mod_Helper.Modding.Accessories.Physics;
/// <summary>
/// PhysicsToyWindow.xaml 的交互逻辑
/// </summary>
/// 
public partial class PhysicsWindow: BaseWindow {

	private PhysicsData? mSelectedPhysicsData = null;
	public PhysicsData SelectedPhysicsData => mSelectedPhysicsData!;

	public bool ChooseMode = false;

	public PhysicsWindow() {
		InitializeComponent();
	}

	private void OnLoaded(object sender, RoutedEventArgs e) {
		TextMsgPhysics2.Visibility = ChooseMode ? Visibility.Visible : Visibility.Collapsed;
		PanelSelection.Visibility = ChooseMode ? Visibility.Visible : Visibility.Collapsed;
	}

	private void ButtonResultClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonCancel) {
			Close();
		} else if (sender == ButtonSelect) {
			mSelectedPhysicsData = UCPhysics.CurrentPhysicsItem;
			DialogResult = true;
			Close();
		}
	}
}