using SCS_Mod_Helper.Class.Base;
using System.Collections.ObjectModel;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon.CreatedSii;
public class CreatedModel(string modelName): BaseBinding {
	private string modelName = modelName;
	public string ModelName {
		get => modelName;
		set {
			modelName = value;
			InvokeChange();
		}
	}

	private ObservableCollection<CreatedModelItem> createdModelItems = [];
	public ObservableCollection<CreatedModelItem> CreatedModelItems {
		get => createdModelItems;
		set {
			createdModelItems = value;
			InvokeChange();
		}
	}

	private int selectCount = 0;
	public int SelectCount {
		get => selectCount;
		set {
			selectCount = value;
			InvokeChange(nameof(SelectAll));
		}
	}

	public void UpdateCount() {
		selectCount = 0;
	}

	public bool? SelectAll {
		get {
			if (SelectCount == CreatedModelItems.Count)
				return true;
			else if (SelectCount == 0)
				return false;
			return null;
		}
		set {
			foreach (var item in CreatedModelItems) {
				item.Check = value == true;
			}
			SelectCount = value == true ? createdModelItems.Count : 0;
		}
	}
}

public class CreatedModelItem: BaseBinding {

	public CreatedModelItem(string path) {
		Path = path;
		VehicleType = GetString("Unknown");
		VehicleID = GetString("UnknownFile");
		AccType = "";
		ModelName = GetString("Unknown");
	}

	public CreatedModelItem(string path, string vehicleType, string vehicleID, string accType, string modelName) {
		Path = path;
		VehicleType = vehicleType;
		VehicleID = vehicleID;
		AccType = accType;
		ModelName = modelName;
	}

	public bool Check {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;

	public string Path {
		get;
		set {
			field = value;
			InvokeChange();
		}
	}

	public string VehicleType {
		get;
		set {
			field = value;
			InvokeChange();
		}
	}

	public string VehicleID {
		get;
		set {
			field = value;
			InvokeChange();
		}
	}

	public string AccType {
		get;
		set {
			field = value;
			InvokeChange();
		}
	}

	public string ModelName {
		get;
		set {
			field = value;
			InvokeChange();
		}
	}

	public string DisplayName {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = string.Empty;

	public string Look {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = string.Empty;

	public string Variant {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = string.Empty;

	public void ReadModelDetail() {
		if (DisplayName != string.Empty)
			return;
		Task.Run(() => {
			AccAddonSCSIO.ReadAccAddon(this);
			if (Look == string.Empty)
				Look = "default";
			if (Variant == string.Empty)
				Variant = "default";
		});
	}
}