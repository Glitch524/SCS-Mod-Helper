using ColorPicker;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;
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
			int type;
			if (sender == ButtonPaintJob) {
				type = PaintJobBinding.TEX_PAINT_JOB;
			} else if (sender == ButtonBaseTexOverride) {
				type = PaintJobBinding.TEX_BASE_TEX_OVR;
			} else if (sender == ButtonChooseFlakeNoise) {
				type = PaintJobBinding.TEX_FLAKE_NOISE;
			} else if (sender == ButtonChooseAccTex) {
				type = PaintJobBinding.TEX_ACC_TEX;
			} else { return; }
			Binding.ChooseTex(type);
		}

		private void FileDrop(object sender, DragEventArgs e) => Binding.FileDrop();


		private void ButtonChooseColor(object sender, RoutedEventArgs e) {
			PopupColorPicker.PlacementTarget = (UIElement)sender;
			PopupColorPicker.IsOpen = true;
			string property;
			if (sender == ButtonBaseColor) {
				property = nameof(Binding.PaintJobData.BaseColor);
			} else if (sender == ButtonVariantBase) {
				property = nameof(Binding.PaintJobData.VariantColorBase);
			} else if (sender == ButtonVariant1) {
				property = nameof(Binding.PaintJobData.VariantColor1);
			} else if (sender == ButtonVariant2) {
				property = nameof(Binding.PaintJobData.VariantColor2);
			} else if (sender == ButtonVariant3) {
				property = nameof(Binding.PaintJobData.VariantColor3);
			} else if (sender == ButtonFlipColor) {
				property = nameof(Binding.PaintJobData.FlipColor);
			} else if (sender == ButtonFlakeColor) {
				property = nameof(Binding.PaintJobData.FlakeColor);
			} else if (sender == ButtonMaskR) {
				property = nameof(Binding.PaintJobData.MaskRColor);
			} else if (sender == ButtonMaskG) {
				property = nameof(Binding.PaintJobData.MaskGColor);
			} else if (sender == ButtonMaskB) {
				property = nameof(Binding.PaintJobData.MaskBColor);
			} else
				throw new ArgumentException("");
			Binding colorBinding = new() {
				Source = Binding.PaintJobData,
				Path = new PropertyPath(property.ToString()),
				Mode = BindingMode.TwoWay
			};
			VehicleColorPicker.SetBinding(PickerControlBase.SelectedColorProperty, colorBinding);
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


