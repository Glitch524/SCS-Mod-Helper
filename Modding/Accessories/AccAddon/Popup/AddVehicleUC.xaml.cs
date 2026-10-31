using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon;

/// <summary>
/// AddVehicleUC.xaml 的交互逻辑
/// </summary>
public partial class AddVehicleUC: BaseUserControl {


	private AccAddonBinding? mBinding = null;
	public AccAddonBinding Binding {
		get {
			mBinding ??= (AccAddonBinding) GridMain.DataContext;
			return mBinding;
		}
	}

	public AddVehicleUC() {
		InitializeComponent();
	}

	public void Clean() {
		TextNewVehicleID.Text = string.Empty;
		TextNewVehicleType.SelectedValue = "Truck";
		TextNewDisplayName.Text = string.Empty;
		TextNewManifaturer.Text = string.Empty;
		TextNewVehicleProdYear.Text = null;
		TextNewDescription.Text = string.Empty;
	}

	private void ButtonResultClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonOK) {
			if (TextNewVehicleID.Text.Length == 0 || TextNewDisplayName.Text.Length == 0) {
				MessageBox.Show(Window.GetWindow(this), Util.GetString("MessageAddErrNotFilled"));
				return;
			}
			int prod;
			if (string.IsNullOrEmpty(TextNewVehicleProdYear.Text))
				prod = DateTime.Now.Year;
			else
				prod = int.Parse(TextNewVehicleProdYear.Text);
			AccessoryVehicle newVehicle = new(TextNewVehicleID.Text,
				  prod,
				  TextNewDisplayName.Text,
				  TextNewDescription.Text,
				  (string)TextNewVehicleType.SelectedValue,
				  TextNewManifaturer.Text,
				  Binding.AddVehicleETS2 == true,
				  false);
			Clean();
			VehiclesIO.AddVehicle(newVehicle);
			Binding.AddNewVehicle(newVehicle);
			Binding.AddVehicleETS2 = null;
			Binding.PopupAddVehicleOpen = false;
		} else if (sender == ButtonCancel)
			Binding.PopupAddVehicleOpen = false;
	}

	private void NumberOnly(object sender, TextCompositionEventArgs e) => TextControl.NumberOnly(sender, e);
}

public partial class AccAddonBinding {
	public bool? AddVehicleETS2 = null;

	public void AddNewVehicle(AccessoryVehicle newVehicle) {
		try {
			bool isETS2 = newVehicle.IsETS2;
			ObservableCollection<AccessoryVehicle> Vehicles = isETS2 ? VehiclesETS2 : VehiclesATS;
			if (newVehicle.VehicleID[0] <= 'm') {
				for (int i = 0; i < Vehicles.Count; i++) {
					var cVehicle = Vehicles[i];
					var cResult = newVehicle.CompareTo(cVehicle);
					if (cResult == 0) {
						throw new(Util.GetString("MessageAddErrSameID"));
					} else if (cResult < 0) {
						Vehicles.Insert(i, newVehicle);
						break;
					} else if (i == Vehicles.Count - 1)
						Vehicles.Add(newVehicle);
				}
			} else {
				for (int i = Vehicles.Count - 1; i >= 0; i--) {
					var cVehicle = Vehicles[i];
					var cResult = newVehicle.VehicleID.CompareTo(cVehicle.VehicleID);
					if (cResult == 0) {
						throw new(Util.GetString("MessageAddErrSameID"));
					} else if (cResult > 0) {
						Vehicles.Insert(i + 1, newVehicle);
						break;
					} else if (i == 0)
						Vehicles.Insert(0, newVehicle);
				}
				Vehicles.Insert(0, newVehicle);
			}
			VehiclesIO.AddVehicle(newVehicle);
		} catch (Exception ex) {
			MessageBox.Show(BindingWindow, ex.Message, Util.GetString("MessageTitleErr"));
		}
	}
}