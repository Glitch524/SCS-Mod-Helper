using SCS_Mod_Helper.Utils;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace SCS_Mod_Helper.Main.About.AvatarNameService;

public class GithubAuthorInfo: AuthorInfo {
	protected static readonly HttpClient httpClient = new(new SocketsHttpHandler {
		PooledConnectionLifetime = TimeSpan.FromMinutes(2)
	});
	protected override HttpClient HttpClient => httpClient;


	public GithubAuthorInfo(): base(
			"https://github.com/Glitch524",
			Paths.GithubAvatarFile(),
			Paths.GithubNameFile()) {
		HttpClient.DefaultRequestHeaders.Add("User-Agent", "SCS Mod Helper/2.0");
		HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
	}

	protected override async Task<(string? AvatarUrl, string? DisplayName)> GetUserInfoAsync() {
		try {
			var url = $"https://api.github.com/users/{Uri.EscapeDataString("Glitch524")}";
			var user = await HttpClient.GetFromJsonAsync<GithubUserInfo>(url);
			if (user == null)
				return (null, null);
			return (user.AvatarUrl, user.Name ?? user.Login);
		} catch (Exception e) {
			MessageBox.Show("发生异常");
			Debug.Write(e.Message);
		}
		return (null, null);
	}
}

public class GithubUserInfo {
	[JsonPropertyName("login")]
	public string? Login { get; set; }
	[JsonPropertyName("name")]
	public string? Name { get; set; }
	[JsonPropertyName("avatar_url")]
	public string? AvatarUrl { get; set; }
}