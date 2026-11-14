using Microsoft.Win32;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Windows;
using System.Windows.Controls;

namespace SCS_Mod_Helper.Main; 
/// <summary>
/// ExportsPage.xaml 的交互逻辑
/// </summary>
public partial class ExportsPage: BasePage {
	public ExportsPage() {
		InitializeComponent();
	}

	private void ButtonClick(object sender, RoutedEventArgs e) {
		if (sender is Button button) {
			string tag = (string)button.Tag;
			var split = tag.Split('|');
			switch (split[0]) {
				case "LockZip":
					LockZipPackage();
					break;
			}
		}
	}

	private void LockZipPackage() {
		var ofd = new OpenFileDialog() {
			Title = GetString("TitleLockZip"),
			Multiselect = false,
			AddExtension = true,
			DefaultDirectory = Instances.CommonModPath,
			DefaultExt = "zip",
			Filter = GetFilter("DialogFilterZip"),
		};
		if (ofd.ShowDialog() == true) {
			var zip = ofd.FileName;
			Task.Run(async () => {
				var (result, reason) = await Util.ZipLock(zip);
				Application.Current.Dispatcher.Invoke(() => {
					if (result)
						MessageBox.Show(GetString("DialogResultZipLocked"));
					else
						MessageBox.Show(GetString("DialogResultZipLockFail") + reason);
				});
			});
		}
	}
}
