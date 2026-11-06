using CommunityToolkit.Mvvm.Input;
using SCS_Mod_Helper.Modding.Accessories;
using SCS_Mod_Helper.Modding.PaintJob.ColorMask;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Extensions;

namespace SCS_Mod_Helper.Modding.PaintJob; 
public class AccPaintJobData(Func<bool> isUVOpen): AccessoryData() {
	//https://dariowouters.gitlab.io/scs-updates/unit/accessory_paint_job_data/
	//name 从accessoryData继承的变量
	//icon
	//price
	//unlock
	//part_type

	private readonly Func<bool> IsUVOpen = isUVOpen;

	private static bool IsPreviewOpen() => Application.Current.Windows.OfType<PaintJobTexPreviewWindow>().Any();

	private VehicleGroups? mVehicleGroups = null;
	public VehicleGroups VehiclesGroups { 
		get {
			if (mVehicleGroups == null) {
				mVehicleGroups = new();
				VehiclesIO.LoadPaintJobVehicles(mVehicleGroups);
			}
			return mVehicleGroups;
		}
	}

	private Vehicle? mCurrentVehicle = null;
	public Vehicle? CurrentVehicle {
		get => mCurrentVehicle;
		set {
			mCurrentVehicle = value;
			InvokeChange();
			if (IsPreviewOpen() && IsUVOpen())
				value?.GetCabinUV();
			InvokeChange(nameof(CabinList));
			InvokeChange(nameof(AccessoryList));
		}
	}

	public List<Cabin>? CabinList {
		get {
			var cabins = CurrentVehicle?.Cabins;
			if (cabins != null)
				CurrentCabin = cabins.FirstOrDefault();
			return cabins;
		}
	}

	public Cabin? CurrentCabin {
		get {
			if (field == null) {
				if (CabinList == null)
					field = null;
				else
					field = CabinList.FirstOrDefault();
			}
			return field;
		}
		set {
			field = value;
			InvokeChange();
		}
	} = null;



	public List<Accessory>? AccessoryList => CurrentVehicle?.Accessories;


	public string PaintJobTexRealPath = "";
	private string mPaintJobTex = "";
	public string PaintJobTex {
		get => mPaintJobTex;
		set {
			mPaintJobTex = value;
			InvokeChange();
		}
	}

	public string BaseTexOverride {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = string.Empty;

	public Visibility BaseTexOverrideVisibility {
		get => BaseTexOverride.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
	}

	private Color mBaseColor = Colors.White;
	public Color BaseColor {//6位hex 基本颜色
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = Colors.White;

	public Brush BaseColorVisual => BaseColor.ToBrush();

	private bool mBaseColorLocked = true;
	public bool BaseColorLocked {//固定基本颜色
		get => mBaseColorLocked;
		set {
			mBaseColorLocked = value;
			InvokeChange();
			InvokeChange(nameof(BaseColorCustomizable));
		}
	}

	public bool BaseColorCustomizable {
		get => !mBaseColorLocked;
		set {
			mBaseColorLocked = !value;
			InvokeChange();
			InvokeChange(nameof(BaseColorLocked));
		}
	}

	public bool AlternateUVSet {//额外UV集？
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;

	public bool Stock { //应该不需要显示因为默认必须false
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;

	private readonly ObservableCollection<ColorVariant> mColorVariantList = [];
	public ObservableCollection<ColorVariant> ColorVariantList => mColorVariantList;

	public ICommand AddVariantCommand => new RelayCommand(AddColorVariant);
	public void AddColorVariant() {
		ColorVariantList.Add(new());
		InvokeChange(nameof(CurrentVariant));
	}

	public ICommand RemoveVariantCommand => new RelayCommand<ColorVariant>(RemoveColorVariant);
	public void RemoveColorVariant(ColorVariant? variant) {
		if (variant == null)
			return;
		ColorVariantList.Remove(variant);
		if (CurrentVariant == variant) {
			if (ColorVariantList.Count > 0)
				CurrentVariant = ColorVariantList[0];
			else
				CurrentVariant = null;
		}
	}

	private ColorVariant? mCurrentVariant = null;
	public ColorVariant? CurrentVariant {
		get {
			if (ColorVariantList.Count == 0)
				return null;
			mCurrentVariant ??= ColorVariantList[0];
			return mCurrentVariant;
		}
		set {
			mCurrentVariant = value;
			InvokeChange();
			InvokeChange(nameof(VariantColorBase));
			InvokeChange(nameof(VariantColorBaseBrush));
			InvokeChange(nameof(VariantColor1));
			InvokeChange(nameof(VariantColor1Brush));
			InvokeChange(nameof(VariantColor2));
			InvokeChange(nameof(VariantColor2Brush));
			InvokeChange(nameof(VariantColor3));
			InvokeChange(nameof(VariantColor3Brush));
		}
	}

	public Color VariantColorBase {
		get => CurrentVariant?.ColorBase ?? Colors.White;
		set {
			if (CurrentVariant == null)
				return;
			CurrentVariant.ColorBase = value;
			InvokeChange();
			InvokeChange(nameof(VariantColorBaseBrush));
		}
	}
	public Brush VariantColorBaseBrush => VariantColorBase.ToBrush();

	public Color VariantColor1 {
		get => CurrentVariant?.Color1 ?? (Airbrush ? Colors.Red : Colors.Blue);
		set {
			if (CurrentVariant == null)
				return;
			CurrentVariant.Color1 = value;
			InvokeChange();
			InvokeChange(nameof(VariantColor1Brush));
		}
	}
	public Brush VariantColor1Brush => VariantColor1.ToBrush();

	public Color VariantColor2 {
		get => CurrentVariant?.Color2 ?? Color.FromRgb(0, 255, 0);
		set {
			if (CurrentVariant == null)
				return;
			CurrentVariant.Color2 = value;
			InvokeChange();
			InvokeChange(nameof(VariantColor2Brush));
		}
	}
	public Brush VariantColor2Brush => VariantColor2.ToBrush();

	public Color VariantColor3 {
		get => CurrentVariant?.Color3 ?? Colors.Red;
		set {
			if (CurrentVariant == null)
				return;
			CurrentVariant.Color3 = value;
			InvokeChange();
			InvokeChange(nameof(VariantColor3Brush));
		}
	}
	public Brush VariantColor3Brush => VariantColor3.ToBrush();


	public int TabIndex {
		get => Airbrush ? 0 : 1;
		set {
			Airbrush = value == 0;

			InvokeChange();
			InvokeChange(nameof(VariantColor1Brush));
			InvokeChange(nameof(Airbrush));
			LoadMaskChannel();
		}
	}

	private bool mAirbrush = true;
	public bool Airbrush {
		get => mAirbrush;
		set {
			mAirbrush = value;
			InvokeChange();
			InvokeChange(nameof(TabIndex));
		}
	}

	public void LoadTexImage(string imagePath) {
		PaintJobTexImage = null;
		MaskChannelR = null;
		MaskChannelG = null;
		MaskChannelB = null;
		if (IsPreviewOpen()) {
			PaintJobTexImage = Util.LoadPfimImage(imagePath);
			LoadMaskChannel();
		}
	}

	public void LoadMaskChannel() {
		if (!Airbrush && PaintJobTexImage != null && MaskChannelR == null) {
			ChannelExtraction.ExtractRGBChannels(PaintJobTexImage, out WriteableBitmap rBmp, out WriteableBitmap gBmp, out WriteableBitmap bBmp);
			MaskChannelR = rBmp;
			MaskChannelG = gBmp;
			MaskChannelB = bBmp;
		}
	}


	//以下为ColorMask内容

	protected BitmapSource? mPaintJobTexImage = null;
	public BitmapSource? PaintJobTexImage {
		get => mPaintJobTexImage;
		set {
			mPaintJobTexImage?.Freeze();
			mPaintJobTexImage = value;
			if (value == null) {
				MaskChannelR = null;
				MaskChannelG = null;
				MaskChannelB = null;
			}
			InvokeChange();
		}
	}
	protected BitmapSource? mMaskChannelR = null;
	public BitmapSource? MaskChannelR {
		get => mMaskChannelR;
		set {
			mMaskChannelR?.Freeze();
			mMaskChannelR = value;
			InvokeChange();
		}
	}
	protected BitmapSource? mMaskChannelG = null;
	public BitmapSource? MaskChannelG {
		get => mMaskChannelG;
		set {
			mMaskChannelG?.Freeze();
			mMaskChannelG = value;
			InvokeChange();
		}
	}
	protected BitmapSource? mMaskChannelB = null;
	public BitmapSource? MaskChannelB {
		get => mMaskChannelB;
		set {
			mMaskChannelB?.Freeze();
			mMaskChannelB = value;
			InvokeChange();
		}
	}
	private static string ColorToHex(Color color) {
		string r = string.Format("{0:x2}", color.R).ToUpper();
		string g = string.Format("{0:x2}", color.G).ToUpper();
		string b = string.Format("{0:x2}", color.B).ToUpper();
		return r + g + b;
	}

	public static Color HexToColor(string hex) {
		hex = '#' + hex.PadRight(6, 'F');
		return (Color)ColorConverter.ConvertFromString(hex);
	}

	private Color mMaskRColor = Colors.Red;
	public Color MaskRColor {//6位hex 遮罩颜色（红）
		get => mMaskRColor;
		set {
			mMaskRColor = value;
			InvokeMaskR();
		}
	}
	public Brush MaskRColorVisual => MaskRColor.ToBrush();

	public string MaskRColorHex {
		get => ColorToHex(MaskRColor);
		set {
			MaskRColor = HexToColor(value);
			InvokeMaskR();
		}
	}

	public byte MaskRColorR {
		get => mMaskRColor.R;
		set {
			mMaskRColor.R = value;
			InvokeMaskR();
		}
	}
	public byte MaskRColorG {
		get => mMaskRColor.G;
		set {
			mMaskRColor.G = value;
			InvokeMaskR();
		}
	}
	public byte MaskRColorB {
		get => mMaskRColor.B;
		set {
			mMaskRColor.B = value;
			InvokeMaskR();
		}
	}

	private void InvokeMaskR() {
		InvokeChange(nameof(MaskRColor));
		InvokeChange(nameof(MaskRColorVisual));
		InvokeChange(nameof(MaskRColorHex));
		InvokeChange(nameof(MaskRColorR));
		InvokeChange(nameof(MaskRColorG));
		InvokeChange(nameof(MaskRColorB));
	}

	private bool mMaskRLocked = true;
	public bool MaskRLocked {//固定遮罩颜色（红）
		get => mMaskRLocked;
		set {
			mMaskRLocked = value;
			InvokeChange();
			InvokeChange(nameof(MaskRCustomizable));
		}
	}

	public bool MaskRCustomizable {
		get => !mMaskRLocked;
		set {
			mMaskRLocked = !value;
			InvokeChange();
			InvokeChange(nameof(MaskRLocked));
		}
	}



	private Color mMaskGColor = Color.FromRgb(0, 255, 0);
	public Color MaskGColor {//6位hex 遮罩颜色（绿）
		get => mMaskGColor;
		set {
			mMaskGColor = value;
			InvokeMaskG();
		}
	}

	public Brush MaskGColorVisual => MaskGColor.ToBrush();

	public string MaskGColorHex {
		get => ColorToHex(MaskGColor);
		set {
			MaskGColor = HexToColor(value);
			InvokeMaskG();
		}
	}

	public byte MaskGColorR {
		get => mMaskGColor.R;
		set {
			mMaskGColor.R = value;
			InvokeMaskG();
		}
	}
	public byte MaskGColorG {
		get => mMaskGColor.G;
		set {
			mMaskGColor.G = value;
			InvokeMaskG();
		}
	}
	public byte MaskGColorB {
		get => mMaskGColor.B;
		set {
			mMaskGColor.B = value;
			InvokeMaskG();
		}
	}

	private void InvokeMaskG() {
		InvokeChange(nameof(MaskGColor));
		InvokeChange(nameof(MaskGColorVisual));
		InvokeChange(nameof(MaskGColorHex));
		InvokeChange(nameof(MaskGColorR));
		InvokeChange(nameof(MaskGColorG));
		InvokeChange(nameof(MaskGColorB));
	}

	private bool mMaskGLocked = true;
	public bool MaskGLocked {//固定遮罩颜色（绿）
		get => mMaskGLocked;
		set {
			mMaskGLocked = value;
			InvokeChange();
			InvokeChange(nameof(MaskGCustomizable));
		}
	}

	public bool MaskGCustomizable {
		get => !mMaskGLocked;
		set {
			mMaskGLocked = !value;
			InvokeChange();
			InvokeChange(nameof(MaskGLocked));
		}
	}



	private Color mMaskBColor = Colors.Blue;
	public Color MaskBColor {//6位hex 遮罩颜色（蓝）
		get => mMaskBColor;
		set {
			mMaskBColor = value;
			InvokeMaskB();
		}
	}

	public Brush MaskBColorVisual => MaskBColor.ToBrush();
	public string MaskBColorHex {
		get => ColorToHex(MaskBColor);
		set {
			MaskBColor = HexToColor(value);
			InvokeMaskB();
		}
	}

	public byte MaskBColorR {
		get => mMaskBColor.R;
		set {
			mMaskBColor.R = value;
			InvokeMaskB();
		}
	}
	public byte MaskBColorG {
		get => mMaskBColor.G;
		set {
			mMaskBColor.G = value;
			InvokeMaskB();
		}
	}
	public byte MaskBColorB {
		get => mMaskBColor.B;
		set {
			mMaskBColor.B = value;
			InvokeMaskB();
		}
	}

	private void InvokeMaskB() {
		InvokeChange(nameof(MaskBColor));
		InvokeChange(nameof(MaskBColorVisual));
		InvokeChange(nameof(MaskBColorHex));
		InvokeChange(nameof(MaskBColorR));
		InvokeChange(nameof(MaskBColorG));
		InvokeChange(nameof(MaskBColorB));
	}


	private bool mMaskBLocked = true;
	public bool MaskBLocked {//固定遮罩颜色（蓝）
		get => mMaskBLocked;
		set {
			mMaskBLocked = value;
			InvokeChange();
			InvokeChange(nameof(MaskBCustomizable));
		}
	}

	public bool MaskBCustomizable {
		get => !mMaskBLocked;
		set {
			mMaskBLocked = !value;
			InvokeChange();
			InvokeChange(nameof(MaskBLocked));
		}
	}


	private bool mFlipFlake = false;
	public bool FlipFlake {
		get => mFlipFlake;
		set {
			mFlipFlake = value;
			InvokeChange();
		}
	}

	private bool mAlternateFlipFlakeUVSet = false;
	public bool AlternateFlipFlakeUVSet {
		get => mAlternateFlipFlakeUVSet;
		set {
			mAlternateFlipFlakeUVSet = value;
			InvokeChange();
		}
	}

	private Color mFlipColor = Colors.Red;
	public Color FlipColor {//变色龙漆颜色
		get => mFlipColor;
		set {
			mFlipColor = value;
			InvokeChange();
			InvokeChange(nameof(FlipColorVisual));
		}
	}
	public Brush FlipColorVisual => FlipColor.ToBrush();

	private bool mFlipColorLocked = true;
	public bool FlipColorLocked {//固定金属漆颜色
		get => mFlipColorLocked;
		set {
			mFlipColorLocked = value;
			InvokeChange(nameof(FlipColorLocked));
			InvokeChange(nameof(FlipColorCustomizable));
		}
	}

	public bool FlipColorCustomizable {
		get => !mFlipColorLocked;
		set {
			mFlipColorLocked = !value;
			InvokeChange(nameof(FlipColorLocked));
			InvokeChange(nameof(FlipColorCustomizable));
		}
	}

	private float? mFlipStrength = null;
	public float? FlipStrength {//金属漆效果强度
		get => mFlipStrength;
		set {
			mFlipStrength = value;
			InvokeChange();
		}
	}


	private Color mFlakeColor = Color.FromRgb(0, 255, 0);
	public Color FlakeColor {//鳞片漆颜色
		get => mFlakeColor;
		set {
			mFlakeColor = value;
			InvokeChange();
			InvokeChange(nameof(FlakeColorVisual));
		}
	}
	public Brush FlakeColorVisual => FlakeColor.ToBrush();

	private bool mFlakeColorLocked = true;
	public bool FlakeColorLocked {//固定鳞片漆颜色
		get => mFlakeColorLocked;
		set {
			mFlakeColorLocked = value;
			InvokeChange();
		}
	}

	public bool FlakeColorCustomizable {
		get => !mFlakeColorLocked;
		set {
			mFlakeColorLocked = !value;
			InvokeChange(nameof(FlakeColorLocked));
			InvokeChange(nameof(FlakeColorCustomizable));
		}
	}

	private float? mFlakeShininess = null;
	public float? FlakeShininess {//鳞片漆光泽度
		get => mFlakeShininess;
		set {
			mFlakeShininess = value;
			InvokeChange();
		}
	}

	private float? mFlakeDensity = null;
	public float? FlakeDensity {//鳞片密度
		get => mFlakeDensity;
		set {
			mFlakeDensity = value;
			InvokeChange();
		}
	}

	private float? mFlakeClearcoatRolloff = null;
	public float? FlakeClearcoatRolloff {//鳞片漆清漆层滚落效应		调整透明涂层镜面反射高光的锐度。值越高，边缘越清晰。
		get => mFlakeClearcoatRolloff;
		set {
			mFlakeClearcoatRolloff = value;
			InvokeChange();
		}
	}

	private float? mFlakeUVScale = null;
	public float? FlakeUVScale {//UV比率
		get => mFlakeUVScale;
		set {
			mFlakeUVScale = value;
			InvokeChange();
		}
	}

	private float? mFlakeVRatio = null;
	public float? FlakeVRatio {//UV比率
		get => mFlakeVRatio;
		set {
			mFlakeVRatio = value;
			InvokeChange();
		}
	}

	public const string DefaultFlakeNoise = "/material/custom/flake_noise.tobj";
	private string mFlakeNoise = string.Empty;
	public string FlakeNoise {//珠光噪声图
		get => mFlakeNoise;
		set {
			mFlakeNoise = value;
			InvokeChange();
		}
	}

	public Visibility FlakeNoiseVisibility => FlakeNoise == DefaultFlakeNoise ? Visibility.Visible : Visibility.Collapsed;


	//Override 部分


	private readonly ObservableCollection<PaintJobOverrideData> mOverrideList = [];

	public ObservableCollection<PaintJobOverrideData> OverrideList => mOverrideList;

	public void AddOvr() {
		OverrideList.Add(new(OverrideList.Count));
		SelectedOverride = OverrideList.Last();
	}

	public void RemoveOvr(PaintJobOverrideData? ovrData) {
		if (ovrData == null)
			return;
		int index = OverrideList.IndexOf(ovrData);
		ovrData.RemoveAccList();
		OverrideList.RemoveAt(index);
		for (int i = index; i < OverrideList.Count; i++) {
			OverrideList[i].Index = i;
		}
	}


	private PaintJobOverrideData? mSelectedOverride = null;
	public PaintJobOverrideData? SelectedOverride {
		get => mSelectedOverride;
		set {
			ChangeCheckState = true;
			ObservableCollection<Accessory> accList;
			if (mSelectedOverride != null) {
				accList = mSelectedOverride.AccList;
				foreach (var acc in accList) {
					acc.Check = false;
				}
			}
			mSelectedOverride = value;
			if (value != null) {
				accList = value.AccList;
				foreach (var acc in accList) {
					acc.Check = true;
				}
			}
			ChangeCheckState = false;
			InvokeChange();
			InvokeChange(nameof(IsCheckAllChecked));
		}
	}

	bool ChangeCheckState = false;
	public void AccessoryChecked(Accessory accessory) {
		if (ChangeCheckState)
			return;
		if (SelectedOverride == null)
			return;
		accessory.BelongingOverride?.AccList.Remove(accessory);
		SelectedOverride.AccList.Add(accessory);
		accessory.BelongingOverride = SelectedOverride;
	}

	public void AccessoryUnchecked(Accessory accessory) {
		if (ChangeCheckState || SelectedOverride == null)
			return;
		SelectedOverride.AccList.Remove(accessory);
		accessory.BelongingOverride = null;
	}

	public bool IsCheckAllChecked {
		get {
			if (AccessoryList != null && SelectedOverride != null) {
				if (AccessoryList.Count == SelectedOverride.AccList.Count && AccessoryList.Count > 0)
					return true;
			}
			return false;
		}
		set {
			CheckAllAcc(value);
			InvokeChange();
		}
	}

	public void CheckAllAcc(bool check) {
		if (AccessoryList == null || AccessoryList.Count == 0)
			return;
		foreach(var acc in AccessoryList) {
			acc.Check = check;
			if (check)
				AccessoryChecked(acc);
			else
				AccessoryUnchecked(acc);
		}
	}


	public string AccTex {
		get => SelectedOverride?.AccTex ?? "";
		set {
			if (SelectedOverride != null) {
				SelectedOverride.AccTex = value!;
				InvokeChange();
				InvokeChange(nameof(AccTexVisibility));
			}
		}
	}
	public Visibility AccTexVisibility => AccTex.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

	protected BitmapSource? mAccTexImage = null;
	public BitmapSource? AccTexImage {
		get => SelectedOverride?.AccTexImage;
		set {
			SelectedOverride?.AccTexImage = value;
			InvokeChange();
		}
	}
	public float? AccFlakeUVScale {
		get => SelectedOverride?.AccFlakeUVScale;
		set {
			if (SelectedOverride != null && value != null) {
				SelectedOverride.AccFlakeUVScale = (float)value;
				InvokeChange();
			}
		}
	}

	public float? AccFlakeVRatio {
		get => SelectedOverride?.AccFlakeVRatio;
		set {
			if (SelectedOverride != null && value != null) {
				SelectedOverride.AccFlakeVRatio = (float)value;
				InvokeChange();
			}
		}
	}

	private bool mSyncOverNetwork = false;
	public bool SyncOverNetwork {
		get => mSyncOverNetwork;
		set {
			mSyncOverNetwork = value;
			InvokeChange();
		}
	}
}