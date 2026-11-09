using SCS_Mod_Helper.Language;
using System.Windows;
using System.Windows.Controls;

namespace SCS_Mod_Helper.Class.Base; 
public class BaseUserControl: UserControl, IBaseVisual {
	public Window IWindow => Window.GetWindow(this);

	public string GetString(string key, params object[] args) => Resource.GetString(IWindow, key, args);
	public string GetQuantityString(string key, int quantity, params object[] args) => Resource.GetQuantityString(IWindow, key, quantity, args);
	public string GetFilter(string key) => Resource.GetFilter(IWindow, key);
}
