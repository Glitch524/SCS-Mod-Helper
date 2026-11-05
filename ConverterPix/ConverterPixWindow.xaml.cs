using SCS_Mod_Helper.Class.Base;
using System.Windows;
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

		public string PackPath {
			get => Binding.PackPath;
			set => Binding.PackPath = value;
		}

		private void BreadcrumbBarItemClicked(Wpf.Ui.Controls.BreadcrumbBar _, Wpf.Ui.Controls.BreadcrumbBarItemClickedEventArgs args) {
			var index = args.Index;
			Binding.DirBack(index);
		}

		public void ListBoxScrollToTop(PIXFile file) => ListFiles.ScrollIntoView(file);

		private void ButtonExtractFileClick(object sender, RoutedEventArgs e) {
			if (Binding.SelectedFile is PIXFile pixFile) {
				Binding.ExtractFile(pixFile);
			}
		}

		private void ButtonShowFileClick(object sender, RoutedEventArgs e) {
			if (Binding.SelectedFile is PIXFile pixFile) {
				Binding.ReadFile(pixFile);
			}
		}

		private void ListBoxKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				Binding.OpenSelectedDir();
			} else if (e.Key == Key.Back) {
				Binding.FolderBack();
				e.Handled = true;
			}
		}

		private void ButtonEDF(object sender, RoutedEventArgs e) {
			var extractTask = new ExtractAllTask(PackPath, Binding.DestPath) {
				Owner = this
			};
			extractTask.ShowDialog();
		}
	}
}
