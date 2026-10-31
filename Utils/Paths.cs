using System.IO;

namespace SCS_Mod_Helper.Utils;

partial class Paths {
	private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
	public static string AppPath = Path.Combine(LocalAppData, "SCS Mod Helper");

	public static string ManifestFile(string? projectLocation = null) => Path.Combine(projectLocation ?? Instances.ProjectLocation, "manifest.sii");


	public static string EmbDBFile() => Path.Combine(AppPath, "TempVehicles.db");
	public static string EmbHashFile() => Path.Combine(AppPath, "TempVehicles.hash");
	public static string UserAvatarFile() => Path.Combine(AppPath, "user_avatar.png");
	public static string UsernameFile() => Path.Combine(AppPath, "username");
	public static string WridFile() => Path.Combine(AppPath, "wbi");


	public static string CustomDBPath() => @"Vehicles\CustomVehicles.db";
	public static string VehiclesDBPath() => "Vehicles.db";


	public static string StorageDBPath() => "Storage.dll";

	public static string LanguageDir() => "Language";
	public static string VehiclesLanguageDir() => "Vehicles";


	public static string ExcpPath() => $"Excp_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt";
}
