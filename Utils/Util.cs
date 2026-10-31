using Pfim;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Utils {

	static partial class TextControl {
		public static void NumberOnly(object sender, TextCompositionEventArgs e) => e.Handled = RegexNumber().IsMatch(e.Text);

		[GeneratedRegex("[^0-9]+")]
		private static partial Regex RegexNumber();
		public static void FloatOnly(object sender, TextCompositionEventArgs e) {
			var currentText = ((TextBox)sender).Text;
			string input = e.Text;
			if (input.Length == 0) {
				e.Handled = false;
				return;
			}
			var match = RegexFloat().IsMatch(e.Text);
			if (match) {
				e.Handled = true;
				return;
			}
			if (input[0] == '.') {
				if (currentText.Length == 0 || currentText.Contains('.')) {
					e.Handled = true;
					return;
				}
			}
			e.Handled = false;
		}

		[GeneratedRegex("[^0-9.-]+")]
		private static partial Regex RegexFloat();
	}

	static class Util {
		public static string SystemLocaleForSCS() {
			var culture = System.Globalization.CultureInfo.InstalledUICulture.Name;
			return culture.Replace('-', '_').ToLower();
		}

		public static Window? MainWindow = null;
		public static string GetString(string key, params object[] args) {
			string res = Application.Current.FindResource(key).ToString()!;//"Object reference not set to an instance of an object." 如果字典的值为空就会出现这个报错
			if (args.Length > 0) {
				res = string.Format(res, args);
			}
			return res;
		}

		public static int GetInt(string key) => (int)Application.Current.FindResource(key);//"Object reference not set to an instance of an object." 如果字典的值为空就会出现这个报错

		public static string GetFilter(string key) {
			var specificFilter = GetString(key);
			var allFilter = GetString("FilterAllFiles");
			return $"{specificFilter}|{allFilter}";
		}

		public static string GetString(this Window window, string key, params object[] args) {
			string res = window.FindResource(key).ToString()!;
			if (args.Length > 0) {
				res = string.Format(res, args);
			}
			return res;
		}
		public static string GetFilter(this Window window, string key) {
			var specificFilter = window.GetString(key);
			var allFilter = window.GetString("FilterAllFiles");
			return $"{specificFilter}|{allFilter}";
		}

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
			switch (image.Format) {
				case ImageFormat.Rgb24:
					return PixelFormats.Bgr24;
				case ImageFormat.Rgba32:
					return PixelFormats.Bgra32;
				case ImageFormat.Rgb8:
					return PixelFormats.Gray8;
				case ImageFormat.R5g5b5a1:
				case ImageFormat.R5g5b5:
					return PixelFormats.Bgr555;
				case ImageFormat.R5g6b5:
					return PixelFormats.Bgr565;
				default:
					throw new Exception($"Unable to convert {image.Format} to WPF PixelFormat");
			}
		}
	}

	static class CollectionUtil {
		public static void AddItem<T>(DataGrid table, ObservableCollection<T> list, T newItem) {
			int selectedIndex = table.SelectedIndex;
			if (selectedIndex == -1) {
				selectedIndex = list.Count;
				list.Add(newItem);
			} else
				list.Insert(selectedIndex, newItem);
			table.SelectedIndex = selectedIndex;
			table.Focus();
		}

		public static void RemoveItem<T>(DataGrid table, ObservableCollection<T> list) {
			if (table.SelectedIndex == -1)
				return;
			while (table.SelectedItem != null) {
				list.Remove((T)table.SelectedItem);
			}
		}

		public static void MoveDataGridItems<T>(bool up, DataGrid table, ObservableCollection<T> list) {
			if (table.SelectedIndex == -1)
				return;
			int target = table.SelectedIndex;
			List<T> l = [];
			while (table.SelectedIndex != -1) {
				if (table.SelectedIndex < target) {
					target--;
				}
				l.Add(list[table.SelectedIndex]);
				list.RemoveAt(table.SelectedIndex);
			}
			if (up) {
				if (target > 0)
					target--;
			} else if (target < list.Count)
				target++;

			if (table.SelectionMode == DataGridSelectionMode.Extended) {
				var IList = table.SelectedItems;
				foreach (T pair in l) {
					list.Insert(target, pair);
					IList.Add(pair);
					target++;
				}
			} else {
				list.Insert(target, l.First());
				table.SelectedIndex = target;
			}
			table.Focus();
		}

		public static void MoveListBoxItems<T>(bool up, ListBox listBox, ObservableCollection<T> list) {
			if (listBox.SelectedIndex == -1)
				return;
			int target = listBox.SelectedIndex;
			List<T> l = [];
			while (listBox.SelectedIndex != -1) {
				if (listBox.SelectedIndex < target) {
					target--;
				}
				l.Add(list[listBox.SelectedIndex]);
				list.RemoveAt(listBox.SelectedIndex);
			}
			if (up) {
				if (target > 0)
					target--;
			} else if (target < list.Count)
				target++;

			if (listBox.SelectionMode == SelectionMode.Extended) {
				var IList = listBox.SelectedItems;
				foreach (T pair in l) {
					list.Insert(target, pair);
					IList.Add(pair);
					target++;
				}
			} else {
				list.Insert(target, l.First());
				listBox.SelectedIndex = target;
			}
			listBox.Focus();
		}
	}
}
