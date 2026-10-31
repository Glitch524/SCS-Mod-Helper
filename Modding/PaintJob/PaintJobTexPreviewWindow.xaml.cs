using SCS_Mod_Helper.Class.Base;

namespace SCS_Mod_Helper.Modding.PaintJob; 
/// <summary>
/// PaintJobTexPreviewWindow.xaml 的交互逻辑
/// </summary>
public partial class PaintJobTexPreviewWindow: BaseWindow {
	readonly PaintJobBinding Binding;
	public PaintJobTexPreviewWindow(PaintJobBinding binding) {
		InitializeComponent();
		Binding = binding;
		GridMain.DataContext = binding;
		Loaded += OnLoaded;
		Closing += OnClosing;
	}

	private void OnLoaded(object sender, System.Windows.RoutedEventArgs e) {
		if (Binding.PaintJobTexImage == null && Binding.PaintJobTex.Length > 0) 
			Binding.LoadTexImage(Binding.PaintJobTexRealPath);
		Binding.LoadColorChannel();
	}

	private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e) {
		Binding.PreviewWindow = null;
	}
}