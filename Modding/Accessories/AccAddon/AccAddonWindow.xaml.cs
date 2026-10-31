using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Locale;
using SCS_Mod_Helper.Modding.Accessories.Physics;
using SCS_Mod_Helper.Vehicles;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class AccAddonWindow: BaseWindow {
	private readonly AccAddonBinding Binding;
	private bool VehicleExpandedETS2 {
		get => Binding.VehicleExpandedETS2; 
		set => Binding.VehicleExpandedETS2 = value;
	}
	private bool VehicleExpandedATS {
		get => Binding.VehicleExpandedATS; 
		set => Binding.VehicleExpandedATS = value;
	}

	private ObservableCollection<AccessoryVehicle> VehiclesETS2 => Binding.VehiclesETS2;
	private ObservableCollection<AccessoryVehicle> VehiclesATS => Binding.VehiclesATS;

	private readonly ContextMenu MenuModelType, MenuLook, MenuVariant;

	public AccAddonWindow() {
		InitializeComponent();

		Binding = new(() => this);
		GridMain.DataContext = Binding;

		MenuModelType = (ContextMenu)Resources["MenuModelType"];
		MenuModelType.ItemsSource = Binding.ModelTypes;
		MenuLook = (ContextMenu)Resources["MenuLook"];
		MenuLook.ItemsSource = Binding.LookList;
		MenuVariant = (ContextMenu)Resources["MenuVariant"];
		MenuVariant.ItemsSource = Binding.VariantList;

		Loaded += OnLoaded;
	}

	private async void OnLoaded(object sender, RoutedEventArgs e) {
		var tLoad = Binding.LoadVehicles();
		await tLoad;
		Binding.LoadHistory();
		Binding.ModelIcon = AccessoryDataUtil.LoadModelIconByIconName(Binding.IconName);

		Binding.LoadLooksAndVariants();
		Binding.UpdateOthersChecked();
		Binding.Loading = false;
	}

	private void OnMenuClicked(object sender, RoutedEventArgs e) {
		MenuItem item = (MenuItem)sender;
		var menuName = (string)item.CommandParameter;
		ContextMenu cm = menuName switch {
			nameof(MenuModelType) => MenuModelType,
			nameof(MenuLook) => MenuLook,
			nameof(MenuVariant) => MenuVariant,
			_ => throw new ArgumentException("")
		};
		AccessoryVehicle vehicle = (AccessoryVehicle)cm.DataContext;
		if (cm == MenuModelType) {
			var acc = (AccessoryType) item.DataContext;
			if (vehicle.IsETS2) {
				if (acc.ForETS2)
					vehicle.ModelType = acc.AccessoryETS2!;
			} else {
				if (acc.ForATS)
					vehicle.ModelType = acc.AccessoryATS!;
			}
		} else if (cm == MenuLook) {
			vehicle.Look = (string)item.DataContext;
		} else if (cm == MenuVariant) {
			vehicle.Variant = (string)item.DataContext;
		} else
			return;
	}

	private void SaveOnClosing(object? sender, CancelEventArgs e) => Binding.SaveHistory();

	private void ButtonChooseIcon(object sender, RoutedEventArgs e) => Binding.ChooseIcon();

	private void ButtonChooseModelClick(object sender, RoutedEventArgs e) {
		try {
			int type;
			if (sender == ButtonChooseModel)
				type = AccAddonBinding.MODEL;
			else if (sender == ButtonChooseModelUK)
				type = AccAddonBinding.MODEL_UK;
			else if (sender == ButtonChooseExtModel)
				type = AccAddonBinding.EXT_MODEL;
			else if (sender == ButtonChooseExtModelUK)
				type = AccAddonBinding.EXT_MODEL_UK;
			else if (sender == ButtonChooseColl)
				type = AccAddonBinding.MODEL_COLL;
			else
				return;
			Binding.ChooseModel(type);
		} catch (Exception ex) {
			MessageBox.Show(this, ex.Message);
		}
	}

	private void ButtonHideIn(object sender, RoutedEventArgs e) {
		if (Binding.HideInUC == null) {
			Binding.HideInUC = new(Binding);
			PopupHideIn.Child = Binding.HideInUC;
		}
		PopupHideIn.IsOpen = true;
	}

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

	private void ListDataGotFocus(object sender, RoutedEventArgs e) {
		ShowPopupListData((TextBox)sender);
	}

	private void ListDataLostFocus(object sender, RoutedEventArgs e) {
		PopupListData.IsOpen = false;
	}

	private void ButtonPhysicsClick(object sender, RoutedEventArgs e) {
		PhysicsWindow physicsWindow = new() {
			Owner = this,
			ChooseMode = true,
		};
		var result = physicsWindow.ShowDialog();
		if (result == true) {
			var selected = physicsWindow.SelectedPhysicsData;
			var physName = selected.PhysicsName;
			if (!physName.EndsWith(AccSCSIO.NamePSuffix))
				physName += AccSCSIO.NamePSuffix;
			Binding.AddNewData(physName);
			TextData.Focus();

			int i = 0;
			while (i < Binding.PhysicsList.Count) {
				var p = Binding.PhysicsList[i];
				if (p.PhysicsName == selected.PhysicsName) {
					Binding.PhysicsList.Remove(p);
					continue;
				}
				i++;
			}
			Binding.PhysicsList.Add(selected);
		}
	}

	private void ShowPopupListData(TextBox target) {
		if (Binding.ListDataUC == null) {
			Binding.ListDataUC = new();
			PopupListData.Child = Binding.ListDataUC;
		}
		Binding.OpeningList = target.Name;
		PopupListData.PlacementTarget = target;
		PopupListData.IsOpen = true;
	}

	private void NumberOnly(object sender, TextCompositionEventArgs e) => e.Handled = RegexNumber().IsMatch(e.Text);

	[GeneratedRegex("[^0-9]+")]
	private partial Regex RegexNumber();

	private void ButtonVehicleReinit(object sender, RoutedEventArgs e) {
		Binding.PopupAddVehicleOpen = false;
		Binding.ReinitVehicleList();
	}

	private void ButtonAddVehicleClick(object sender, RoutedEventArgs e) {
		PopupAddVehicle.PlacementTarget = (Button)sender;
		if (sender == ButtonAddVehicleETS2) {
			Binding.AddVehicleETS2 = true;
		}
		PopupAddVehicle.IsOpen = true;
	}

	private void ButtonDeleteVehicleClick(object sender, RoutedEventArgs e) {
		if (TableVehiclesETS2.SelectedIndex == -1 && TableVehiclesATS.SelectedIndex == -1)
			return;
		if (sender == ButtonDeleteVehicleETS2) {
			AccAddonBinding.DeleteVehicle(true, VehiclesETS2, TableVehiclesETS2);
		} else if (sender == ButtonDeleteVehicleATS) {
			AccAddonBinding.DeleteVehicle(false, VehiclesATS, TableVehiclesATS);
		}
	}

	private void ButtonCoverValue(object sender, RoutedEventArgs e) {
		void ForeachValue(Action<AccessoryVehicle> value, Action<AccessoryVehicle>? valueATS = null) {
			if (VehicleExpandedETS2) {
				foreach (var t in VehiclesETS2) {
					if (t.Check)
						value(t);
				}
			}
			if (VehicleExpandedATS) {
				foreach (var t in VehiclesATS) {
					if (t.Check) {
						if (valueATS != null) {
							valueATS(t);
						} else {
							value(t);
						}
					}
				}
			}
		}
		if (sender == ButtonCoverModelType) {
			var acc = Binding.CurrentModelType;
			ForeachValue((t) => {
				if (acc.ForETS2)
					t.ModelType = acc.AccessoryETS2!;
			}, (t) => {
				if (acc.ForATS)
					t.ModelType = acc.AccessoryATS!;
			});
		} else if (sender == ButtonCoverLook) {
			ForeachValue((t) => t.Look = TextLook.Text);
		} else if (sender == ButtonCoverVariant) {
			ForeachValue((t) => t.Variant = TextVariant.Text);
		}
	}

	private void VehicleCheckBoxClick(object sender, RoutedEventArgs e) {
		CheckBox box = (CheckBox)sender;
		AccessoryVehicle selected = (AccessoryVehicle)box.DataContext;
		Binding.SetSelected(selected);
		Binding.OverwriteEmpty(selected, selected.Check);
	}
}