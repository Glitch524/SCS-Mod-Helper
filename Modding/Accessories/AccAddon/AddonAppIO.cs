using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Xml;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon;

public class AddonAppIO(string savePath): AppIO {
	protected const string TITLE_ACCESSORY_ADDON = "AccessoryAddon";
	public const string ATTR_GAME_VERSION = "GameVersion";
	protected const string TITLE_MODEL_NAME = "ModelName";
	protected const string TITLE_DISPLAY_NAME = "DisplayName";
	protected const string TITLE_PRICE = "Price";
	protected const string TITLE_UNLOCK_LEVEL = "UnlockLevel";
	protected const string TITLE_ICON_NAME = "IconName";
	protected const string TITLE_PART_TYPE = "PartType";
	protected const string TITLE_MODEL_PATH = "ModelPath";
	protected const string TITLE_MODEL_PATH_UK = "ModelPathUK";
	protected const string TITLE_EXT_MODEL_PATH = "ExternalModelPath";
	protected const string TITLE_EXT_MODEL_PATH_UK = "ExternalModelPathUK";
	protected const string TITLE_COLLISION_PATH = "CollPath";
	protected const string TITLE_MODEL_TYPE = "ModelType";
	protected const string TITLE_LOOK = "Look";
	protected const string TITLE_VARIANT = "variant";

	protected const string TITLE_HIDE_IN = "HideIn";
	protected const string TITLE_ELECTRIC_TYPE = "ElectricType";

	protected const string TITLE_DATA_LIST = "DataList";
	protected const string TITLE_DATA = "Data";
	protected const string TITLE_SUITABLE_FOR_LIST = "SuitableForList";
	protected const string TITLE_SUITABLE_FOR = "SuitableFor";
	protected const string TITLE_CONFLICT_WITH_LIST = "ConflictWithList";
	protected const string TITLE_CONFLICT_WITH = "ConflictWith";
	protected const string TITLE_DEFAULTS_LIST = "DefaultsList";
	protected const string TITLE_DEFAULTS = "Defaults";
	protected const string TITLE_OVERRIDES_LIST = "OverridesList";
	protected const string TITLE_OVERRIDES = "Overrides";
	protected const string TITLE_REQUIRE_LIST = "RequireList";
	protected const string TITLE_REQUIRE = "Require";

	protected const string TITLE_VEHICLES_ETS2 = "VehiclesETS2";
	protected const string TITLE_VEHICLES_ATS = "VehiclesATS";
	protected const string TITLE_VEHICLE = "Vehicle";
	protected const string ATTR_VEHICLE_CHECK = "Check";
	protected const string ATTR_VEHICLE_ID = "VehicleID";
	protected const string ATTR_MODEL_TYPE = "ModelType";
	protected const string ATTR_LOOK = "Look";
	protected const string ATTR_VARIANT = "Variant";
	protected readonly string SavePath = savePath;

	public void SaveAddon(AccAddonBinding data) {
		WriteXmlDeclaration();
		SkipCreatingIFEmpty = true;

		WriteElement(TITLE_ACCESSORY_ADDON, () => {
			WriteAttribute(ATTR_GAME_VERSION, Instances.GameVersion);

			WriteValueElement(TITLE_MODEL_NAME, data.ModelName);
			WriteValueElement(TITLE_DISPLAY_NAME, data.DisplayName);
			WriteValueElement(TITLE_PRICE, data.Price ?? 1);
			WriteValueElement(TITLE_UNLOCK_LEVEL, data.UnlockLevel ?? 0);
			WriteValueElement(TITLE_ICON_NAME, data.IconName);
			WriteValueElement(TITLE_PART_TYPE, data.PartType);
			WriteValueElement(TITLE_MODEL_PATH, data.ModelPath);
			WriteValueElement(TITLE_MODEL_PATH_UK, data.ModelPathUK);
			WriteValueElement(TITLE_EXT_MODEL_PATH, data.ExtModelPath);
			WriteValueElement(TITLE_EXT_MODEL_PATH_UK, data.ExtModelPathUK);
			WriteValueElement(TITLE_COLLISION_PATH, data.CollPath);
			WriteValueElement(TITLE_MODEL_TYPE, data.AccType);
			WriteValueElement(TITLE_LOOK, data.Look);
			WriteValueElement(TITLE_VARIANT, data.Variant);
			WriteValueElement(TITLE_HIDE_IN, data.HideIn);
			WriteValueElement(TITLE_ELECTRIC_TYPE, data.ElectricType);

			WriteList(TITLE_DATA_LIST, data.Data, TITLE_DATA);
			WriteList(TITLE_SUITABLE_FOR_LIST, data.SuitableFor, TITLE_SUITABLE_FOR);
			WriteList(TITLE_CONFLICT_WITH_LIST, data.ConflictWith, TITLE_CONFLICT_WITH);
			WriteList(TITLE_DEFAULTS_LIST, data.Defaults, TITLE_DEFAULTS);
			WriteList(TITLE_OVERRIDES_LIST, data.Overrides, TITLE_OVERRIDES);
			WriteList(TITLE_REQUIRE_LIST, data.Require, TITLE_REQUIRE);

			WriteVehicles(TITLE_VEHICLES_ETS2, data.VehiclesETS2);
			WriteVehicles(TITLE_VEHICLES_ATS, data.VehiclesATS);
		});
		SaveDocument(SavePath);
	}

	protected void WriteList(string listTitle, Collection<string> list, string subTitle) {
		if (list.Count == 0)
			return;
		WriteElement(listTitle, () => {
			foreach (var item in list) {
				WriteValueElement(subTitle, item);
			}
		});
	}

	protected void WriteVehicles(string listTitle, Collection<AccessoryVehicle> vehicles) {
		if (vehicles.Count == 0)
			return;
		WriteElement(listTitle, () => {
			foreach (var te in vehicles) {
				WriteElement(TITLE_VEHICLE, () => {
					WriteAttribute(ATTR_VEHICLE_CHECK, te.Check);
					WriteAttribute(ATTR_VEHICLE_ID, te.VehicleID);
					WriteAttribute(ATTR_MODEL_TYPE, te.ModelType);
					WriteAttribute(ATTR_LOOK, te.Look);
					WriteAttribute(ATTR_VARIANT, te.Variant);
				});
			}
		});
	}

	public void LoadAddon(AccAddonBinding data) {
		doc.Load(SavePath);
		XmlNode? addonNode = doc.SelectSingleNode(TITLE_ACCESSORY_ADDON);
		if (addonNode == null)
			return;
		foreach (XmlNode child in addonNode.ChildNodes) {
			switch (child.Name) {
				case TITLE_MODEL_NAME:
					data.ModelName = child.InnerText;
					break;
				case TITLE_DISPLAY_NAME:
					data.DisplayName = child.InnerText;
					break;
				case TITLE_PRICE:
					data.Price = long.Parse(child.InnerText);
					break;
				case TITLE_UNLOCK_LEVEL:
					data.UnlockLevel = uint.Parse(child.InnerText);
					break;
				case TITLE_ICON_NAME:
					data.IconName = child.InnerText;
					break;
				case TITLE_PART_TYPE:
					data.PartType = child.InnerText;
					break;
				case TITLE_MODEL_PATH:
					data.ModelPath = child.InnerText;
					break;
				case TITLE_MODEL_PATH_UK:
					data.ModelPathUK = child.InnerText;
					break;
				case TITLE_EXT_MODEL_PATH:
					data.ExtModelPath = child.InnerText;
					break;
				case TITLE_EXT_MODEL_PATH_UK:
					data.ExtModelPathUK = child.InnerText;
					break;
				case TITLE_COLLISION_PATH:
					data.CollPath = child.InnerText;
					break;
				case TITLE_MODEL_TYPE:
					data.AccType = child.InnerText;
					break;
				case TITLE_LOOK:
					data.Look = child.InnerText;
					break;
				case TITLE_VARIANT:
					data.Variant = child.InnerText;
					break;
				case TITLE_HIDE_IN:
					data.HideIn = uint.Parse(child.InnerText);
					break;
				case TITLE_ELECTRIC_TYPE:
					data.ElectricType = child.InnerText;
					break;
				case TITLE_DATA_LIST:
					ReadList(child, TITLE_DATA, data.Data);
					data.InvokeChange(nameof(data.DataListContent));
					break;
				case TITLE_SUITABLE_FOR_LIST:
					ReadList(child, TITLE_SUITABLE_FOR, data.SuitableFor);
					data.InvokeChange(nameof(data.SuitableForListContent));
					break;
				case TITLE_CONFLICT_WITH_LIST:
					ReadList(child, TITLE_CONFLICT_WITH, data.ConflictWith);
					data.InvokeChange(nameof(data.ConflictWithListContent));
					break;
				case TITLE_DEFAULTS_LIST:
					ReadList(child, TITLE_DEFAULTS, data.Defaults);
					data.InvokeChange(nameof(data.DefaultsListContent));
					break;
				case TITLE_OVERRIDES_LIST:
					ReadList(child, TITLE_OVERRIDES, data.Overrides);
					data.InvokeChange(nameof(data.OverridesListContent));
					break;
				case TITLE_REQUIRE_LIST:
					ReadList(child, TITLE_REQUIRE, data.Require);
					data.InvokeChange(nameof(data.RequireListContent));
					break;
				case TITLE_VEHICLES_ETS2:
					data.SelectedCountETS2 = ReadVehicles(child, data.VehiclesETS2);
					break;
				case TITLE_VEHICLES_ATS:
					data.SelectedCountATS = ReadVehicles(child, data.VehiclesATS);
					break;
			}
		}
	}

	protected static void ReadList(XmlNode node, string title, Collection<string> list) {
		foreach (XmlNode c in node.ChildNodes) {
			if (c.Name == title)
				list.Add(c.InnerText);
		}
	}

	protected static int ReadVehicles(XmlNode node, Collection<AccessoryVehicle> vehicles) {
		Dictionary<string, AccessoryVehicle> vehicleDict = vehicles.ToDictionary(v => v.VehicleID);
		int selected = 0;
		foreach (XmlNode v in node.ChildNodes) {
			if (v.Name != TITLE_VEHICLE)
				continue;
			var vehicleID = GetAttribute(v, ATTR_VEHICLE_ID);
			if (vehicleDict.TryGetValue(vehicleID, out var vehicle)) {
				vehicle.Check = GetAttributeBool(v, ATTR_VEHICLE_CHECK);
				if (vehicle.Check)
					selected++;
				vehicle.ModelType = GetAttribute(v, ATTR_MODEL_TYPE);
				vehicle.Look = GetAttribute(v, ATTR_LOOK);
				vehicle.Variant = GetAttribute(v, ATTR_VARIANT);
				vehicleDict.Remove(vehicleID);
			}
		}
		foreach (var vehicleKV in vehicleDict) {//如果出现记录里没有出现过的新车，会导致新车内容没有被读取内容覆盖。
			var vehicle = vehicleKV.Value;
			vehicle.Check = false;
			vehicle.ModelType = "";
			vehicle.Look = "";
			vehicle.Variant = "";
		}
		return selected;
	}
}