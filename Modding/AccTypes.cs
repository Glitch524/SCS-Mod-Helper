using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;

namespace SCS_Mod_Helper.Modding;

public class AccTypes: BaseBinding {
	public List<AccTypeItem> AccGroupList { get; }

	public AccTypes() {
		AccGroupList = [new AccExteriorGroup(0, AddToAllAccs, ExpandCheck), new AccInteriorList(1, AddToAllAccs, ExpandCheck)];
	}

	public List<AccType> AllAccs { get; } = [];

	private void AddToAllAccs(List<AccType> childList) {
		AllAccs.AddRange(childList);
	}

	public List<AccType>? SelectedList { 
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = null;

	public void UpdateSelectedList(string path) {
		if (path.Length == 0)
			return;
		SelectedList = AccGroupList[path[0] - '0'].GetAccTypeList(path[1..]);
	}

	int OpeningIndex = -1;
	private void ExpandCheck(int index, bool visible) {
		if (visible) {
			if (OpeningIndex == -1) {
				OpeningIndex = index;
			} else {
				AccGroupList[OpeningIndex].Expanded = false;
				OpeningIndex = index;
			}
		} else {
			if (OpeningIndex == index)//因为新的展开会让旧的变量收起，expanded被设为false时也会触发一次expandcheck，避免直接把openingindex的赋值取消
				OpeningIndex = -1;
		}
	}
}

public abstract class AccTypeItem: BaseBinding {
	public bool Expanded { 
		get;
		set {
			field = value;
			InvokeExpanded(value);
			InvokeChange();
		}
	} = false;

	protected abstract void InvokeExpanded(bool expanded);

	public bool Focusable { get; } = false;
	private string? mGroupName = null;
	public string GroupName {
		get {
			mGroupName ??= GetGroupName();
			return mGroupName;
		}
	}

	protected abstract string GetGroupName();

	public abstract List<AccType> GetAccTypeList(string path);
}

public class AccExteriorGroup: AccTypeItem {
	protected override string GetGroupName() => VehiclesIO.GetAccGroupName("Exterior");
	private readonly int Index;
	private readonly Action<int, bool> ExpandCheckAction;

	protected override void InvokeExpanded(bool expanded) => ExpandCheckAction(Index, expanded);

	public List<AccExteriorList> Groups { get; }

	int OpeningIndex = -1;

	public AccExteriorGroup(int index,
						 Action<List<AccType>> addToParent,
						 Action<int, bool> expandCheckAction) : this(
							 "",
							 index,
							 addToParent,
							 expandCheckAction) {
	}
	public AccExteriorGroup(
		string parent,
		int index,
		Action<List<AccType>> addToParent,
		Action<int, bool> expandCheckAction) {
		Index = index;
		var nParent = parent + index;
		ExpandCheckAction = expandCheckAction;
		Groups = [
			new AccExteriorList(ExteriorGroupType.Cabin, nParent, 0, addToParent, ExpandCheck),
			new AccExteriorList(ExteriorGroupType.DoorNStep, nParent, 1, addToParent, ExpandCheck),
			new AccExteriorList(ExteriorGroupType.Decors, nParent, 2, addToParent, ExpandCheck),
			new AccExteriorList(ExteriorGroupType.Mirrors, nParent, 3, addToParent, ExpandCheck),
			new AccExteriorList(ExteriorGroupType.Front, nParent, 4, addToParent, ExpandCheck),
			new AccExteriorList(ExteriorGroupType.Lights, nParent, 5, addToParent, ExpandCheck),
			new AccExteriorList(ExteriorGroupType.ChassisProtection, nParent, 6, addToParent, ExpandCheck),
			new AccExteriorList(ExteriorGroupType.Chassis, nParent, 7, addToParent, ExpandCheck),
			new AccExteriorList(ExteriorGroupType.Others, nParent, 8, addToParent, ExpandCheck)];
	}

	private void ExpandCheck(int index, bool visible) {
		if (visible) {
			if (OpeningIndex == -1) {
				OpeningIndex = index;
			} else {
				Groups[OpeningIndex].Expanded = false;
				OpeningIndex = index;
			}
		} else {
			if (OpeningIndex == index) //因为新的展开会让旧的变量收起，expanded被设为false时也会触发一次expandcheck，避免直接把openingindex的赋值取消
				OpeningIndex = -1;
		}
	}

	public override List<AccType> GetAccTypeList(string path) => Groups[path[0] - '0'].GetAccTypeList(path[1..]);
}

public abstract class AccessoryList(string parent, int index): AccTypeItem {
	protected int Index = index;
	private readonly string Parent = parent + index;

	protected AccType NewType(string accETS2, string accATS, string? nameKey = null) => new(Parent, accETS2, accATS, nameKey);

	protected AccType NewType(string acc, bool? gameType = null) => new(Parent, acc, gameType);
}

public enum ExteriorGroupType {
	Cabin, DoorNStep, Decors, Mirrors, Front, Lights, ChassisProtection, Chassis, Others
}

public class AccExteriorList: AccessoryList {
	private readonly Action<List<AccType>> AddToParent;
	private readonly Action<int, bool> ExpandCheckAction;
	private readonly ExteriorGroupType GroupType;

	protected override string GetGroupName() {
		string accGroupName = GroupType switch {
			ExteriorGroupType.Cabin => "cabin",
			ExteriorGroupType.DoorNStep => "door_step",
			ExteriorGroupType.Decors => "decors",
			ExteriorGroupType.Mirrors => "mirror",
			ExteriorGroupType.Front => "front",
			ExteriorGroupType.Lights => "lights",
			ExteriorGroupType.ChassisProtection => "chs_protection",
			ExteriorGroupType.Chassis => "chassis",
			ExteriorGroupType.Others => "others",
			_ => throw new ArgumentException("")
		};
		return VehiclesIO.GetAccGroupName(accGroupName);
	}

	private readonly List<AccType> mAccList;
	public List<AccType> AccList => mAccList;

	public void AccessoryChangeLanguage() {
		foreach (var acc in AccList) {
			acc.RefreshName();
		}
	}
	protected override void InvokeExpanded(bool expanded) => ExpandCheckAction(Index, expanded);

	public override List<AccType> GetAccTypeList(string path) => AccList;

	public AccExteriorList(ExteriorGroupType groupType,
						string parent,
						int index,
						Action<List<AccType>> addToParent,
						Action<int, bool> expandCheckAction) : base(parent, index) {
		AddToParent = addToParent;
		ExpandCheckAction = expandCheckAction;
		GroupType = groupType;

		ResourceUtil.ChangeLanguage += AccessoryChangeLanguage;
		mAccList = GroupType switch {
			ExteriorGroupType.Cabin => [
				NewType("airdam"), NewType("roof_addon"), NewType("roof_window"), NewType("sunshield"), NewType("sunshld"), NewType("f_window"),
				NewType("frntglss_mid"), NewType("f_wnd_frame"), NewType("windows"), NewType("hood"), NewType("deflector"), NewType("cabdefl"),
				NewType("cor_def"), NewType("r_deflector"), NewType("s_deflector"), NewType("headache_r"), NewType("sideflares"), NewType("cabflares"),
				NewType("r_panel"), NewType("s_panel"), NewType("rear_cab"), NewType("rear2_cab"), NewType("rear_window"), NewType("r_chs_cover"),
				NewType("s_vent"), NewType("slp_vent")],
			ExteriorGroupType.DoorNStep => [
				NewType("cab_door"), NewType("doorhndl"), NewType("slp_hndl"), NewType("cab_doorstep"), NewType("doorstep"), NewType("rearstep"),
				NewType("hndrail")],
			ExteriorGroupType.Decors => [
				NewType("f_cab_trim"), NewType("windowtrim"), NewType("doortrim"), NewType("f_badge"), NewType("f_logo"), NewType("f_banner"),
				NewType("ornament"), NewType("r_badge"), NewType("r_logo"), NewType("s_badge"), NewType("s_logo"), NewType("decals"), NewType("s_reflector"),
				NewType("s_strip")],
			ExteriorGroupType.Mirrors => [
				NewType("mirror"), NewType("rv_mirror"), NewType("s_mirror"), NewType("f_mirror"), NewType("fl_mirror"), NewType("fr_mirror")],
			ExteriorGroupType.Front => [
				NewType("grill"), NewType("c_grill"), NewType("b_grill"), NewType("f_inlay_cab"), NewType("f_inlay_chs"), NewType("f_intake_bar"),
				NewType("f_intake_cab"), NewType("f_intake_chs"), NewType("f_intk_b_cab"), NewType("f_intk_b_chs"), NewType("s_guard"), NewType("hl_guard"),
				NewType("bullbar"), NewType("f_grill"), NewType("f_equip"), NewType("bumper"), NewType("f_bumper"), NewType("r_bumper")],
			ExteriorGroupType.Lights => [
				NewType("f_light"), NewType("f_light_chs"), NewType("f_light_mid"), NewType("f_light_top"), NewType("r_grill"), NewType("f_turn_light"),
				NewType("r_light"), NewType("beacon"), NewType("chs_beacon"), NewType("f_light_bmp")],
			ExteriorGroupType.ChassisProtection => [
				NewType("fenders"), NewType("f_fender"), NewType("r_fender"), NewType("f_fender_cab"), NewType("f_fender_chs"), NewType("f_fendr_chs"),
				NewType("r_fender_top"), NewType("r_fendr_top"), NewType("mudguard"), NewType("lghfender"), NewType("mudflaps"), NewType("b_mudflap"),
				NewType("f_mudflap"), NewType("r_mudflap"), NewType("sideskirt"), NewType("sideskirt_bar"), NewType("sideskrt_bar")],
			ExteriorGroupType.Chassis => [
				NewType("tank"), NewType("fuelcap"), NewType("exhaust"), NewType("cab_exhaust"), NewType("exhaust_l"), NewType("exhaust_r"),
				NewType("exhaust_m"), NewType("spare_w"), NewType("trunk"), NewType("bed_addon"), NewType("wing")],
			ExteriorGroupType.Others => [
				NewType("b_detector"), NewType("filter"), NewType("filters"), NewType("filter_cover"), NewType("horns"), NewType("l_horn"), NewType("r_horn"),
				NewType("drv_plate"), NewType("codrv_plate"),
				NewType("flag_l", "flag_f_l", "flag_l_both"),
				NewType("flag_r", "flag_f_r", "flag_r_both"),],
			_ => throw new ArgumentException(""),
		};
		AddToParent(mAccList);
	}
}

public class AccInteriorList: AccessoryList {

	private readonly Action<List<AccType>> AddToParent;
	private readonly Action<int, bool> ExpandCheckAction;
	protected override string GetGroupName() => VehiclesIO.GetAccGroupName("Interior");

	public const string TypeCupHolder = "cup_holder";
	public const string TypeCurtainF = "curtain_f";
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

	protected override void InvokeExpanded(bool expanded) => ExpandCheckAction(Index, expanded);

	public AccInteriorList(int index,
						Action<List<AccType>> addToParent,
						Action<int, bool> expandCheckAction) : this("", index, addToParent, expandCheckAction) {
	}

	public AccInteriorList(string parent, int index,
						Action<List<AccType>> addToParent,
						Action<int, bool> expandCheckAction) : base(parent, index) {
		AddToParent = addToParent;
		ExpandCheckAction = expandCheckAction;

		ResourceUtil.ChangeLanguage += AccessoryChangeLanguage;
		mAccList = [
			NewType(TypeCupHolder),
			NewType(TypeCurtainF, true),
			NewType(TypeInlightBck, TypeBckPanel),
			NewType(TypeLPillow),
			NewType(TypeSetCabinet, false),
			NewType(TypeSetGlass),
			NewType(TypeSetLGlass),
			NewType(TypeSterringW),
			NewType(TypeToyHang),
			NewType(TypeToyAc),
			NewType(TypeToyStand),
			NewType(TypeToyDash, false),
			NewType(TypeToySeat),
			NewType(TypeToyBed),
			NewType(TypeToyBig),
			NewType(TypeToyBack, false),
			NewType(TypeToySofa, false),
			NewType(TypeToyTable, false),
		];
		AddToParent(mAccList);
	}

	private readonly List<AccType> mAccList;
	public List<AccType> AccList => mAccList;

	public void AccessoryChangeLanguage() {
		foreach (var acc in AccList) {
			acc.RefreshName();
		}
	}

	public override List<AccType> GetAccTypeList(string path) => AccList;
}

public class AccType: BaseBinding {
	public bool Expanded { get; } = false;
	public bool Focusable { get; } = true;

	public bool IsSelected {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;

	public string Parent;

	public bool IsInterior => Parent[0] == '1';

	public AccType() {
		Parent = "";
		mAccETS2 = "";
		mAccATS = "";
		NameKey = "";
		mName = "";
		mNameFull = "";
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="accETS2">欧卡的饰品</param>
	/// <param name="accATS">美卡的饰品</param>
	/// <param name="nameKey">额外的key名称，用于在资源字典获取本地名</param>
	public AccType(
		string parent,
		string accETS2,
		string accATS,
		string? nameKey = null) {
		Parent = parent;
		mAccETS2 = accETS2;
		mAccATS = accATS;
		NameKey = nameKey ?? "";
		if (string.IsNullOrEmpty(nameKey))
			mName = $"{VehiclesIO.GetAccPartName(accETS2)}/{VehiclesIO.GetAccPartName(accATS)}";
		else
			mName = VehiclesIO.GetAccPartName(nameKey);
		mNameFull = $"{accETS2}/{accATS} → {mName}";
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="acc">饰品的型号</param>
	/// <param name="gameType">True代表这是ets的饰品，False是ATS的饰品，null代表属于两边共有的</param>
	public AccType(
		string parent,
		string acc,
		bool? gameType = null) {// null: both, true: ETS2, false: ATS
		Parent = parent;
		mAccETS2 = acc;
		mGameType = gameType;
		if (gameType == null) {
			mAccETS2 = acc;
			mAccATS = acc;
		} else if (gameType == true) {
			mAccETS2 = acc;
		} else {
			mAccATS = acc;
		}
		NameKey = acc;
		mName = VehiclesIO.GetAccPartName(acc);
		mNameFull = $"{acc} → {mName}";
	}

	private string? mAccETS2;
	public string? AccETS2 {
		get => mAccETS2;
		set {
			mAccETS2 = value;
			InvokeChange();
		}
	}

	private string? mAccATS;
	public string? AccATS {
		get => mAccATS;
		set {
			mAccATS = value;
			InvokeChange();
		}
	}
	public string Accessory {
		get {
			if (mGameType == null) {
				if (mAccETS2 == mAccATS)
					return mAccETS2!;
				else
					return $"{mAccETS2}/{mAccATS}";
			} else if (mGameType == true)
				return mAccETS2!;
			else
				return mAccATS!;
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

	private string mNameFull;
	public string NameFull {
		get => mNameFull;
		set {
			mNameFull = value;
			InvokeChange();
		}
	}

	private readonly string NameKey;
	public void RefreshName() {
		if (Accessory.Length == 0)
			return;
		Name = VehiclesIO.GetAccPartName(NameKey);
	}

	private readonly bool? mGameType;
	public bool? GameType => mGameType;

	public bool ForETS2 => mGameType == null || mGameType == true;
	public bool ForATS => mGameType == null || mGameType == false;

	public override string ToString() => $"AccType={Name}";
}