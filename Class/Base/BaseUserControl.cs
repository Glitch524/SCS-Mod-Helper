using System.Windows;
using System.Windows.Controls;

namespace SCS_Mod_Helper.Class.Base; 
public class BaseUserControl: UserControl, IBaseVisual {
	public string GetString(string key, params object[] args) {
		string res = IWindow.FindResource(key).ToString()!;
		if (args.Length > 0) {
			res = string.Format(res, args);
		}
		return res;
	}

	public Window IWindow => Window.GetWindow(this);
}
