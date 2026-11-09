using SCS_Mod_Helper.Setting;
using SCS_Mod_Helper.Utils;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace SCS_Mod_Helper.Main.About.VersionCheck; 

public partial class VersionChecker {
	protected HttpClient HttpClient;

	private readonly int MaxRetryCount = 3;
	private readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

	public VersionChecker() {
		HttpClient = new(new SocketsHttpHandler {
			PooledConnectionLifetime = TimeSpan.FromMinutes(2),
			AllowAutoRedirect = false //禁止自动重定向
		}) {
			Timeout = TimeSpan.FromSeconds(15)
		};
		HttpClient.DefaultRequestHeaders.Add("User-Agent", "SCS Mod Helper/2.0");
		HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
	}

	public async Task<(Version version, bool isHistory)> CheckAsync(CancellationToken cancellationToken) {
		Exception? lastEx = null;
		var historyVersion = await LoadHistory();
		if (historyVersion != null)
			return (historyVersion, true);
		for (int i = 0; i < MaxRetryCount; i++) {
			try {
				var version = await GetRemoteVersionAsync(cancellationToken);
				await SaveHistory(version);
				return (version, false);
			} catch (TaskCanceledException e) {
				lastEx = e;
			} catch (HttpRequestException e) {
				lastEx = e;
			} catch (Exception e) {
				throw new InvalidOperationException("UnexpectedError", e);
			}
			if (i < MaxRetryCount - 1) {
				await Task.Delay(RetryDelay * (i + 1), cancellationToken);
			}
		}
		throw new TimeoutException("[Version Checker] FailedMultipleAttempts", lastEx);
	}

	private static async Task<Version?> LoadHistory() {
		var historyPath = Paths.UpdateCheckHistoryPath();
		if (File.Exists(historyPath)) {
			var line = await File.ReadAllTextAsync(historyPath);
			var split = line.Split('|');
			var milli = long.Parse(split[0]);
			var version = Version.Parse(split[1]);
			var nowMilli = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			var last = DateTimeOffset.FromUnixTimeSeconds(milli);
			var now = DateTimeOffset.FromUnixTimeSeconds(nowMilli);
			TimeSpan frequency;
			var ucf = Settings.Default.UpdateCheckFrequency;
			switch (ucf) {
				case "hourly":
					frequency = TimeSpan.FromHours(1);
					break;
				case "daily":
					frequency = TimeSpan.FromDays(1);
					break;
				case "weekly":
					frequency = TimeSpan.FromDays(7);
					break;
				default:
					return null;
			}
			if (now - last > frequency) {
				File.Delete(historyPath);
			} else
				return version;
		}
		return null;
	}

	private static async Task SaveHistory(Version version) {
		var historyPath = Paths.UpdateCheckHistoryPath();
		var nowMilli = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		var line = $"{nowMilli}|{version}";
		await File.WriteAllTextAsync(historyPath, line);
	}

	private async Task<Version> GetRemoteVersionAsync(CancellationToken cancellationToken) {
		var url = $"https://github.com/Glitch524/SCS-Mod-Helper/releases/latest";
		using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

		if (!IsRedirect(response.StatusCode)) {
			throw new HttpRequestException($"Unexpected response status code: {response.StatusCode}");
		}
		var location = response.Headers.Location?.ToString();
		if (string.IsNullOrEmpty(location)) {
			throw new HttpRequestException("Redirect location is missing.");
		}

		var match = CatchRegex().Match(location);
		if (!match.Success) {
			throw new HttpRequestException("Failed to extract version from redirect URL.");
		}

		var tagName = match.Groups[1].Value;
		var versionString = Instances.VersionRegex().Replace(tagName, "");
		if (!Version.TryParse(versionString, out var version)) {
			throw new HttpRequestException($"Failed to parse version from tag: {tagName}");
		}
		return version;
	}

	private static bool IsRedirect(HttpStatusCode statusCode) {
		return statusCode is 
			HttpStatusCode.Found or 
			HttpStatusCode.MovedPermanently or 
			HttpStatusCode.Redirect or
			HttpStatusCode.RedirectMethod or 
			HttpStatusCode.TemporaryRedirect;
	}

	[GeneratedRegex(@"/releases/tag/([^/?#]+)")]
	private static partial Regex CatchRegex();
}

public class VersionUtil {
	/// <summary>
	/// 将Github获得的版本号Tag转换为Version对象
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public static Version ParseTag(string tag) {
		if (string.IsNullOrWhiteSpace(tag))
			return new Version(0, 0, 0, 0);
		var clean = Instances.VersionRegex().Replace(tag,"");
		return Version.TryParse(clean, out var v) ? v : new Version(0, 0, 0, 0);
	}

	public static Version GetCurrentVersion() {
		var infoVersion = Instances.AppVersionString;
		if (string.IsNullOrWhiteSpace(infoVersion))
			return new Version(0, 0, 0, 0);
		return Version.TryParse(infoVersion, out var v) ? v : new Version(0, 0, 0, 0);
	}
}