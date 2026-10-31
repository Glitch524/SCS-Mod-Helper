using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon; 
public class AccessoryType: BaseBinding {
	private static ObservableCollection<AccessoryType>? mModelTypes = null;
	public static ObservableCollection<AccessoryType> ModelTypes {
		get {
			mModelTypes ??= GetModelTypes();
			ResourceUtil.ChangeLanguage += AccessoryChangeLanguage;
			return mModelTypes;
		}
	}

	public const string TypeCupHolder = "cup_holder";
	public const string TypeCurtainF = "curtain_f";
	public const string TypeDrvPlate = "drv_plate";
	public const string TypeCoDrvPlate = "codrv_plate";
	public const string TypeFlagL = "flag_l";
	public const string TypeFlagR = "flag_r";
	public const string TypeFlagFL = "flag_f_l";
	public const string TypeFlagFR = "flag_f_r";
	public const string TypeInlightBck = "inlight_bck";
	public const string TypeBckPanel = "bckpanel";
	public const string TypeLPillow = "l_pillow";
	public const string TypeSetCabinet = "set_cabinet";
	public const string TypeSetGlass = "set_glass";
	public const string TypeSetLGlass = "set_lglass";
	public const string TypeSterringW = "sterring_w";
	public const string TypeToyHang = "toyhang";
	public const string TypeToyAc = "toyac";
	public const string TypeToyStand = "toystand";
	public const string TypeToyDash = "toydash";
	public const string TypeToySeat = "toyseat";
	public const string TypeToyBed = "toybed";
	public const string TypeToyBig = "toybig";
	public const string TypeToyBack = "toyback";
	public const string TypeToySofa = "toysofa";
	public const string TypeToyTable = "toytable";
	public static ObservableCollection<AccessoryType> GetModelTypes() {
		return[
			new(TypeCupHolder),
			new(TypeCurtainF, true),
			new(TypeDrvPlate),
			new(TypeCoDrvPlate),
			new(TypeFlagL, TypeFlagFL, "Acc.flag_l"),
			new(TypeFlagR, TypeFlagFR, "Acc.flag_r"),
			new(TypeInlightBck, TypeBckPanel, "Acc.back_wall"),
			new(TypeLPillow),
			new(TypeSetCabinet, false),
			new(TypeSetGlass),
			new(TypeSetLGlass),
			new(TypeSterringW),
			new(TypeToyHang),
			new(TypeToyAc),
			new(TypeToyStand),
			new(TypeToyDash, false),
			new(TypeToySeat),
			new(TypeToyBed),
			new(TypeToyBig),
			new(TypeToyBack, false),
			new(TypeToySofa, false),
			new(TypeToyTable, false),
		];
	}

	public static void AccessoryChangeLanguage() {
		foreach(var acc in ModelTypes) {
			acc.RefreshName();
		}
	}

	public AccessoryType() : this("", "", "") { }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="accETS2">欧卡的饰品</param>
	/// <param name="accATS">美卡的饰品</param>
	/// <param name="nameKey">额外的key名称，用于在资源字典获取本地名</param>
	public AccessoryType(
		string accETS2,
		string accATS,
		string nameKey) {
		mAccessoryETS2 = accETS2;
		mAccessoryATS = accATS;
		NameKey = nameKey;
		if (nameKey == "")
			mName = "";
		else
			mName = GetString(nameKey);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="acc">饰品的型号</param>
	/// <param name="gameType">True代表这是ets的饰品，False是ATS的饰品，null代表属于两边共有的</param>
	public AccessoryType(
		string acc,
		bool? gameType = null) {// null: both, true: ETS2, false: ATS
		mAccessoryETS2 = acc;
		mGameType = gameType;
		if (gameType == null) {
			mAccessoryETS2 = acc;
			mAccessoryATS = acc;
		} else if (gameType == true) {
			mAccessoryETS2 = acc;
		} else {
			mAccessoryATS = acc;
		}
		NameKey = $"Acc.{acc}";
		mName = GetString($"Acc.{acc}");
	}

	private string? mAccessoryETS2;
	public string? AccessoryETS2 {
		get => mAccessoryETS2;
		set {
			mAccessoryETS2 = value;
			InvokeChange();
		}
	}

	private string? mAccessoryATS;
	public string? AccessoryATS {
		get => mAccessoryATS;
		set {
			mAccessoryATS = value;
			InvokeChange();
		}
	}
	public string Accessory {
		get {
			if (mGameType == null) {
				if (mAccessoryETS2 == mAccessoryATS)
					return mAccessoryETS2!;
				else
					return $"{mAccessoryETS2}/{mAccessoryATS}";
			} else if (mGameType == true)
				return mAccessoryETS2!;
			else
				return mAccessoryATS!;
		}
	}

	private string mName;
	public string Name {
		get => mName;
		set {
			mName = value;
			InvokeChange();
		}
	}

	private readonly string NameKey;
	public void RefreshName() {
		if (Accessory.Length == 0)
			return;
		Name = GetString(NameKey);
	}

	private readonly bool? mGameType;
	public bool? GameType => mGameType;

	public bool ForETS2 => mGameType == null || mGameType == true;
	public bool ForATS => mGameType == null || mGameType == false;
}