using Pfim;
using System.Diagnostics;
using System.IO;
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

	public static async Task ZipLock(string zipFile, uint fakeCrc = 0x00000000) {
		byte[] data = await File.ReadAllBytesAsync(zipFile);
		byte[] LocalHeaderSig = [0x50, 0x4B, 0x03, 0x04];
		byte[] centralDirSig = [0x50, 0x4B, 0x01, 0x02];
		for (int i = 0; i <= data.Length - 4; i++) {
			if (SigMatch(data, i, LocalHeaderSig)) {
				int offset = i + 6;
				if (offset + 1 < data.Length) {
					WriteLock(data, offset);
					WriteCrc(data, offset + 8, fakeCrc);
				}
				i += 29;
			}
		}
		for (int i = 0; i <= data.Length - 4; i++) {
			if (SigMatch(data, i, centralDirSig)) {
				int offset = i + 8;
				if (offset < data.Length) {
					WriteLock(data, offset);
					WriteCrc(data, offset + 8, fakeCrc);
				}
				i += 45;
			}
		}
		await File.WriteAllBytesAsync(zipFile, data);
	}

	private static bool SigMatch(byte[] data, int offset, byte[] sig) {
		for (int i = 0; i < sig.Length; i++) {
			if (data[offset + i] != sig[i])
				return false;
		}
		return true;
	}

	private static void WriteLock(byte[] data, int offset) {
		if (offset + 1 >= data.Length)
			return;
		data[offset] = 0x09;
		data[offset + 1] = 0x00;
	}

	private static void WriteCrc(byte[] data, int offset, uint value) {
		if (offset + 3 >= data.Length)
			return;
		data[offset] = (byte)(value & 0xFF);
		data[offset + 1] = (byte)((value >> 8) & 0xFF);
		data[offset + 1] = (byte)((value >> 16) & 0xFF);
		data[offset + 1] = (byte)((value >> 24) & 0xFF);
	}
}