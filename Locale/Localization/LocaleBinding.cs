using CommunityToolkit.Mvvm.Input;
using SCS_Mod_Helper.Class.Base;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SCS_Mod_Helper.Locale.Localization;

public class LocaleBinding(Func<IBaseVisual> visualFunc): WindowBinding(visualFunc) {
	public readonly ObservableCollection<LocaleModule> mLocaleModules = [];
	public ObservableCollection<LocaleModule> LocaleModules => mLocaleModules;
	public readonly ObservableCollection<LocaleModule> DeletedModules = [];

	private LocaleModule? mCurrentModule = null;
	public LocaleModule? CurrentModule {
		get {
			mCurrentModule ??= LocaleModules.FirstOrDefault();
			return mCurrentModule;
		}
		set {
			mCurrentModule = value ?? LocaleModules.FirstOrDefault();
			InvokeChange();

			InvokeChange(nameof(CurrentLocaleList));
		}
	}

	public string CurrentModuleName {
		get => CurrentModule?.ModuleName ?? "";
		set {
			if (CurrentModule != null) {
				CurrentModule.ModuleName = value;
				InvokeChange();
			}
		}
	}

	public ObservableCollection<ModLocale>? CurrentLocaleList => CurrentModule?.LocaleList;

	private ModLocale? mCurrentLocale = null;
	public ModLocale? CurrentLocale {
		get {
			mCurrentLocale ??= CurrentLocaleList?.FirstOrDefault();
			return mCurrentLocale;
		}
		set {
			mCurrentLocale = value ?? CurrentLocaleList?.FirstOrDefault();
			InvokeChange();

			InvokeChange(nameof(CurrentDict));
		}
	}

	public ObservableCollection<LocalePair>? CurrentDict => CurrentLocale?.Dictionary;
	public ObservableCollection<LocalePair> UniversalDict => CurrentModule!.UniversalDict;

	public ICommand SyncCommand => new RelayCommand(SyncWithUniversal);
	public void SyncWithUniversal() {
		var currentList = new List<LocalePair>(CurrentDict!);
		var universalList = new List<LocalePair>(UniversalDict!);

		var currentKey = new HashSet<string>(currentList.Where(p => p.Key.Length > 0 || p.Value.Length > 0).Select(p => p.Key));
		var universalKey = new HashSet<string>(universalList.Where(p => p.Key.Length > 0 || p.Value.Length > 0).Select(p => p.Key));

		var onlyInCurrent = currentKey.Except(universalKey).ToHashSet();
		var onlyInUniversal = universalKey.Except(currentKey).ToHashSet();

		//最后剩下的就是对方没有的pair，交换插入即可
		static void AddAll(ObservableCollection<LocalePair> dict, List<LocalePair> list, HashSet<string> onlySet) {
			foreach (var pair in list) {
				if (onlySet.Contains(pair.Key)) {
					dict.Add(new(pair.Key, pair.Value));
				}
			}
		}
		AddAll(CurrentDict!, universalList, onlyInUniversal);
		AddAll(UniversalDict, currentList, onlyInCurrent);
		var title = GetString("MessageTitleSyncResult");
		StringBuilder message = new();
		if (onlyInCurrent.Count == 0 && onlyInUniversal.Count == 0) {
			message.Append(GetString("MessageSyncResultZero"));
		} else {
			if (onlyInCurrent.Count > 0)
				message.Append(GetString("MessageSyncResultTo", onlyInCurrent.Count));
			if (onlyInUniversal.Count > 0) {
				if (message.Length > 0)
					message.Append('\n');
				message.Append(GetString("MessageSyncResultFrom", onlyInUniversal.Count));
			}
		}
		MessageBox.Show(BindingWindow, message.ToString(), title);
	}

	public ICommand SyncOrderCommand => new RelayCommand(SyncOrder);
	public void SyncOrder() {
		if (CurrentModule == null)
			return;
		List<string> currentKeys = [];
		foreach (var pair in CurrentDict!) {
			currentKeys.Add(pair.Key);
		}
		foreach (var locale in CurrentLocaleList!) {
			if (locale == CurrentLocale)
				continue;
			var dict = locale.Dictionary;
			for (int c = 0; c < currentKeys.Count; c++) {
				for (int d = c; d < dict.Count; d++) {
					var foundPair = dict[d];
					if (currentKeys[c].Equals(foundPair.Key)) {
						if (c != d) {
							dict.RemoveAt(d);
							dict.Insert(c, foundPair);
						}
						break;
					}
				}
			}
		}
	}

	public ICommand CleanupCommand => new RelayCommand(CleanSameDict);
	public void CleanSameDict() {
		if (CurrentModule == null)
			return;
		var universalDict = UniversalDict;
		if (universalDict.Count == 0)
			return;
		foreach (var locale in CurrentLocaleList!) {
			var dict = locale.Dictionary;
			if (locale.LocaleValue == Locale.LocaleValueUni || dict.Count == 0 || dict.Count != universalDict.Count)
				continue;
			var same = true;
			for (int i = 0; i < universalDict.Count; i++) {
				if (universalDict[i].Key != dict[i].Key || universalDict[i].Value != dict[i].Value) {
					same = false;
					break;
				}
			}
			if (same)
				dict.Clear();
		}
	}
}
