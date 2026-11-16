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
		Loaded += OnLoaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e) {
		Binding.LocaleModules.FirstOrDefault()?.Expanded = true;
	}

	private void ButtonClick(object sender, RoutedEventArgs e) {
		var modLocalization = new ModLocalizationWindow() {
			Owner = IWindow
		};
		modLocalization.ShowDialog();
		if (modLocalization.HasChanges) {
			Binding.UpdateLayout();
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

	private void StringResSelectedChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
		if (e.NewValue is LocalePair localePair) {
			localePair.Selected = false;
			Binding.PopupStringResOpen = false;
			ApplyStringResId?.Invoke(localePair.Key);
		}
	}
}

public partial class StringResBinding: WindowBinding {
	public ObservableCollection<LocaleModule> LocaleModules => Instances.LocaleModules;

	public bool? HasValue {
		get {
			if (field == null) {
				if (LocaleModules.Count == 0)
					field = false;
				foreach (var module in LocaleModules) {
					if (module.UniversalDict.Count > 0) {
						field = true;
						return field;
					}
				}
				field = false;
			}
			return field;
		}
		set {
			field = value;
			InvokeChange();
		}
	} = null;
	public void UpdateLayout() {
		HasValue = null;
		InvokeChange(nameof(LocaleModules));
	}

}
