using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;
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
			var packagePath = Paths.VehiclesPackageDir();
			var packageFile = Path.Combine(packagePath, $"{vehicleID}.vep");
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
						var type = GetAttribute(groupNode, ATTR_TYPE);
						LayerGroup group = new(type);
						var isCabin = type.Equals("cabin", StringComparison.CurrentCultureIgnoreCase);
						vehiclePackage.Add(group);
						foreach (XmlNode layer in groupNode.ChildNodes) {
							if (layer.Name == TITLE_LAYER) {
								var id = GetAttribute(layer, ATTR_ID);
								string? path = null;
								foreach (XmlNode values in layer.ChildNodes) {
									if (values.Name == TITLE_PATH) {
										path = values.InnerText;
									}
								}
								if (path == null)
									continue;
								BitmapImage? image = GetUV(pFile, path);
								if (image == null)
									continue;
								VehicleUV uv = new(vehicleID, isCabin, id, image);
								vehiclePackage.AllUVs.Add(uv);
								group.Add(uv);
							}
						}
					}
				}
				RecyclePackages(recycleAction);
				return vehiclePackage;
			} else
				throw new ArgumentException("Package not exist");
		} catch (Exception e) {
			Debug.WriteLine(e);
		}
		return null;
	}

	private static BitmapImage? GetUV(ZipArchive pFile, string path) {
		var uvEntry = pFile.GetEntry(path);
		if (uvEntry == null)
			return null;
		try {
			byte[] data;
			using (var uvStream = uvEntry.Open()) {
				using var ms = new MemoryStream();
				uvStream.CopyTo(ms);
				data = ms.ToArray();
			}
			if (data.Length == 0) {
				throw new NullReferenceException($"Zip entry {path} is empty");
			}
			var bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.StreamSource = new MemoryStream(data);
			bitmap.EndInit();
			if (bitmap.CanFreeze)
				bitmap.Freeze();
			return bitmap;
		} catch (Exception e) {
			Debug.WriteLine(e);
			return null;
		}
	}

	private static List<Action> recycleList = [];
	public static void RecyclePackages(Action recycleAction) {
		recycleList.Add(recycleAction);
		while (recycleList.Count > 3) {
			var f = recycleList.First();
			recycleList.RemoveAt(0);
			f.Invoke();
		}
	}
}
