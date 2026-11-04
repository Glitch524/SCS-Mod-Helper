using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Vehicles.Packages;
using System.Windows.Media.Imaging;

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

	private void OnSizeChanged(object sender, System.Windows.SizeChangedEventArgs e) {
		var windowWidth = Width;
		var windowHeight = Height;

		var r0Height = TitleBar.ActualHeight;
		var r2Height = PanelBottom.ActualHeight;

		var remainHeight = windowHeight - r0Height - r2Height;

		var remainWidth = windowWidth - remainHeight;
		double measureSize;
		if (remainWidth < 300) {
			measureSize = windowWidth - 320;
		} else {
			measureSize = remainHeight;
		}
		if (measureSize < 0)
			measureSize = 0;
		GridPaintJob.Width = measureSize;
		GridPaintJob.Height = measureSize;
	}
}

public partial class PaintJobBinding {
	public string PaintJobTexRealPath => PaintJobData.PaintJobTexRealPath;
	public BitmapSource? PaintJobTexImage {
		get => PaintJobData.PaintJobTexImage;
		set {
			PaintJobData.PaintJobTexImage = value;
			InvokeChange();
		}
	}
	public BitmapSource? MaskChannelR {
		get => PaintJobData.MaskChannelR;
		set {
			PaintJobData.MaskChannelR = value;
			InvokeChange();
		}
	}
	public BitmapSource? MaskChannelG {
		get => PaintJobData.MaskChannelG;
		set {
			PaintJobData.MaskChannelG = value;
			InvokeChange();
		}
	}
	public BitmapSource? MaskChannelB {
		get => PaintJobData.MaskChannelB;
		set {
			PaintJobData.MaskChannelB = value;
			InvokeChange();
		}
	}

	private bool mShowCabinUV = true;

	public bool ShowCabinUV {
		get => mShowCabinUV;
		set {
			mShowCabinUV = value;
			InvokeChange();
		}
	}

	private int mUVOpacityValue = 100;

	public int UVOpacityValue {
		get => mUVOpacityValue;
		set {
			mUVOpacityValue = value;
			InvokeChange();
			InvokeChange(nameof(UVOpacity));
		}
	}

	public float UVOpacity => ((float)UVOpacityValue) / 100;

}