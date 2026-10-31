using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Modding.Accessories.AccHookup;
using SCS_Mod_Helper.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCS_Mod_Helper.Modding.Accessories.Physics; 
/// <summary>
/// PhysicsToyUC.xaml 的交互逻辑
/// </summary>
public partial class PhysicsUC: BaseUserControl {

	private static readonly DependencyProperty CurrentPhysicsItemProperty = DependencyProperty.Register(
		nameof(CurrentPhysicsItem),
		typeof(PhysicsData),
		typeof(PhysicsUC));

	public PhysicsData? CurrentPhysicsItem {
		get => (PhysicsData)GetValue(CurrentPhysicsItemProperty); set => SetValue(CurrentPhysicsItemProperty, value);
	}

	private static readonly DependencyProperty LocalPhysicsProperty = DependencyProperty.Register(
		nameof(LocalPhysics),
		typeof(bool),
		typeof(PhysicsUC));
	public bool LocalPhysics {
		get => (bool)GetValue(LocalPhysicsProperty); set => SetValue(LocalPhysicsProperty, value);
	}


	private readonly PhysicsBinding Binding;


	private static readonly RoutedEvent AddToDataClickEvent = EventManager.RegisterRoutedEvent(
		"AddToDataClick",
		RoutingStrategy.Bubble,
		typeof(RoutedEventHandler),
		typeof(PhysicsUC));

	public event RoutedEventHandler? AddToDataClick {
		add => AddHandler(AddToDataClickEvent, value); remove => RemoveHandler(AddToDataClickEvent, value);
	}

	public SuiItem? CurrentSuiItem {
		get => Binding.CurrentSuiItem; set => Binding.CurrentSuiItem = value;
	}

	public string RopeMaterial {
		get => Binding.RopeMaterial; set => Binding.RopeMaterial = value;
	}

	public string PatchMaterial {
		get => Binding.PatchMaterial; set => Binding.PatchMaterial = value;
	}

	private readonly ContextMenu MenuPhysicsType;

	public PhysicsUC() {
		InitializeComponent();
		Binding = new(() => this);
		GridMain.DataContext = Binding;

		MenuPhysicsType = (ContextMenu)Resources["MenuPhysicsType"];

		Binding.physicsCallback += PhysicsCallback;
		Loaded += OnLoaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e) {
		Binding.LocalPhysics = LocalPhysics;
		if (LocalPhysics) {
			GridAngular.Visibility = Visibility.Visible;
			GridRope.Visibility = Visibility.Visible;
			Grid.SetRow(GridRope, 2);

			Binding.CurrentPhysicsItem = Binding.PhysicsItems.FirstOrDefault();
			PanelSaveButton.Visibility = Visibility.Collapsed;
		}
	}

	public void PhysicsCallback(PhysicsData? physics) => CurrentPhysicsItem = physics;

	public void PhysicsListAdd(PhysicsData physics) => Binding.PhysicsListAdd(physics);

	private void ButtonPhysicsClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonAddToDataClick) {
			RoutedEventArgs args = new(AddToDataClickEvent);
			RaiseEvent(args);
		} else if (sender == ButtonSaveToPhysList) {
			var physics = Binding.CurrentPhysicsItem;
			if (physics == null)
				return;
			AccessoryPhysicsIO.AddPhysicsToList(physics);
			MessageBox.Show(Window.GetWindow(this), Util.GetString("MessageResultPhysSaved"));
		}
	}

	private void ButtonAddRowClick(object sender, RoutedEventArgs e) => MenuPhysicsType.IsOpen = true;
	private void AddPhysicDataClick(object sender, RoutedEventArgs e) {
		MenuItem item = (MenuItem)sender;
		ContextMenu cm = (ContextMenu)item.Parent;
		if (cm == MenuPhysicsType) {
			PhysicsData newPhys;
			if (item.Name == "PhysicsToyData") {
				newPhys = new PhysicsToyData();
			} else if (item.Name == "PhysicsPatchData") {
				newPhys = new PhysicsPatchData();
			} else
				return;
			Binding.PhysicsItems?.Add(newPhys);
			Binding.CurrentPhysicsItem = newPhys;
		}
	}

	private void ButtonDeleteRowClick(object sender, RoutedEventArgs e) {
		if (Binding.CurrentPhysicsItem != null)
			Binding.PhysicsItems?.Remove(Binding.CurrentPhysicsItem);
	}
	private void SortButtonClick(object sender, RoutedEventArgs e) {
		CollectionUtil.MoveListBoxItems(sender == ButtonPhysicUp, ListBoxPhysics, Binding.PhysicsItems!);
	}

	private void ButtonChooseModelClick(object sender, RoutedEventArgs e) {
		try {
			Binding.ChooseModel(sender == ButtonChoosePhysicsColl);
		} catch (Exception ex) {
			MessageBox.Show(Window.GetWindow(this), ex.Message);
		}
	}

	private void NumberOnly(object sender, TextCompositionEventArgs e) => TextControl.NumberOnly(sender, e);

	private void FloatOnly(object sender, TextCompositionEventArgs e) => TextControl.FloatOnly(sender, e);

	private void ButtonInstanceOffsetClicked(object sender, RoutedEventArgs e) {
		if (sender == ButtonInstanceOffsetList) {
			PopupInstanceOffsetList.IsOpen = true;
		}
	}

	private void ButtonChooseMaterialClick(object sender, RoutedEventArgs e) {
		try {
			if (sender == ButtonChooseRopeMaterial) {
				var mat = AccessoryDataUtil.ChooseRope();
				if (mat != null)
					RopeMaterial = mat;
			} else if (sender == ButtonChoosePatchMaterial) {
				var mat = AccessoryDataUtil.ChoosePatch();
				if (mat != null)
					PatchMaterial = mat;
			}
		} catch (Exception ex) {
			MessageBox.Show(Window.GetWindow(this), ex.Message);
		}
	}
}
