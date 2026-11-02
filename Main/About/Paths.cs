using System.IO;

namespace SCS_Mod_Helper.Utils; 
public partial class Paths {
	public static string UpdateCheckHistoryPath() => Path.Combine(AppLocalPath, "UpdateCheckHistory");
	public static string ExtraDescPath() => Path.Combine(AppLocalPath, "ExtraDesc");
	public static string BiliAvatarFile() => Path.Combine(AppLocalPath, "bili_user_avatar.png");
	public static string BiliNameFile() => Path.Combine(AppLocalPath, "bili_username");
	public static string WridFile() => Path.Combine(AppLocalPath, "wbi");
	public static string GithubAvatarFile() => Path.Combine(AppLocalPath, "github_user_avatar.png");
	public static string GithubNameFile() => Path.Combine(AppLocalPath, "github_username");
	public static string SteamAvatarFile() => Path.Combine(AppLocalPath, "steam_user_avatar.png");
	public static string SteamNameFile() => Path.Combine(AppLocalPath, "steam_username");
}
