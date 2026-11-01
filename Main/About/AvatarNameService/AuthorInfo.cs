using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SCS_Mod_Helper.Main.About.AvatarNameService; 

public abstract class AuthorInfo: BaseBinding {
	private const string DefaultUsername = "Glitch524";
	private string? mDisplayName = null;
	public string? DisplayName { 
		get {
			if (mDisplayName == null)
				return DefaultUsername;
			return mDisplayName;
		}
		set {
			mDisplayName = value;
			InvokeChange();
		}
	}

	public string? AvatarUrl {
		get;
		set {
			field = value;
			InvokeChange();
		}
	}

	public BitmapImage? Avatar {
		get;
		set {
			field = value;
			InvokeChange();
		}
	}

	public string? Link {
		get;
		set {
			field = value;
			InvokeChange();
		}
	}

	protected string AvatarPath;
	protected string NamePath;
	public AuthorInfo(string link, string avatarPath, string namePath) {
		Link = link;
		AvatarPath = avatarPath;
		NamePath = namePath;

		Task.Run(() => {
			_ = LoadAvatarAndName();
		});
	}
	//先找到本地的头像和名字，如果存在就先放在ui上，然后判断是否需要更新，如果需要更新就去获取最新的头像和名字，然后保存到本地，再更新ui上的头像和名字
	private async Task LoadAvatarAndName() {
		DirectoryInfo avatarInfo = new(AvatarPath);
		DirectoryInfo parent = new(Paths.AppLocalPath);
		if (!parent.Exists)
			parent.Create();
		bool updateNeeded = true;
		if (File.Exists(AvatarPath) && File.Exists(NamePath)) {//如果头像和名字文件都存在，检查头像文件的创建时间，如果小于10天，则直接读取文件，否则重新获取
			var createTime = avatarInfo.CreationTime;
			var now = DateTime.Now;
			var span = now - createTime;
			updateNeeded = span.TotalDays >= 10;

			Avatar = await ReadAvatarFile();//读取头像文件
			DisplayName = await ReadNameFile();//读取名字文件
		}
		if (updateNeeded) {
			var (avatarUrl, displayName) = await GetUserInfoAsync();//获取用户信息
			if (avatarUrl != null && displayName != null) {
				AvatarUrl = avatarUrl;
				DisplayName = displayName;
				await SaveANFile();//保存头像和名字到本地
				Avatar = await ReadAvatarFile();//只要读取头像文件就行了，名字已经在上面更新了
			}
		}
	}

	protected abstract Task<(string? AvatarUrl, string? DisplayName)> GetUserInfoAsync();


	protected abstract HttpClient HttpClient { get; }

	private async Task SaveANFile() {
		string tempPath = AvatarPath + ".tmp";
		try {
			await using (var stream = await HttpClient.GetStreamAsync(AvatarUrl)) {
				await using var tempFile = File.Create(tempPath);
				await stream.CopyToAsync(tempFile);
			}
			File.Move(tempPath, AvatarPath, overwrite: true);
		} catch {
			if (File.Exists(tempPath))
				File.Delete(tempPath);
		}

		if (DisplayName != null) {
			File.Delete(NamePath);
			File.WriteAllText(NamePath, $"{DisplayName}");
		}
	}

	public async Task<BitmapImage> ReadAvatarFile() {
		byte[]? bytes = null;
		if (File.Exists(AvatarPath))
			bytes = await File.ReadAllBytesAsync(AvatarPath);
		await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

		var bitmap = new BitmapImage();
		bitmap.BeginInit();
		bitmap.CacheOption = BitmapCacheOption.OnLoad;
		try {
			if (bytes != null) {
				bitmap.StreamSource = new MemoryStream(bytes);
			} else {
				throw new ArgumentNullException("bytes", "Avatar file does not exist.");
			}
		} catch (Exception e) {
			if (e.Message != "Avatar file does not exist.")
				Debug.WriteLine($"[AuthorInfo] Failed to read avatar file:\n{e.Message}");
			var uri = new Uri($"pack://application:,,,/Main/About/DefaultIcon.jpg", UriKind.RelativeOrAbsolute);
			bitmap.UriSource = uri;
		}
		bitmap.EndInit();
		bitmap.Freeze();
		return bitmap;
	}

	public async Task<string> ReadNameFile() => File.Exists(NamePath) ? await File.ReadAllTextAsync(NamePath) : DefaultUsername;
}