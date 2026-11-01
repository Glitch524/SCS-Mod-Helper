using SCS_Mod_Helper.Utils;
using System.Net.Http;
using System.Xml.Linq;

namespace SCS_Mod_Helper.Main.About.AvatarNameService; 
public class SteamAuthorInfo: AuthorInfo {
	//https://partner.steamgames.com/documentation/community_data?spm=5176.28103460.0.0.5a6c29887uIeL3
	protected static readonly HttpClient mHttpClient = new(new SocketsHttpHandler {
		PooledConnectionLifetime = TimeSpan.FromMinutes(2)
	});
	protected override HttpClient HttpClient => mHttpClient;

	public SteamAuthorInfo() : base(
			"https://steamcommunity.com/id/Glitch524/",
			Paths.SteamAvatarFile(),
			Paths.SteamNameFile()) {
		HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
	}

	protected override async Task<(string? AvatarUrl, string? DisplayName)> GetUserInfoAsync() {
		var url = $"https://steamcommunity.com/profiles/76561198064740566/?xml=1";
		var xml = await HttpClient.GetStringAsync(url);
		var doc = XDocument.Parse(xml);
		var name = doc.Root?.Element("steamID")?.Value;
		var avatarUrl = doc.Root?.Element("avatarFull")?.Value;
		return (avatarUrl, name);
	}
}