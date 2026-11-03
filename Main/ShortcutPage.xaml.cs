using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SCS_Mod_Helper.Main {
	/// <summary>
	/// ShortcutPage.xaml 的交互逻辑
	/// </summary>
	public partial class ShortcutPage: BasePage {
		public ShortcutPage() {
			InitializeComponent();
		}

		private void ButtonClick(object sender, RoutedEventArgs e) {
			if (sender is Button button) {
				string tag = (string) button.Tag;
				var split = tag.Split('|');
				string path;
				switch(split[0]) {
					case "Game":
						if (split[1] == "ETS2") {
							if (string.IsNullOrEmpty(Instances.ETS2GamePath)) {
								MessageBox.Show(GetString("MessageSetPathFirst"), GetString("MessageTitleNotice"));
								return;
							}
							path = Instances.ETS2GamePath;
						} else {
							if (string.IsNullOrEmpty(Instances.ATSGamePath)) {
								MessageBox.Show(GetString("MessageSetPathFirst"), GetString("MessageTitleNotice"));
								return;
							}
							path = Instances.ATSGamePath;
						}
						break;
					case "Document":
						path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
						if (split[1] == "ETS2")
							path = Path.Combine(path, "Euro Truck Simulator 2");
						else
							path = Path.Combine(path, "American Truck Simulator");
						break;
					case "Workshop":
						if (split[1] == "ETS2") {
							if (string.IsNullOrEmpty(Instances.ETS2GamePath)) {
								MessageBox.Show(GetString("MessageSetPathFirst"), GetString("MessageTitleNotice"));
								return;
							}
							path = Instances.ETS2GamePath.Replace(@"common\Euro Truck Simulator 2", "");
							path = Path.Combine(path, @"workshop\content\227300");
						} else {
							if (string.IsNullOrEmpty(Instances.ATSGamePath)) {
								MessageBox.Show(GetString("MessageSetPathFirst"), GetString("MessageTitleNotice"));
								return;
							}
							path = Instances.ATSGamePath.Replace(@"common\American Truck Simulator", "");
							path = Path.Combine(path, @"workshop\content\270880");
						}
						break;
					case "CommonMod":
						if (string.IsNullOrEmpty(Instances.CommonModPath)) {
							MessageBox.Show(GetString("MessageSetPathFirst"), GetString("MessageTitleNotice"));
							return;
						}
						path = Instances.CommonModPath;
						break;
					default:
						return;
				}
				if(Directory.Exists(path)) {
					Process.Start(new ProcessStartInfo {
						FileName = path,
						UseShellExecute = true
					});
				} else 
					MessageBox.Show(GetString("MessageFolderNotExist"), GetString("MessageTitleNotice"));
			}
		}
	}
}
