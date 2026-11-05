using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon.CreatedSii;
internal class CreatedModelBinding(Func<IBaseVisual> visualFunc): WindowBinding(visualFunc) {
	public static string CurrentProjectLocation => Instances.ProjectLocation;

	private Visibility mProgressRingVisibility = Visibility.Visible;
	public Visibility ProgressRingVisibility {
		get => mProgressRingVisibility;
		set {
			mProgressRingVisibility = value;
			InvokeChange();
		}
	}
	public bool ProgressRingVisible {
		get => mProgressRingVisibility == Visibility.Visible;
		set {
			mProgressRingVisibility = value ? Visibility.Visible : Visibility.Collapsed;
			InvokeChange(nameof(ProgressRingVisibility));
		}
	}

	public int SelectCount {
		get => CurrentModel!.SelectCount;
		set {
			CurrentModel!.SelectCount = value;
			InvokeChange();
		}
	}

	private ObservableCollection<CreatedModel> mCreatedModelList = [];
	public ObservableCollection<CreatedModel> CreatedModelList {
		get => mCreatedModelList;
		set {
			mCreatedModelList = value;
			InvokeChange();
		}
	}

	private CreatedModel? mCurrentModel = null;
	public CreatedModel? CurrentModel {
		get => mCurrentModel ?? CreatedModelList.FirstOrDefault();
		set {
			mCurrentModel = value ?? CreatedModelList.FirstOrDefault();
			InvokeChange();

			InvokeChange(nameof(CurrentModelItems));
		}
	}

	public ObservableCollection<CreatedModelItem>? CurrentModelItems => CurrentModel?.CreatedModelItems;

	private CreatedModelItem? currentModelItem = null;
	public CreatedModelItem? CurrentModelItem {
		get => currentModelItem;
		set {
			currentModelItem = value;
			InvokeChange();

			value?.ReadModelDetail();
		}
	}

	public void DeleteSelected() {
		Task.Run(() => {
			ProgressRingVisible = true;

			List<CreatedModel> deletingModel = [];
			foreach (var model in CreatedModelList) {
				List<CreatedModelItem> deletingItem = [];
				foreach (var item in model.CreatedModelItems) {
					if (item.Check) {
						//File.Delete(item.path);
						deletingItem.Add(item);
					}
				}
				if (deletingItem.Count > 0) {
					Application.Current.Dispatcher.Invoke(() => {
						foreach (var item in deletingItem) {
							model.CreatedModelItems.Remove(item);
						}
					}, DispatcherPriority.Render);
					model.UpdateCount();
				}
				if (model.CreatedModelItems.Count == 0)
					deletingModel.Add(model);
			}
			Application.Current.Dispatcher.Invoke(() => {
				foreach (var model in deletingModel) {
					CreatedModelList.Remove(model);
				}
			}, DispatcherPriority.Render);
			ProgressRingVisible = false;
		});
	}
}
