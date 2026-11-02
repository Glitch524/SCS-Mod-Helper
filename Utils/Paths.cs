using System.IO;

namespace SCS_Mod_Helper.Utils;

partial class Paths {
	private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
	public static string AppLocalPath = Path.Combine(LocalAppData, "Glitch524", "SCS Mod Helper");

	public static string ManifestFile(string? projectLocation = null) => Path.Combine(projectLocation ?? Instances.ProjectLocation, "manifest.sii");


	public static string EmbDBFile() => Path.Combine(AppLocalPath, "TempVehicles.db");
	public static string EmbHashFile() => Path.Combine(AppLocalPath, "TempVehicles.hash");


	public static string CustomDBPath() => @"Vehicles\CustomVehicles.db";
	public static string VehiclesDBPath() => "Vehicles.db";


	public static string StorageDBPath() => "Storage.dll";

	public static string LanguageDir() => "Language";
	public static string VehiclesLanguageDir() => "Vehicles";


	public static string SampleEnglishPath() => @"Language\en-US_English(US).txt";
	public static string SampleExamplePath() => @"Language\example.txt";

	public static string ExcpDir() => "Exceptions";
	public static string ExcpPath() => @$"Exceptions\Excp_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt";
}
