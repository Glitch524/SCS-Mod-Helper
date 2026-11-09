using Pfim;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Utils; 

static class Util {
	public static bool IsDebug() => Debugger.IsAttached;
	public static string SystemLocaleForSCS() {
		var culture = System.Globalization.CultureInfo.InstalledUICulture.Name;
		return culture.Replace('-', '_').ToLower();
	}

	public static Window? MainWindow = null;

	public static bool IsEmpty(params string[] values) {
		foreach (var v in values) {
			if (v.Length == 0)
				return true;
		}
		return false;
	}


	public static void OpenFile(string filename) {
		Process pro = new() {
			StartInfo = new(filename) { UseShellExecute = true }
		};
		pro.Start();
	}

	public static BitmapImage LoadImage(string filename) {
		var image = new BitmapImage();
		image.BeginInit();
		image.CacheOption = BitmapCacheOption.OnLoad;
		image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
		image.UriSource = new(filename);
		image.EndInit();
		return image;
	}

	public static BitmapSource LoadPfimImage(string filename) {
		var image = Pfimage.FromFile(filename);
		var pixelFormat = PfimGetPixelFormat(image);
		var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
		try {
			var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
			return BitmapSource.Create(image.Width, image.Height, 96, 96, pixelFormat, null, data, image.DataLen, image.Stride);
		} finally {
			handle.Free();
		}
	}

	public static PixelFormat PfimGetPixelFormat(IImage image) {
		return image.Format switch {
			ImageFormat.Rgb24 => PixelFormats.Bgr24,
			ImageFormat.Rgba32 => PixelFormats.Bgra32,
			ImageFormat.Rgb8 => PixelFormats.Gray8,
			ImageFormat.R5g5b5a1 or ImageFormat.R5g5b5 => PixelFormats.Bgr555,
			ImageFormat.R5g6b5 => PixelFormats.Bgr565,
			_ => throw new Exception($"Unable to convert {image.Format} to WPF PixelFormat"),
		};
	}
}