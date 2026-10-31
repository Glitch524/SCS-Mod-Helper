using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace SCS_Mod_Helper.HttpService;

public class GithubAvatarService: IIOService {
	protected static readonly HttpClient httpClient = new(new SocketsHttpHandler {
		PooledConnectionLifetime = TimeSpan.FromMinutes(2)
	});
	static GithubAvatarService() {
		httpClient.DefaultRequestHeaders.Add("User-Agent", "SCS Mod Helper/2.0");
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
	}

	public async Task<UserInfo?> GetAvatarAsync() {
		try {
			var url = $"https://api.github.com/users/{Uri.EscapeDataString("Glitch524")}";
			var user = await httpClient.GetFromJsonAsync<GithubUserInfo>(url);
			if (user == null)
				return null;
			return new UserInfo {
				DisplayName = user.Name ?? user.Login,
				AvatarUrl = user.AvatarUrl
			};
		} catch (Exception e) {
			MessageBox.Show("发生异常");
			Debug.Write(e.Message);
		}
		return null;
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