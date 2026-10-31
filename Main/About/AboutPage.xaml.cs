using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.HttpService;
using SCS_Mod_Helper.Utils;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Wpf.Ui.Input;

namespace SCS_Mod_Helper.Main.About;
/// <summary>
/// AboutWindow.xaml 的交互逻辑
/// </summary>
public partial class AboutPage: BasePage {
	private readonly AboutBinding Binding;

	public AboutPage() {
		InitializeComponent();
		Binding = new(() => this);
		GridMain.DataContext = Binding;

		Task.Run(() => {
			_ = Binding.LoadAvatarAndName();
		});
	}

	private void HyperlinkClick(object sender, RequestNavigateEventArgs e) {
		System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
			FileName = e.Uri.ToString(),
			UseShellExecute = true
		});
		e.Handled = true;
	}
}

public class AboutBinding(Func<IBaseVisual> windowFunc): WindowBinding(windowFunc) {
	public static bool IsCN => Instances.CurrentLanguage == "zh-CN";

	private BitmapImage? mAvatar = null;
	public BitmapImage? Avatar {
		get => mAvatar;
		set {
			mAvatar = value;
			InvokeChange();
		}
	}

	private const string DefualtUsername = "Glitch524";
	private string mUsername = DefualtUsername;
	public string Username {
		get => mUsername;
		set {
			mUsername = value;
			InvokeChange();
		}
	}

	private static string UserLink => IsCN ? "https://space.bilibili.com/1970055" : "https://github.com/Glitch524";

	public static ICommand UserLinkCommand => new RelayCommand<object>(o => {
		System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
			FileName = UserLink,
			UseShellExecute = true
		});
	});

	public async Task LoadAvatarAndName() {
		string avatarPath = Paths.UserAvatarFile();
		string namePath = Paths.UsernameFile();
		DirectoryInfo avatarInfo = new(avatarPath);
		DirectoryInfo parent = new(Paths.AppPath);
		if (!parent.Exists)
			parent.Create();
		if (File.Exists(avatarPath) && File.Exists(namePath)) {
			var createTime = avatarInfo.CreationTime;
			var now = DateTime.Now;
			var span = now - createTime;
			if (span.TotalDays < 10) {
				_ = ReadANFile(avatarPath, namePath);
				return;
			}
		}
		IIOService avatarService = IsCN ? new BilibiliIOService() : new GithubAvatarService();
		var info = await avatarService.GetAvatarAsync();
		if (info != null) {
			await SaveANFile(info);
		}
		_ = ReadANFile(avatarPath, namePath);
	}

	private async Task ReadANFile(string avatarPath, string namePath) {
		var result = await Task.Run(() => {
			var bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			if (File.Exists(avatarPath)) {
				bitmap.StreamSource = new MemoryStream(File.ReadAllBytes(avatarPath));
			} else {
				var uri = new Uri($"pack://application:,,,/Modding/PaintJob/arrow.png", UriKind.RelativeOrAbsolute);
				//var uri = new Uri($"pack://application:,,,/Main/About/head.png", UriKind.RelativeOrAbsolute);
				bitmap.UriSource = uri;
			}
			bitmap.EndInit();
			bitmap.Freeze();

			string name;
			bool profileCN;
			if (File.Exists(namePath)) {
				name = File.ReadAllText(namePath);
			} else {
				name = DefualtUsername;
				profileCN = IsCN;
			}
			return (bitmap, name);
		});
		Avatar = result.bitmap;
		Username = result.name;
	}


	protected static readonly HttpClient httpClient = new(new SocketsHttpHandler {
		PooledConnectionLifetime = TimeSpan.FromMinutes(2)
	});

	private static async Task SaveANFile(UserInfo info) {
		string avatarPath = Paths.UserAvatarFile();
		string namePath = Paths.UsernameFile();
		string tempPath = avatarPath + ".tmp";

		try {
			await using (var stream = await httpClient.GetStreamAsync(info.AvatarUrl)) {
				await using var tempFile = File.Create(tempPath);
				await stream.CopyToAsync(tempFile);
			}
			File.Move(tempPath, avatarPath, overwrite: true);
		} catch {
			if (File.Exists(tempPath))
				File.Delete(tempPath);
		}

		if (info.DisplayName != null) {
			File.Delete(namePath);
			File.WriteAllText(namePath, $"{info.DisplayName}");
		}
	}
}