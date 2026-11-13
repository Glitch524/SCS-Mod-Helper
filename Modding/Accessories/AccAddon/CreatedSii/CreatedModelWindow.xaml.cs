using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon.CreatedSii;

/// <summary>
/// CleanSii.xaml 的交互逻辑
/// </summary>
public partial class CreatedModelWindow: BaseWindow {
	private readonly CreatedModelBinding Binding;

	public ObservableCollection<CreatedModel> CreatedModelList {
		get => Binding.CreatedModelList; set => Binding.CreatedModelList = value;
	}
	public CreatedModel? CurrentModel {
		get => Binding.CurrentModel; set => Binding.CurrentModel = value;
	}

	public ObservableCollection<CreatedModelItem>? CurrentModelItems => Binding.CurrentModelItems;

	public CreatedModelWindow() {
		InitializeComponent();

		Binding = new(() => this);
		GridMain.DataContext = Binding;

		Loaded += OnLoaded;
		Closed += OnClosed;
	}

	private readonly CancellationTokenSource source = new();

	private void OnLoaded(object sender, RoutedEventArgs e) => Task.Run(LoadCreatedSii);

	private void OnClosed(object? sender, EventArgs e) => source.Cancel();

	private readonly DirectoryInfo defDir = new(Paths.DefTruckDir());
	readonly Dictionary<string, CreatedModel> ModelPair = [];

	private void LoadCreatedSii() {
		ModelPair.Clear();
		void NoSii() {
			Application.Current.Dispatcher.Invoke(() => {
				MessageBox.Show(this, GetString("MessageLoadSiiErrNoSii"));
				Close();
			});
		}
		if (!defDir.Exists) {
			NoSii();
			return;
		}
		LoadCreatedSii(defDir);
		if (source.IsCancellationRequested)
			return;
		if (CreatedModelList.Count == 0) {
			NoSii();
			return;
		}
		CurrentModel = CreatedModelList.First();
		Binding.ProgressRingVisible = false;
	}

	private void LoadCreatedSii(DirectoryInfo info) {
		foreach (var dir in info.GetDirectories()) {
			if (source.IsCancellationRequested)
				return;
			LoadCreatedSii(dir);
		}
		foreach (var file in info.GetFiles()) {
			if (source.IsCancellationRequested)
				return;
			if (file.Extension.Equals(".sii")) {
				var filePath = file.FullName;

				var pathShort = filePath.Replace(Paths.DefTruckDir(), "");

				var modelName = "";
				var vehicleID = "";
				var modelType = "";
				var deVehicle = filePath.Replace(defDir.FullName + '\\', "");
				var split = deVehicle.Split('\\');
				if (split.Length >= 4) {
					vehicleID = split[0];
					modelType = split[2];
					modelName = split[3][..^4];
				}
				CreatedModelItem c = new(filePath, pathShort, vehicleID, modelType, modelName);

				if (ModelPair.TryGetValue(modelName, out CreatedModel? createdModel)) {
					Dispatcher.Invoke(new(() => {
						createdModel.CreatedModelItems.Add(c);
					}), System.Windows.Threading.DispatcherPriority.Render);
				} else {
					CreatedModel cm = new(modelName);
					cm.CreatedModelItems.Add(c);
					Dispatcher.Invoke(new(() => {
						CreatedModelList.Add(cm);
					}), System.Windows.Threading.DispatcherPriority.Render);
					ModelPair.Add(modelName, cm);
				}
			}
		}
	}

	private void ButtonClick(object sender, RoutedEventArgs e) {
		if (sender == ButtonDelete) {
			var result = MessageBox.Show(this, GetString("MessageDeleteDoubleCheck"), GetString("ButtonDelete"), MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes) {
				Binding.DeleteSelected();
			}
		} else if (sender == ButtonOpen) {
			CreatedModelItem s = (CreatedModelItem)TableCreatedSii.SelectedItem;
			if (s == null) {
				MessageBox.Show(this, GetString("MessageOpenErrNoSelected"));
				return;
			}
			if (!File.Exists(s.Path)) {
				MessageBox.Show(this, GetString("MessageOpenErrNotExist"));
				return;
			}
			Process.Start("notepad.exe", s.Path);
		} else if (sender == ButtonClose) {
			DialogResult = true;
		}
	}

	private void CheckBoxClicked(object sender, RoutedEventArgs e) {//不使用Checked和Unchecked事件，因为切换CreatedModel时会触发Checked，导致SelectCount错误
		if (sender is CheckBox checkBox) {
			if (checkBox.IsChecked == true)
				Binding.SelectCount++;
			else
				Binding.SelectCount--;
		}
	}
}

