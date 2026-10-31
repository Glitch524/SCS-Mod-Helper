using SCS_Mod_Helper.Utils;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SCS_Mod_Helper.Class.Base;
public class BaseBinding: INotifyPropertyChanged {
	public event PropertyChangedEventHandler? PropertyChanged;
	public void InvokeChange([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new(name));

	public static string GetString(string key, params object[] param) => Util.GetString(key, param);
	public static string GetFilter(string key) => Util.GetFilter(key);
}
public class WindowBinding(Func<IBaseVisual> baseVisualFactory): BaseBinding {
	private readonly Func<IBaseVisual> BaseVisualFactory = baseVisualFactory;
	protected Window BindingWindow => BaseVisualFactory().IWindow;
	public new string GetString(string key, params object[] param) => BindingWindow.GetString(key, param);
	public new string GetFilter(string key) => BindingWindow.GetFilter(key);
}