using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Locale.Localization;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SCS_Mod_Helper.Locale.Localization;

/// <summary>
/// ModLocalization.xaml 的交互逻辑
/// </summary>
public partial class ModLocalizationWindow: BaseWindow {
	private readonly LocaleBinding Binding;

	public static string ProjectLocation => Instances.ProjectLocation;

	private ObservableCollection<LocaleModule> Modules => Binding.LocaleModules;
	private ObservableCollection<LocaleModule> DeletedModules => Binding.DeletedModules;

	private LocaleModule? CurrentModule {
		get => Binding.CurrentModule; set => Binding.CurrentModule = value;
	}

	public ObservableCollection<LocalePair> CurrentDict => Binding.CurrentDict!;
	public ObservableCollection<LocalePair> UniversalDict => Binding.UniversalDict!;

	public bool HasChanges = false;
	public ModLocalizationWindow() {
		InitializeComponent();

		Binding = new(() => this);
		GridMain.DataContext = Binding;

		Loaded += OnLoaded;
	}

	private async void OnLoaded(object sender, RoutedEventArgs e) {
		var loadedModules = await Task.Run(() => LocaleIO.ReadLocaleDict());
		Modules.Clear();
		foreach (var m in loadedModules) {
			Modules.Add(m);
		}
		Binding.CurrentModule = null;//见LocaleBinding内的CurrentModule 设为空时会获取LocaleModules列表的第一个module
	}

	private void ButtonModuleClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonAddModule) {
			var newModule = new LocaleModule("new_translation");
			Modules.Add(newModule);
			CurrentModule = newModule;
		}
	}

	private void ButtonDeleteModuleClick(object sender, RoutedEventArgs e) {
		var result = MessageBox.Show(this, GetString("MessageDeleteLocale"), GetString("MessageTitleNotice"), MessageBoxButton.YesNo);
		if (result == MessageBoxResult.Yes) {
			DeletedModules.Add(CurrentModule!);
			Modules.Remove(CurrentModule!);
			Binding.CurrentModule = null;
		}
	}

	private void ButtonCleanClick(object sender, RoutedEventArgs e) {
		Binding.CleanSameDict();
	}

	private void ButtonSyncingClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonSyncWithUni) {
			Binding.SyncWithUniversal();
		} else if (sender == ButtonSyncOrder) {
			Binding.SyncOrder();
		}
	}

	private void DeleteLocaleClick(object sender, RoutedEventArgs e) {
		if (sender is Button button) {
			ModLocale locale = (ModLocale)button.DataContext;
			locale.Dictionary.Clear();
		}
	}

	private void OperateButtonClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonAdd) {
			CollectionUtil.AddItem(TableDict, CurrentDict, new("", ""));
		} else if (sender == ButtonDelete) {
			CollectionUtil.RemoveItem(TableDict, CurrentDict);
		} else if (sender == ButtonUp || sender == ButtonDown) {
			CollectionUtil.MoveDataGridItems(sender == ButtonUp, TableDict, CurrentDict);
		}
	}

	private void ButtonCloseClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonCancel) {
			Close();
		} else if (sender == ButtonSave) {
			if (UniversalDict.Count > 0)
				RunSave();
			else {
				var result = MessageBox.Show(this, GetString("MessageUniEmpty"), GetString("MessageTitleNotice"), MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					RunSave();
			}
		}
	}
	private void RunSave() {
		LocaleIO.SaveLocaleDict(this, Modules, DeletedModules);
		HasChanges = true;
		MessageBox.Show(this, GetString("MessageSaveSuccess"));
	}
}