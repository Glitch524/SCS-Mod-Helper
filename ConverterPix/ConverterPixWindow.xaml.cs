using SCS_Mod_Helper.Class.Base;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCS_Mod_Helper.ConverterPix {
	/// <summary>
	/// ConverterPixWindow.xaml 的交互逻辑
	/// </summary>
	public partial class ConverterPixWindow: BaseWindow {
		private readonly ConverterPixBinding Binding;

		public ConverterPixWindow() {
			InitializeComponent();

			Binding = new(() => this, ListBoxScrollToTop);
			GridMain.DataContext = Binding;
		}

		private void BreadcrumbBarItemClicked(Wpf.Ui.Controls.BreadcrumbBar _, Wpf.Ui.Controls.BreadcrumbBarItemClickedEventArgs args) {
			var index = args.Index;
			Binding.FolderBack(index);
		}

		public void ListBoxScrollToTop(PIXFile file) => ListFiles.ScrollIntoView(file);

		private void ListBoxKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				Binding.OpenSelectedDir();
			} else if (e.Key == Key.Back) {
				Binding.FolderBack();
				e.Handled = true;
			}
		}

		private void ButtonAddPackClick(object sender, RoutedEventArgs e) => Binding.AddPack();

		private void ButtonClosePackClick(object sender, RoutedEventArgs e) {
			if (sender is Button button) {
				var pack = (PixPackage) button.DataContext;
				Binding.ClosePack(pack);
			}
        }
	}
}
