using SCS_Mod_Helper.Utils;
using System.Net.Http;

namespace SCS_Mod_Helper.Main.About.AvatarNameService; 
public class SteamAuthorInfo: AuthorInfo {
	protected static readonly HttpClient httpClient = new(new SocketsHttpHandler {
		PooledConnectionLifetime = TimeSpan.FromMinutes(2)
	});
	protected override HttpClient HttpClient => httpClient;


	public SteamAuthorInfo() : base(
			"https://steamcommunity.com/id/Glitch524/",
			Paths.SteamAvatarFile(),
			Paths.SteamNameFile()) {

	}

	protected override async Task<(string? AvatarUrl, string? DisplayName)> GetUserInfoAsync() {
		return (null, null);
	}
}
