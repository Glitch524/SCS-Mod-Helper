using ColorPicker;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Modding.Accessories;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SCS_Mod_Helper.Modding.PaintJob {
	/// <summary>
	/// PaintJobWindow.xaml 的交互逻辑
	/// </summary>
	public partial class PaintJobWindow: BaseWindow {
		private readonly PaintJobBinding Binding;

		public PaintJobWindow() {
			InitializeComponent();

			Binding = new(() => this);
			GridMain.DataContext = Binding;
		}

		private void NumberOnly(object sender, TextCompositionEventArgs e) => TextControl.NumberOnly(sender, e);

		private void FloatOnly(object sender, TextCompositionEventArgs e) => TextControl.FloatOnly(sender, e);

		private void HexCheck(object sender, TextCompositionEventArgs e) {
			if (sender is Wpf.Ui.Controls.TextBox textBox) {
				if (textBox.Text.Length >= 6 && textBox.SelectionLength == 0) {
					e.Handled = true;
					return;
				}
				char c = e.Text[0];
				if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')) {
					e.Handled = false;
				} else
					e.Handled = true;
			}
		}

		private void OnPasted(object sender, DataObjectPastingEventArgs e) {
			if (sender is TextBox textBox) {
				if (e.DataObject.GetDataPresent(DataFormats.UnicodeText)) {
					string pasted = (string)e.DataObject.GetData(DataFormats.UnicodeText);
					if (pasted.StartsWith('#'))
						pasted = pasted[1..];
					if (pasted.Length > 8) {
						e.CancelCommand();
						FlyoutWrongHex.Show();
					}
					pasted = pasted.ToUpper();
					for (int i = 0; i < pasted.Length; i++) {
						char c = pasted[i];
						if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F')) {
							continue;
						} else {
							e.CancelCommand();
							FlyoutWrongHex.Show();
							return;
						}
					}
					e.CancelCommand();
					if (pasted.Length > 6) {
						pasted = pasted[^6..];
					} else
						pasted = pasted.PadLeft(6, '0');
					textBox.Text = pasted;
					textBox.SelectionStart = 0;
					textBox.SelectionLength = pasted.Length;
				}
			}
		}

		private void ButtonChooseIcon(object sender, RoutedEventArgs e) => Binding.ChooseIcon();

		private void ButtonChooseTextureClick(object sender, RoutedEventArgs e) {
			Binding.ChooseTex((string)((Control)sender).Tag);
		}

		private void PreviewFileBoxDragOver(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				AccessoryDataUtil.CheckDragFileType(e);
			} else if (e.Data.GetDataPresent(DataFormats.Text)) {
				e.Effects = DragDropEffects.Copy;
			} else
				e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void PreviewFileButtonDragOver(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				AccessoryDataUtil.CheckDragFileType(e);
			} else
				e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void FileDrop(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (files.Length > 0) {
					string Tag = (string)((Control)sender).Tag;
					Binding.FileDrop(Tag, files[0]);
				}
			}
		}

		private void ButtonChooseColor(object sender, RoutedEventArgs e) {
			if (sender is Button button) {
				PopupColorPicker.PlacementTarget = button;
				PopupColorPicker.IsOpen = true;
				string property = (string) button.Tag;
				Binding colorBinding = new() {
					Source = Binding.PaintJobData,
					Path = new PropertyPath(property.ToString()),
					Mode = BindingMode.TwoWay
				};
				VehicleColorPicker.SetBinding(PickerControlBase.SelectedColorProperty, colorBinding);
			}
		}

		private void ColorVariantClick(object sender, RoutedEventArgs e) {
			if (PopupVariantList.IsOpen)
				return;
			PopupVariantList.IsOpen = true;
		}

		private void VariantColorSelected(object sender, SelectionChangedEventArgs e) {
			if (sender is ListBox) {
				if (PopupVariantList.IsOpen)
					PopupVariantList.IsOpen = false;
			}
		}

		private void AccessoryChecked(object sender, RoutedEventArgs e) {
			if (sender is CheckBox checkBox) {
				var acc = (Accessory)checkBox.DataContext;
				Binding.AccessoryChecked(acc);
			}
		}

		private void AccessoryUnchecked(object sender, RoutedEventArgs e) {
			if (sender is CheckBox checkBox) {
				var acc = (Accessory)checkBox.DataContext;
				Binding.AccessoryUnchecked(acc);
			}
		}

		private void ButtonPaintJobPreviewClick(object sender, RoutedEventArgs e) => Binding.OpenPaintJobPreview();

		private void ButtonStartClick(object sender, RoutedEventArgs e) => Binding.CreatePaintJobSii();


		private void AccListCheckBoxRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) => e.Handled = true;//用来避免点击acc listbox在点击checkbox时的自动滑动

	}
}


