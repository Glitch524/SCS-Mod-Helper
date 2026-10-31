using SCS_Mod_Helper.Utils;
using System.Windows;
using Wpf.Ui.Controls;

namespace SCS_Mod_Helper.Class.Base;
public class BaseWindow: FluentWindow, IBaseVisual {
	public Window IWindow => this;

	public BaseWindow() {
		if (Instances.FollowSystem) {
			Loaded += (sender, e) => {
				SetThemeWatcher(true);
			};
		}
	}

	public void SetThemeWatcher(bool watch) {
		if (watch)
			Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
		else
			Wpf.Ui.Appearance.SystemThemeWatcher.UnWatch(this);
	}

	public string GetString(string key, params object[] args) {
		string res = FindResource(key).ToString()!;
		if (args.Length > 0) {
			res = string.Format(res, args);
		}
		return res;
	}
}