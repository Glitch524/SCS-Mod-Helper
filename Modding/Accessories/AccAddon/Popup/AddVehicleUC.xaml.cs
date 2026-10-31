using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;
using System.Collections.ObjectModel;
using System.Windows;
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
		TextNewVehicleType.SelectedValue = "Truck";
		TextNewDisplayName.Text = string.Empty;
		TextNewManufacturer.Text = string.Empty;
		TextNewVehicleProdYear.Text = null;
		TextNewDescription.Text = string.Empty;
	}

	private void ButtonResultClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonOK) {
			Binding.AddNewVehicleClick();
			Binding.PopupAddVehicleOpen = false;
		} else if (sender == ButtonCancel)
			Binding.PopupAddVehicleOpen = false;
	}

	private void NumberOnly(object sender, TextCompositionEventArgs e) => TextControl.NumberOnly(sender, e);
}

public partial class AccAddonBinding {
	public bool? AddVehicleETS2 = null;

	public void AddNewVehicleClick() {
		if (NewManufacturer.Length == 0 || 
			NewVehicleCodename.Length == 0 ||
			NewVehicleName.Length == 0) {
			MessageBox.Show(BindingWindow, Util.GetString("MessageAddErrNotFilled"));
			return;
		}
		int prod;
		if (string.IsNullOrEmpty(NewProdYear))
			prod = DateTime.Now.Year;
		else
			prod = int.Parse(NewProdYear);
		AccessoryVehicle newVehicle = new(NewVehicleID,
			  prod,
			  NewVehicleName,
			  NewDescription ?? string.Empty,
			  NewVehicleType,
			  NewManufacturer,
			  AddVehicleETS2 == true,
			  false);
		Clean();
		VehiclesIO.AddVehicle(newVehicle);
		AddNewVehicle(newVehicle);
		AddVehicleETS2 = null;
	}

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

	public void Clean() {
		NewManufacturer = string.Empty;
		NewVehicleCodename = string.Empty;
		NewProdYear = string.Empty;
		NewVehicleType = "Truck";
		NewVehicleName = string.Empty;
		NewDescription = string.Empty;
	}

	public string NewVehicleID => (mNewManufacturer?.Length > 0 || mNewVehicleCodename?.Length > 0) ? $"{mNewManufacturer}.{mNewVehicleCodename}" : string.Empty;
	private string mNewManufacturer = string.Empty;
	public string NewManufacturer {
		get => mNewManufacturer;
		set {
			mNewManufacturer = value;
			InvokeChange();
			InvokeChange(nameof(NewVehicleID));
		}
	}
	private string mNewVehicleCodename = null;
	public string NewVehicleCodename {
		get => mNewVehicleCodename;
		set {
			mNewVehicleCodename = value;
			InvokeChange();
			InvokeChange(nameof(NewVehicleID));
		}
	}
	private string mNewProdYear = null;
	public string NewProdYear {
		get => mNewProdYear;
		set {
			mNewProdYear = value;
			InvokeChange();
		}
	}
	public static string NewProdYearPlaceholder => DateTime.Now.Year.ToString();
	private string mNewVehicleType = "Truck";
	public string NewVehicleType {
		get => mNewVehicleType;
		set {
			mNewVehicleType = value;
			InvokeChange();
		}
	}
	private string mNewVehicleName = null;
	public string NewVehicleName {
		get => mNewVehicleName;
		set {
			mNewVehicleName = value;
			InvokeChange();
		}
	}
	private string mNewDescription = null;
	public string NewDescription {
		get => mNewDescription;
		set {
			mNewDescription = value;
			InvokeChange();
		}
	}
}