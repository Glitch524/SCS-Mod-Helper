using System.IO;

namespace SCS_Mod_Helper.Utils;

partial class Paths {
	public static string IntDecorsDir(string projectLocation) => Path.Combine(projectLocation, @"vehicle\truck\upgrade\interior_decors");

	public static string AccessoryIconDir(string sub = "") {
		var p = Path.Combine(Instances.ProjectLocation, @"material\ui\accessory");
		if (sub.Length > 0)
			p = Path.Combine(p, sub);
		return p;
	}

	public static string RopeDir(string sub = "") {
		var p = Path.Combine(Instances.ProjectLocation, @"material\ropes");
		if (sub.Length > 0)
			p = Path.Combine(p, sub);
		return p;
	}


	public static string DefTruckDir() => Path.Combine(Instances.ProjectLocation, @"def\vehicle\truck");
	private static string DefVehicleDir(string projectLocation, string vehicleType, string vehicleID) =>
		Path.Combine(projectLocation, @"def\vehicle", vehicleType, vehicleID);

	private static string AccAddonFile(string projectLocation, string vehicleType, string vehicleID, string modelType, string modelName) {
		string dir = Path.Combine(DefVehicleDir(projectLocation, vehicleType, vehicleID), "accessory", modelType);
		Directory.CreateDirectory(dir);
		return Path.Combine(dir, $"{modelName}.sii");
	}
	public static string AccAddonFile(string vehicleType, string vehicleID, string modelType, string modelName) =>
		AccAddonFile(Instances.ProjectLocation, vehicleType, vehicleID, modelType, modelName);



	public static string HookupStorageDir(string projectLocation) => Path.Combine(projectLocation, @"def\vehicle");
	public static string HookupStorageDir() => HookupStorageDir(Instances.ProjectLocation);
	public static string AddonHookupsDir(string projectLocation, string suiFilename) {
		string dir = Path.Combine(projectLocation, @"def\vehicle\addon_hookups");
		Directory.CreateDirectory(dir);
		return Path.Combine(dir, $"{suiFilename}.sui");
	}
	public static string AddonHookupsDir(string suiFilename) => AddonHookupsDir(Instances.ProjectLocation, suiFilename);



	public static string AccPaintJobFile(string projectLocation, string vehicleType, string vehicleID, string paintJobName) {
		string dir = Path.Combine(DefVehicleDir(projectLocation, vehicleType, vehicleID), "paint_job");
		Directory.CreateDirectory(dir);
		return Path.Combine(dir, $"{paintJobName}.sii");
	}
	public static string AccPaintJobFile(string vehicleType, string vehicleID, string paintJobName) =>
		AccPaintJobFile(Instances.ProjectLocation, vehicleType, vehicleID, paintJobName);


	public static string SimplePaintJobFile(string projectLocation, string vehicleType, string vehicleID, string paintJobName) {
		string dir = Path.Combine(DefVehicleDir(projectLocation, vehicleType, vehicleID), @"paint_job\accessory");
		Directory.CreateDirectory(dir);
		return Path.Combine(dir, $"{paintJobName}.sii");
	}
	public static string SimplePaintJobFile(string vehicleType, string vehicleID, string paintJobName) =>
		SimplePaintJobFile(Instances.ProjectLocation, vehicleType, vehicleID, paintJobName);


	public static string HookupFile(string projectLocation, string hookupName) {
		string dir = Path.Combine(projectLocation, @"unit\hookup\vehicle");
		Directory.CreateDirectory(dir);
		return Path.Combine(dir, $"{hookupName}.sii");
	}
	public static string HookupFile(string hookupName) => HookupFile(Instances.ProjectLocation, hookupName);

	public static string VehiclePath() {
		string dir = Path.Combine(Instances.ProjectLocation, @$"vehicle");
		Directory.CreateDirectory(dir);
		return dir;
	}

	public static string VehicleUpgradePath(string vehicleType) {
		string dir = Path.Combine(Instances.ProjectLocation, @$"vehicle\{vehicleType}\upgrade");
		Directory.CreateDirectory(dir);
		return dir;
	}


	public static string SavedPhysicsFile() => "Physics.DEP";
	public static string DefaultDEDDir() => "Def Files";
}
