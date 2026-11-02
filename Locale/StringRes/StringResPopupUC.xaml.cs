using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Locale.Localization;
using SCS_Mod_Helper.Modding.Accessories;
using SCS_Mod_Helper.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SCS_Mod_Helper.Locale.StringRes;

/// <summary>
/// StringResPopupUC.xaml 的交互逻辑
/// </summary>
public partial class StringResPopupUC: BaseUserControl {
	public Action<string>? ApplyStringResId { get; set; }
	private StringResBinding Binding => (StringResBinding)PanelMain.DataContext;
	public StringResPopupUC() {
		InitializeComponent();
	}

	private void ButtonClick(object sender, RoutedEventArgs e) {
		var modLocalization = new ModLocalizationWindow() {
			Owner = IWindow
		};
		modLocalization.ShowDialog();
		if (modLocalization.HasChanges) {
			StringResUtil.SetupStringResList();
		}
	}

	private void StringResSelectionChanged(object sender, SelectionChangedEventArgs e) {
		if (sender is ListBox listBox) {
			if (e.AddedItems.Count == 0)
				return;
			var selectedItem = e.AddedItems[0];
			listBox.SelectedIndex = -1;

			if (selectedItem is StringResItem item) {
				ApplyStringResId?.Invoke(item.Header);
			}
		}
	}
}

public partial class StringResBinding: WindowBinding {
	public static ObservableCollection<StringResItem> StringResList => StringResUtil.StringResList;
}
