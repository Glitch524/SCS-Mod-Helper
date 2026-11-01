using SCS_Mod_Helper.Utils;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SCS_Mod_Helper.Main.About.AvatarNameService;

public class BiliAuthorInfo: AuthorInfo {//吊李卤味，拿个头像同做贼咁
	private static readonly CookieContainer cookies = new();

	protected override HttpClient HttpClient => mHttpClient;

	private static readonly HttpClient mHttpClient = new(new SocketsHttpHandler {
		PooledConnectionLifetime = TimeSpan.FromMinutes(2),
		CookieContainer = cookies,
		AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
	});



	public BiliAuthorInfo(): base(
			"https://space.bilibili.com/1970055",
			Paths.BiliAvatarFile(),
			Paths.BiliNameFile()) {
		HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
			"(KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36");

		HttpClient.DefaultRequestHeaders.Referrer = new Uri("https://space.bilibili.com/");
		HttpClient.DefaultRequestHeaders.Add("Origin", "https://space.bilibili.com");
		HttpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
		HttpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9");
	}

	/// <summary>
	/// 对参数值中的 !'()* 字符进行过滤
	/// </summary>
	private static string FilterSpecialChars(string s) => new(Array.FindAll(s.ToCharArray(), c => !"!'()*".Contains(c)));


	protected override async Task<(string? AvatarUrl, string? DisplayName)> GetUserInfoAsync() {
		await EnsureCookiesAsync();

		var parameters = new Dictionary<string, string> { { "mid", "1970055" } };

		var wrid = await CalcWridAsync(parameters);
		parameters["w_rid"] = wrid;

		var queryParts = new List<string>();
		foreach (var kv in parameters) {
			queryParts.Add($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}");
		}

		var url = $"https://api.bilibili.com/x/space/acc/info?" + string.Join("&", queryParts);

		Debug.WriteLine($"[WBI] 请求URL：{url}");

		using var resp = await HttpClient.GetAsync(url);
		var body = await resp.Content.ReadAsStringAsync();

		Debug.WriteLine($"[WBI] HTTP状态码: {resp.StatusCode}");
		Debug.WriteLine($"[WBI] 响应头:");
		foreach (var header in resp.Headers) {
			Debug.WriteLine($"\t{header.Key}:{string.Join(", ", header.Value)}");
		}
		Debug.WriteLine($"[WBI] 响应体: {body}");

		BiliApiResponse? response = null;
		try {
			response = JsonSerializer.Deserialize<BiliApiResponse>(body);
			if (response == null) {
				Debug.WriteLine("[WBI] 响应体解析后为null");
				return (null, null);
			}
		} catch (Exception e) {
			Debug.WriteLine($"[WBI]反序列化失败：{e.Message}");
			return (null, null);
		}
		if (response?.Code != 0 || response.Data == null)
			return (null, null);
		return (response.Data.Face, response.Data.Name);
	}

	private async Task EnsureCookiesAsync() {
		if (cookies.Count > 0)
			return;
		Debug.WriteLine("[WBI] 首次请求首页获取Cookie...");
		var homeResp = await HttpClient.GetAsync("https://www.bilibili.com/");
		Debug.WriteLine($"[WBI] 首页HTTP状态码：{homeResp.StatusCode}, cookie数量：{cookies.Count}");
		var apiResp = await HttpClient.GetAsync("https://api.bilibili.com/");
		Debug.WriteLine($"[WBI] API HTTP状态码：{apiResp.StatusCode}, cookie数量：{cookies.Count}");

		var wwwCookies = cookies.GetCookies(new Uri("https://www.bilibili.com/"));
		var apiCookies = cookies.GetCookies(new Uri("https://api.bilibili.com/"));

		foreach (Cookie wwwCookie in wwwCookies) {
			if (wwwCookie.Name == "b_nut")
				continue;
			bool alreadyExists = false;
			foreach (Cookie apiCookie in apiCookies) {
				if (apiCookie.Name == wwwCookie.Name) {
					alreadyExists = true;
					break;
				}
			}
			if (!alreadyExists) {
				cookies.Add(new Cookie(wwwCookie.Name, wwwCookie.Value) {
					Domain = ".bilibili.com",
					Path = "/"
				});
			}
			Debug.WriteLine($"\t首页Cookie:{wwwCookie.Name} = {wwwCookie.Value}");
		}
		foreach (Cookie apiCookie in apiCookies) {
			Debug.WriteLine($"\tapiCookie:{apiCookie.Name} = {apiCookie.Value}");
		}
	}

	/// <summary>
	/// 计算 w_rid 签名
	/// </summary>
	private async Task<string> CalcWridAsync(Dictionary<string, string> parameters) {
		var (imgKey, subKey) = await GetWbiKeysAsync();
		var mixinKey = GetMixinKey(imgKey, subKey);//"ea1db124af3c7062474693fa704f4ff8"

		//添加wts（当前unix时间戳，秒级）
		var wts = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		parameters["wts"] = wts.ToString();

		//按key字典序排序
		var sorted = new SortedDictionary<string, string>(parameters);

		//URL编码+拼接
		var parts = new List<string>();
		foreach (var kv in sorted) {
			var key = Uri.EscapeDataString(kv.Key);
			var value = Uri.EscapeDataString(FilterSpecialChars(kv.Value));
			parts.Add($"{key}={value}");
		}
		var query = string.Join("&", parts);

		//MD5(query + mixinKey) => w_rid
		var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(query + mixinKey));
		var wrid = Convert.ToHexStringLower(hashBytes);

		return wrid;
	}

	private static readonly char splitChar = '|';
	/// <summary>
	/// 从 nav 接口获取 img_key 和 sub_key
	/// </summary>
	private async Task<(string imgKey, string subKey)> GetWbiKeysAsync() {
		// img_key 和 sub_key 每 12 小时轮换一次，缓存一段时间内复用，不要每次请求都重新获取。

		string wridPath = Paths.WridFile();
		string imgKey, subKey;
		if (File.Exists(wridPath)) {
			var split = File.ReadAllText(wridPath).Split(splitChar);
			string time = split[0];
			var wridExpTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(time));
			if (DateTimeOffset.UtcNow < wridExpTime) {
				imgKey = split[1];
				subKey = split[2];
				return (imgKey, subKey);
			}
		}

		var navUrl = "https://api.bilibili.com/x/web-interface/nav";
		var json = await HttpClient.GetStringAsync(navUrl);
		var doc = JsonDocument.Parse(json);
		var root = doc.RootElement;

		var wbiImg = root.GetProperty("data").GetProperty("wbi_img");
		var imgUrl = wbiImg.GetProperty("img_url").GetString();
		var subUrl = wbiImg.GetProperty("sub_url").GetString();

		// 提取文件名（去掉路径和扩展名）
		static string ExtractKey(string url) {
			var filename = url[(url.LastIndexOf('/') + 1)..];
			return filename[..filename.LastIndexOf('.')];
		}
		imgKey = ExtractKey(imgUrl!);//"7cd084941338484aae1ad9425b84077c"
		subKey = ExtractKey(subUrl!);//"4932caff0ff746eab6f01bf08b70ac45"

		var expTime = DateTimeOffset.UtcNow.AddHours(12);
		File.WriteAllText(wridPath, $"{expTime.ToUnixTimeMilliseconds()}{splitChar}{imgKey}{splitChar}{subKey}");

		return (imgKey, subKey);
	}
	// 混排索引表（来自B站前端源码，固定不变）
	private static readonly int[] MixinKeyEncTab =
	[
		46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35,
		27, 43, 5, 49, 33, 9, 42, 19, 29, 28, 14, 39, 12, 38, 41,
		13, 37, 48, 7, 16, 24, 55, 40, 61, 26, 17, 0, 1, 60, 51,
		30, 4, 22, 25, 54, 21, 56, 59, 6, 63, 57, 62, 11, 36, 20, 34, 44, 52
	];


	/// <summary>
	/// 通过 img_key + sub_key 生成 mixin_key（32位）
	/// </summary>
	private static string GetMixinKey(string imgKey, string subKey) {
		var orig = imgKey + subKey;
		var sb = new StringBuilder();
		foreach (var i in MixinKeyEncTab) {
			sb.Append(orig[i]);
		}
		return sb.ToString()[..32];
	}

}

public class BiliUserInfo {
	[JsonPropertyName("name")]
	public string? Name { get; set; }
	[JsonPropertyName("face")]
	public string? Face { get; set; }
}
public class BiliApiResponse {
	[JsonPropertyName("code")]
	public int Code { get; set; }
	[JsonPropertyName("data")]
	public BiliUserInfo? Data { get; set; }
	[JsonPropertyName("msg")]
	public string? Msg { get; set; }
	[JsonPropertyName("message")]
	public string? Message { get; set; }
}