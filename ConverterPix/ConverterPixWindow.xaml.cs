using Microsoft.Win32;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
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

		public string PackPath {
			get => Binding.PackPath;
			set => Binding.PackPath = value;
		}

		private void ButtonChoosePack(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog = new() {
				Filter = Util.GetFilter("DialogFilterMod"),
				Multiselect = false
			};
			if (openFileDialog.ShowDialog() == true) {
				PackPath = openFileDialog.FileName;
				Binding.DirStart(openFileDialog.SafeFileName);
			}
		}

		private void ButtonChooseDestClick(object sender, RoutedEventArgs e) {
			OpenFolderDialog openFolderDialog = new() {
				Multiselect = false,
				InitialDirectory = Binding.DestPath
			};
			if (openFolderDialog.ShowDialog() == true) {
				Binding.DestPath = openFolderDialog.FolderName;
			}
		}

		private void ButtonClick(object sender, RoutedEventArgs e) {
			if (sender == ButtonBack) {
				Binding.Undo();
			} else if (sender == ButtonReturn) {
				Binding.FolderBack();
			}
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
