using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Locale;
using SCS_Mod_Helper.Modding.Accessories.Physics;
using SCS_Mod_Helper.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCS_Mod_Helper.Modding.Accessories.AccHookup;

/// <summary>
/// AddonHookup.xaml 的交互逻辑
/// </summary>
public partial class AccHookupWindow: BaseWindow {
	private readonly AccHookupBinding Binding;

	private readonly ContextMenu MenuSuitableFor;

	public AccHookupWindow() {
		InitializeComponent();

		Binding = new(() => this);
		GridMain.DataContext = Binding;

		MenuSuitableFor = (ContextMenu)Resources["MenuSuitableFor"];
		MenuSuitableFor.PlacementTarget = ButtonSelectSuitableFor;

		Binding.SuiChanged += OnSuiChanged;

		Loaded += OnLoaded;

	}

	private void OnLoaded(object sender, RoutedEventArgs e) {
		Task.Run(Binding.LoadFiles);
	}

	void OnSuiChanged(SuiItem? suiItem) => UCPhysics.CurrentSuiItem = suiItem;

	private void ButtonAddSuiClick(object sender, RoutedEventArgs e) {
		SuiItem newItem = new("new_sui");
		Binding.SuiItems.Add(newItem);
		Binding.CurrentSuiItem = newItem;
	}

	private void ButtonDeleteSuiClick(object sender, RoutedEventArgs e) {
		if (sender is Button button) {
			var SuiItem = (SuiItem)button.DataContext;
			Binding.SuiItems.Remove(SuiItem);

			if (Binding.CurrentSuiItem == SuiItem) {
				Binding.CurrentSuiItem = Binding.SuiItems.FirstOrDefault();

			}
		}
	}

	private void ButtonAddRowClick(object sender, RoutedEventArgs e) {
		AccessoryHookupData newItem = new();
		Binding.Hookups?.Add(newItem);
		Binding.CurrentHookupItem = newItem;
	}

	private void ButtonDeleteRowClick(object sender, RoutedEventArgs e) {
		if (Binding.CurrentHookupItem != null)
			Binding.Hookups?.Remove(Binding.CurrentHookupItem);
	}

	private void SortButtonClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonToyUp || sender == ButtonToyDown) {
			CollectionUtil.MoveListBoxItems(sender == ButtonToyUp, ListBoxHookup, Binding.Hookups!);
		}
	}

	private void OnMenuClicked(object sender, RoutedEventArgs e) {
		MenuItem item = (MenuItem)sender;
		ContextMenu cm = (ContextMenu)item.Parent ?? item.ContextMenu;
		if (cm == MenuSuitableFor) {
			var newSF = (string)item.Tag;
			Binding.NewSuitableFor = newSF;
			Binding.AddNewSuitableFor();
			TextSuitableFor.Focus();
		}
	}

	private void ButtonChooseIcon(object sender, RoutedEventArgs e) => Binding.ChooseIcon();

	private void ButtonChooseModelClick(object sender, RoutedEventArgs e) {
		try {
			if (sender is Button button) {
				ChooseModelType chooseType = (ChooseModelType)button.Tag;
				Binding.ChooseModel(chooseType);
			}
		} catch (Exception ex) {
			MessageBox.Show(this, ex.Message);
		}
	}

	private void NumberOnly(object sender, TextCompositionEventArgs e) => TextControl.NumberOnly(sender, e);
	private void ButtonListClick(object sender, RoutedEventArgs e) {
		TextBox target;
		if (sender == ButtonAddData) {
			target = TextData;
			Binding.AddNewData();
		} else if (sender == ButtonAddSuitableFor) {
			target = TextSuitableFor;
			Binding.AddNewSuitableFor();
		} else if (sender == ButtonAddConflictWith) {
			target = TextConflictWith;
			Binding.AddNewConflictWith();
		} else if (sender == ButtonAddDefaults) {
			target = TextDefaults;
			Binding.AddNewDefaults();
		} else if (sender == ButtonAddOverrides) {
			target = TextOverrides;
			Binding.AddNewOverrides();
		} else if (sender == ButtonAddRequire) {
			target = TextRequire;
			Binding.AddNewRequire();
		} else
			return;
		target.Focus();
	}

	private void ButtonChooseSuitableFor(object sender, RoutedEventArgs e) => MenuSuitableFor.IsOpen = true;

	private void ListDataGotFocus(object sender, RoutedEventArgs e) => ShowPopupListData((TextBox)sender);

	private void ListDataLostFocus(object sender, RoutedEventArgs e) => PopupListData.IsOpen = false;

	private void ButtonPhysicsClick(object sender, RoutedEventArgs e) {
		PhysicsWindow physicsWindow = new() {
			Owner = this,
			ChooseMode = true,
		};
		if (physicsWindow.ShowDialog() == true) {
			var selected = physicsWindow.SelectedPhysicsData;
			Binding.AddNewData(selected);
			TextData.Focus();

			UCPhysics.PhysicsListAdd(selected);
		}
	}

	private void AddToDataClick(object sender, RoutedEventArgs e) {
		var newPhys = UCPhysics.CurrentPhysicsItem;
		Binding.AddNewData(newPhys);
		TextData.Focus();
	}

	private void ButtonSuitableForClick(object sender, RoutedEventArgs e) {
		MenuSuitableFor.PlacementTarget = (Button)sender;
		MenuSuitableFor.IsOpen = true;
	}

	private void ShowPopupListData(TextBox target) {
		if (Binding.ListDataUC == null) {
			Binding.ListDataUC = new();
			PopupListData.Child = Binding.ListDataUC;
		}
		Binding.OpeningList = target.Name;
		PopupListData.PlacementTarget = target;
		if (target == TextData || target == TextSuitableFor) {
			PopupListData.Width = BorderData.ActualWidth;
		} else
			PopupListData.Width = BorderConflictWith.ActualWidth;
		PopupListData.IsOpen = true;
	}

	private void ButtonResultClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonCancel) {
			Close();
		} else if (sender == ButtonSave) {
			Binding.SaveFiles();
		}
	}
}