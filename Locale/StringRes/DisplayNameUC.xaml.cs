using CommunityToolkit.Mvvm.Input;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Modding.Accessories;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace SCS_Mod_Helper.Locale.StringRes; 
/// <summary>
/// StringResControl.xaml 的交互逻辑
/// </summary>
public partial class DisplayNameUC: BaseUserControl {
	public static readonly DependencyProperty AccessoryDataProperty = DependencyProperty.Register(
		nameof(AccessoryData),
		typeof(AccessoryData),
		typeof(DisplayNameUC),
		new FrameworkPropertyMetadata(
			null,
			FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
			OnDisplayNameChanged));

	public AccessoryData? AccessoryData {
		get => (AccessoryData) GetValue(AccessoryDataProperty);
		set => SetValue(AccessoryDataProperty, value);
	}

	/// <summary>
	/// 当使用这个User Control绑定的值改变时，触发这个方法
	/// 将新的值传给binding内对应的变量
	/// </summary>
	/// <param name="d">发生属性更改的依赖对象。</param>
	/// <param name="e">包含事件数据的属性更改参数。</param>
	private static void OnDisplayNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is DisplayNameUC uc) {
			switch(e.Property.Name) {
				case nameof(AccessoryData):
					uc.Binding.AccessoryData = e.NewValue as AccessoryData;
					break;
			}
		}
	}



	private readonly StringResBinding Binding;

	public DisplayNameUC() {
		InitializeComponent();

		Binding = new StringResBinding(() => this);

		PanelMain.DataContext = Binding;

		StringResUC.ApplyStringResId = ApplyStringRes;
	}

	private void CheckStringRes(object sender, RoutedEventArgs e) => Binding.CheckNameStringRes();


	public void ApplyStringRes(string resId) {
		TextDisplayName.SelectedText = $"@@{resId}@@";
		TextDisplayName.Focus();
	}
}

public partial class StringResBinding(Func<IBaseVisual> windowFunc): WindowBinding(windowFunc) {

	private AccessoryData? mAccessoryData = null;
	public AccessoryData? AccessoryData {
		get => mAccessoryData;
		set {
			mAccessoryData = value;
			InvokeChange();
			InvokeChange(nameof(DisplayName));
		}
	}

	public string DisplayName {
		get => AccessoryData?.DisplayName ?? string.Empty;
		set {
			AccessoryData?.DisplayName = value;

			CheckResResult = null;

			InvokeChange();
			InvokeChange(nameof(CheckResVisibility));
		}
	}
	public Visibility CheckResVisibility => DisplayName.Contains("@@") ? Visibility.Visible : Visibility.Collapsed;

	public string? LocalizedDisplayName {
		get {
			if (AccessoryData == null)
				return null;
			if (AccessoryData.LocalizedDisplayName == null) {
				StringResUtil.GetStringResResults(DisplayName, out var localizedName);
				AccessoryData.LocalizedDisplayName = localizedName;
			}
			return AccessoryData.LocalizedDisplayName;
		}
		set {
			AccessoryData?.LocalizedDisplayName = value;
			InvokeChange();
		}
	}

	private string? mCheckResResult = null;
	public string? CheckResResult {
		get => mCheckResResult;
		set {
			mCheckResResult = value;
			InvokeChange();
		}
	}

	private bool mPopupCheckOpen = false;
	public bool PopupCheckOpen {
		get => mPopupCheckOpen;
		set {
			mPopupCheckOpen = value;
			InvokeChange();
		}
	}

	public ICommand CheckDisplayNameResult => new RelayCommand(CheckNameStringRes);
	public void CheckNameStringRes() {
		if (CheckResResult == null) {
			CheckResResult = StringResUtil.GetStringResResults(DisplayName, out var localized);
			LocalizedDisplayName = localized;
		}
		PopupCheckOpen = true;
	}

	private bool mPopupStringResOpen = false;
	public bool PopupStringResOpen {
		get => mPopupStringResOpen;
		set {
			mPopupStringResOpen = value;
			InvokeChange();
		}
	}

	public ICommand OpenPopupStringRes => new RelayCommand(() => PopupStringResOpen = true);
}