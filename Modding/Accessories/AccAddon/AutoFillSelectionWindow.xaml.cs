using SCS_Mod_Helper.Class.Base;
using System.Windows;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon;

/// <summary>
/// AutoFillSelectionWindow.xaml 的交互逻辑
/// </summary>
public partial class AutoFillSelectionWindow: BaseWindow {
	private readonly AutoFillBinding binding;
	public bool CheckModelName {
		get => binding.CheckModelName; 
		set => binding.CheckModelName = value;
	}
	public bool CheckModelType {
		get => binding.CheckModelType; 
		set => binding.CheckModelType = value;
	}
	public bool CheckModelPath {
		get => binding.CheckModelPath; 
		set => binding.CheckModelPath = value;
	}
	public bool CheckModelPathUK {
		get => binding.CheckModelPathUK; 
		set => binding.CheckModelPathUK = value;
	}
	public bool CheckCollPath {
		get => binding.CheckCollPath; 
		set => binding.CheckCollPath = value;
	}
	public AutoFillSelectionWindow(
		int type,
		string? modelName,
		string? modelType,
		string? modelPath,
		string? modelPathUK,
		string? collPath) {
		InitializeComponent();
		binding = new(() => this, type, modelName, modelType, modelPath, modelPathUK, collPath);
		StackMain.DataContext = binding;
	}

	private void ResultClick(object sender, RoutedEventArgs e) {
		DialogResult = true;
		Close();
	}
}

public class AutoFillBinding: WindowBinding {

	public AutoFillBinding(
		Func<IBaseVisual> visualFunc,
		int chooseType,
		string? modelName,
		string? modelType,
		string? modelPath,
		string? modelPathUK,
		string? collPath): base(visualFunc) {
		mchooseType = chooseType;
		mCheckModelName = !string.IsNullOrEmpty(modelName);
		mTextModelName = modelName;
		mCheckModelType = !string.IsNullOrEmpty(modelType);
		mTextModelType = modelType;
		mCheckModelPath = !string.IsNullOrEmpty(modelPath);
		mTextModelPath = modelPath;
		mCheckModelPathUK = !string.IsNullOrEmpty(modelPathUK);
		mTextModelPathUK = modelPathUK;
		mCheckCollPath = !string.IsNullOrEmpty(collPath);
		mTextCollPath = collPath;

		mVisibleCount = 0;
		if (VisibleModelName)
			mVisibleCount++;
		if (VisibleModelType)
			mVisibleCount++;
		if (VisibleModelPath)
			mVisibleCount++;
		if (VisibleModelPathUK)
			mVisibleCount++;
		if (VisibleCollPath)
			mVisibleCount++;
		CheckCount = mVisibleCount;
	}

	private int mchooseType;
	public int ChooseType {
		get => mchooseType;
		set {
			mchooseType = value;
			InvokeChange(nameof(VisiblilityModelName));
		}
	}

	public bool SelectAll {
		get => (VisibleCount == CheckCount);
		set {
			InvokeChange();
			if (VisibleModelName) 
				CheckModelName = value;
			if (VisibleModelType) 
				CheckModelType = value;
			if (VisibleModelPath) 
				CheckModelPath = value;
			if (VisibleModelPathUK) 
				CheckModelPathUK = value;
			if (VisibleCollPath) 
				CheckCollPath = value;
		}
	}

	private void CheckSelectAll(bool value) { 
		if (value)
			CheckCount++;
		else
			CheckCount--;
		InvokeChange(nameof(SelectAll));
	}

	private int mVisibleCount;
	public int VisibleCount {
		get => mVisibleCount;
		set {
			mVisibleCount = value;
			InvokeChange();
		}
	}

	private int CheckCount;

	private bool mCheckModelName;
	public bool CheckModelName {
		get => mCheckModelName;
		set {
			mCheckModelName = value;
			InvokeChange();
			CheckSelectAll(value);
		}
	}

	private string? mTextModelName;
	public string? TextModelName {
		get => mTextModelName;
		set {
			mTextModelName = value;
			InvokeChange();

			InvokeChange(nameof(VisiblilityModelName));
		}
	}

	public bool VisibleModelName => !string.IsNullOrEmpty(TextModelName);
	public Visibility VisiblilityModelName => VisibleModelName ? Visibility.Visible : Visibility.Collapsed;



	private bool mCheckModelType;
	public bool CheckModelType {
		get => mCheckModelType;
		set {
			mCheckModelType = value;
			InvokeChange();
			CheckSelectAll(value);
		}
	}

	private string? mTextModelType;
	public string? TextModelType {
		get => mTextModelType;
		set {
			mTextModelType = value;
			InvokeChange();

			InvokeChange(nameof(VisiblilityModelType));
		}
	}
	public bool VisibleModelType => !string.IsNullOrEmpty(TextModelType);
	public Visibility VisiblilityModelType => VisibleModelType ? Visibility.Visible : Visibility.Collapsed;


	private bool mCheckModelPath;
	public bool CheckModelPath {
		get => mCheckModelPath;
		set {
			mCheckModelPath = value;
			InvokeChange();
			CheckSelectAll(value);
		}
	}

	private string? mTextModelPath;
	public string? TextModelPath {
		get => mTextModelPath;
		set {
			mTextModelPath = value;
			InvokeChange();

			InvokeChange(nameof(VisiblilityModelPath));
		}
	}

	public bool VisibleModelPath => ChooseType == AccAddonBinding.MODEL || ChooseType == AccAddonBinding.EXT_MODEL || !string.IsNullOrEmpty(TextModelPath);
	public Visibility VisiblilityModelPath => VisibleModelPath ? Visibility.Visible : Visibility.Collapsed;


	private bool mCheckModelPathUK;
	public bool CheckModelPathUK {
		get => mCheckModelPathUK;
		set {
			mCheckModelPathUK = value;
			InvokeChange();
			CheckSelectAll(value);
		}
	}

	private string? mTextModelPathUK;
	public string? TextModelPathUK {
		get => mTextModelPathUK;
		set {
			mTextModelPathUK = value;
			InvokeChange();

			InvokeChange(nameof(VisiblilityModelPathUK));
		}
	}
	public bool VisibleModelPathUK => ChooseType == AccAddonBinding.MODEL_UK || ChooseType == AccAddonBinding.EXT_MODEL_UK || !string.IsNullOrEmpty(TextModelPathUK);
	public Visibility VisiblilityModelPathUK => VisibleModelPathUK ? Visibility.Visible : Visibility.Collapsed;


	private bool mCheckCollPath;
	public bool CheckCollPath {
		get => mCheckCollPath;
		set {
			mCheckCollPath = value;
			InvokeChange();
			CheckSelectAll(value);
		}
	}

	private string? mTextCollPath;


	public string? TextCollPath {
		get => mTextCollPath;
		set {
			mTextCollPath = value;
			InvokeChange();

			InvokeChange(nameof(VisiblilityCollPath));
		}
	}

	public bool VisibleCollPath => ChooseType == AccAddonBinding.MODEL_COLL || !string.IsNullOrEmpty(TextCollPath);
	public Visibility VisiblilityCollPath => VisibleCollPath ? Visibility.Visible : Visibility.Collapsed;
}