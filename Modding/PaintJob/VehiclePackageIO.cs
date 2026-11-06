using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace SCS_Mod_Helper.Modding.PaintJob;

class VehiclePackageIO: AppIO {
	public const string TITLE_PAINTJOB_PACKAGE = "PaintjobPackage";
	public const string ATTR_GAME_VERSION = "GameVersion";
	public const string ATTR_VEHICLE_ID = "VehicleID";

	public const string TITLE_GROUP = "Group";
	public const string ATTR_TYPE = "Type";

	public const string TITLE_LAYER = "Layer";
	public const string ATTR_ID = "ID";

	public const string TITLE_PATH = "Path";
	public const string TITLE_SUITABLE_FOR = "SuitableFor";
	public const string TITLE_CONFLICT_WITH = "ConflictWith";

	public VehiclePackage? ReadVehiclePackage(string vehicleID, Action recycleAction) {
		try {
			var packageFile = Paths.VehiclesPackageDir(vehicleID);
			if (File.Exists(packageFile)) {
				//读取zip 读取里面的manifest，然后获得对应的文件和名字
				using FileStream fs = new(packageFile, FileMode.Open);
				using ZipArchive pFile = new(fs, ZipArchiveMode.Read);
				var manifest = pFile.GetEntry("manifest.xml") ?? throw new ArgumentException("This is not a vehicle package");
				var stream = manifest.Open();

				doc.Load(stream);
				XmlNode? pNode = doc.SelectSingleNode(TITLE_PAINTJOB_PACKAGE) ?? throw new ArgumentException("This is not a vehicle package");

				var gameVersion = GetAttribute(pNode, ATTR_GAME_VERSION);
				var fileVID = GetAttribute(pNode, ATTR_VEHICLE_ID);
				if (vehicleID != fileVID)
					throw new ArgumentException("This is not a vehicle package");

				VehiclePackage vehiclePackage = new(vehicleID);
				foreach (XmlNode groupNode in pNode.ChildNodes) {
					if (groupNode.Name == TITLE_GROUP) {
						var groupType = GetAttribute(groupNode, ATTR_TYPE);
						var group = vehiclePackage.Add(groupType);
						foreach (XmlNode layer in groupNode.ChildNodes) {
							if (layer.Name == TITLE_LAYER) {
								var layerID = GetAttribute(layer, ATTR_ID);
								string? path = GetChildNode(layer, TITLE_PATH)?.InnerText;
								if (path == null)
									continue;
								group.Add(layerID, path);
							}
						}
					}
				}
				vehiclePackage.SortGroups();
				RecyclePackages(recycleAction);
				return vehiclePackage;
			} else
				throw new ArgumentException("Package not exist");
		} catch (Exception e) {
			Debug.WriteLine(e);
		}
		return null;
	}

	private static readonly List<Action> recycleList = [];
	public static void RecyclePackages(Action recycleAction) {
		recycleList.Add(recycleAction);
		while (recycleList.Count > 3) {
			var f = recycleList.First();
			recycleList.RemoveAt(0);
			f.Invoke();
		}
	}
}
