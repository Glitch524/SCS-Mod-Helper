using SCS_Mod_Helper.Language;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SCS_Mod_Helper.Class.Base;
public class BaseBinding: INotifyPropertyChanged {
	public event PropertyChangedEventHandler? PropertyChanged;
	public void InvokeChange([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new(name));


	public static string GetString(string key, params object[] args) => Resource.GetString(key, args);
	public static string GetQuantityString(string key, int quantity, params object[] args) => Resource.GetQuantityString(key, quantity, args);
	public static string GetFilter(string key) => Resource.GetFilter(key);
}
public class WindowBinding(Func<IBaseVisual> baseVisualFactory): BaseBinding {
	private readonly Func<IBaseVisual> BaseVisualFactory = baseVisualFactory;
	protected Window BindingWindow => BaseVisualFactory().IWindow;

	public new string GetString(string key, params object[] args) => BindingWindow.GetString(key, args);
	public new string GetQuantityString(string key, int quantity, params object[] args) => BindingWindow.GetQuantityString(key, quantity, args);
	public new string GetFilter(string key) => BindingWindow.GetFilter(key);
}