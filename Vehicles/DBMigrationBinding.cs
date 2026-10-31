using SCS_Mod_Helper.Class.Base;
using System.Collections.ObjectModel;

namespace SCS_Mod_Helper.Vehicles; 
public class DBMigrationBinding(
	Func<IBaseVisual> baseVisual,
	int oldVersion,
	int newVersion,
	bool veEdited,
	bool vaEdited): WindowBinding(baseVisual) {
	private readonly int mOldVersion = oldVersion;
	private string? OldVersionString = null;
	public string OldVersion {
		get {
			OldVersionString ??= GetVersion(mOldVersion);
			return OldVersionString;
		}
	}

	private readonly int mNewVersion = newVersion;
	private string? NewVersionString = null;
	public string NewVersion {
		get {
			NewVersionString ??= GetVersion(mNewVersion);
			return NewVersionString;
		}
	}

	private static string GetVersion(int version) {
		string v = version.ToString();
		string date = v[^6..];
		int subV = int.Parse(v.Substring(v.Length - 9, 3));
		v = v[..^9];
		return $"{v}.{subV}.{date}";
	}

	public bool VehicleETS2Edited { get; } = veEdited;
	public bool VehicleATSEdited { get; } = vaEdited;

	private int mSelectedTab = 0;
	public int SelectedTab {
		get => mSelectedTab;
		set {
			mSelectedTab = value;
			InvokeChange();
			InvokeChange(nameof(ETS2Selected));
		}
	}

	public bool ETS2Selected => SelectedTab == 0;

	public ObservableCollection<Vehicle> OldVehiclesETS2 { get; } = [];
	public ObservableCollection<Vehicle> OldVehiclesATS { get; } = [];
	public ObservableCollection<Vehicle> NewVehiclesETS2 { get; } = [];
	public ObservableCollection<Vehicle> NewVehiclesATS { get; } = [];

	public Dictionary<string, Vehicle> KeepETS2 = [];
	public int KeepETS2Count => KeepETS2.Count;
	public Dictionary<string, Vehicle> KeepATS = [];
	public int KeepATSCount => KeepATS.Count;
}
