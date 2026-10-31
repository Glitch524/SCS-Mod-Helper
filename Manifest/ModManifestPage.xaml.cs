using Microsoft.Win32;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Main;
using SCS_Mod_Helper.Utils;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Wpf.Ui;
using Wpf.Ui.Extensions;

namespace SCS_Mod_Helper.Manifest;
/// <summary>
/// ModManifestPage.xaml 的交互逻辑
/// </summary>
public partial class ModManifestPage: BasePage {
	private readonly ManifestBinding binding;

	string IconName {
		get => binding.IconName; set => binding.IconName = value;
	}
	string DescriptionName {
		get => binding.DescriptionName; set => binding.DescriptionName = value;
	}

	DescLocale CurrentLocale {
		get => binding.CurrentLocale; set => binding.CurrentLocale = value;
	}
	string DescContent {
		get => binding.DescContent; set => binding.DescContent = value;
	}

	Dictionary<string, DescLocale> LocaleDict => binding.LocaleDict;

	public ModManifestPage() {
		InitializeComponent();

		binding = ManifestBinding.GetInstance(this);

		GridMain.DataContext = binding;

		binding.InsertColor = InsertColor;
	}

	private void OnLoaded(object sender, RoutedEventArgs e) {
		NaviMainWindow mainWindow = (NaviMainWindow)Window.GetWindow(this);
		var navigation = mainWindow.Navigation;
		GridMeasureSize.SetBinding(HeightProperty, new Binding(nameof(ActualHeight)) {//page在navigationview里会获得无限高度，所以任何可以滚动的控件都会无限拉长，因此将page高度绑定到navigationview的高度（并-1，nv的上面有一条border）即可限定高度
			Source = navigation,
			Mode = BindingMode.OneWay,
			Converter = new NavigationHeightConverter()
		});
	}

	/// <summary>
	/// 选择切换project文件夹
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ChooseProjectLocation(object sender, RoutedEventArgs e) {
		var folderDialog = new OpenFolderDialog {
			Multiselect = false
		};
		var history = binding.ProjectLocation;
		if (history.Length > 0) {
			do {
				history = new DirectoryInfo(history).Parent!.FullName;
				if (Directory.Exists(history)) {
					folderDialog.InitialDirectory = history;
					break;
				}
			} while (history.Length > 0);
		}
		if (folderDialog.ShowDialog() == true) {
			_ = CheckProjectLocation(folderDialog.FolderName);
		}
	}

	public async Task CheckProjectLocation(string folderName) {
		if (Directory.Exists(folderName)) {
			void ExecuteLoad() {
				binding.ProjectLocation = folderName;
				binding.LoadFiles();
			}
			if (File.Exists(Paths.ManifestFile(folderName))) {
				ExecuteLoad();
				return;
			}

			var result = MessageBox.Show(Window.GetWindow(this), GetString("MessageNoManifest"), GetString("MessageTitleNotice"), MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes) {
				ExecuteLoad();
			}
		}
	}

	private void ImageButtonClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonChooseIcon) {
			OpenFileDialog ofd = new() {
				Multiselect = false,
				Filter = Util.GetFilter("DialogFilterIcon"),
				DefaultExt = "jpg"
			};
			if (ofd.ShowDialog() != true)
				return;
			try {
				Bitmap bitmap = new(ofd.FileName);
				if (bitmap.Width == 276 && bitmap.Height == 162) {
					binding.ModIcon = Util.LoadImage(ofd.FileName);
					binding.NewIcon = true;
				} else {
					var result = MessageBox.Show(Window.GetWindow(this), GetString("MessageIconErrWrongSize"), GetString("MessageTitleNotice"), MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes)
						Process.Start("explorer.exe", "/select," + ofd.FileName);
				}
			} catch (Exception ex) {
				if (ex.Message.Equals("Parameter is not valid."))
					MessageBox.Show(Window.GetWindow(this), GetString("MessageIconErrNotValidImage"));
			}
		}
	}

	private void InsertColor(string? color) {
		if (color == null)
			return;
		var sStart = TextDescription.SelectionStart;
		var sLength = TextDescription.SelectionLength;

		DescContent = DescContent.Insert(sStart, color);
		sStart += color.Length;

		if (sLength > 0) {
			var sEnd = sStart + sLength;
			DescContent = DescContent.Insert(sEnd, "[normal]");
			sEnd += "[normal]".Length;
			TextDescription.SelectionStart = sEnd;
		} else
			TextDescription.SelectionStart = sStart;
		TextDescription.Focus();
	}

	private void DeleteLocaleClick(object sender, RoutedEventArgs e) {
		if (sender is Button button) {
			DescLocale locale = (DescLocale)button.DataContext;
			if (locale.LocaleValue.Equals(Locale.Localization.Locale.LocaleValueUni)) {
				MessageBox.Show(Window.GetWindow(this), GetString("MessageDeleteLocaleErrUni"));
				return;
			} else {
				var result = MessageBox.Show(Window.GetWindow(this), GetString("MessageDeleteLocaleDoubleCheck"), GetString("MessageTitleNotice"), MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes) {
					locale.DescContent = "";
				}
			}
		}
	}

	private void CopyFromUniversalClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonCopyFromUniversal) {
			if (CurrentLocale.LocaleValue.Equals(Locale.Localization.Locale.LocaleValueUni))
				return;
			if (DescContent.Length > 0) {
				var result = MessageBox.Show(Window.GetWindow(this), GetString("MessageCopyDescNotEmpty"), GetString("MessageTitleNotice"), MessageBoxButton.YesNo);
				if (result == MessageBoxResult.No)
					return;
			}
			DescContent = LocaleDict[Locale.Localization.Locale.LocaleValueUni].DescContent;
		}
	}

	private void ButtonResultClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonSave) {
			ManifestIO.SaveManifest(Window.GetWindow(this), binding);
			MessageBox.Show(Window.GetWindow(this), GetString("ResultSaved"));
		}
	}

	private void TextFilenameChanged(object sender, TextChangedEventArgs e) {
		if (sender == TextIconName) {
			if (IconName.EndsWith(".jpg"))
				return;
			var ss = TextIconName.SelectionStart;
			IconName += ".jpg";
			TextIconName.SelectionStart = ss;
		} else if (sender == TextDescriptionName) {
			if (DescriptionName.EndsWith(".txt"))
				return;
			var ss = TextDescriptionName.SelectionStart;
			DescriptionName += ".txt";
			TextDescriptionName.SelectionStart = ss;
		}
	}
}

public class NavigationHeightConverter: IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is double d) {
			return d - 1;
		}
		return 0;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		return 0;
	}
}