using SCS_Mod_Helper.Main.About.AvatarNameService;
using SCS_Mod_Helper.Main.About.VersionCheck;
using SCS_Mod_Helper.Setting;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace SCS_Mod_Helper.Main {
	public partial class MainBinding {
		public static string AppVersion => Instances.AppVersionString;

		public bool HasUpdate => RemoteVersion != null && RemoteVersion > Instances.AppVersion;

		public static bool ShowInfoBadge => Settings.Default.ShowUpdateAvailable;

		public Version? RemoteVersion { 
			get; 
			set{
				field = value;
				InvokeChange();
				InvokeChange(nameof(HasUpdate));
				InvokeChange(nameof(RemoteVersionToolTip));
			}
		} = null;

		public bool IsHistoryVersion = false;

		public string? RemoteVersionToolTip => 
			RemoteVersion != null ? 
				GetString("ToolTipNewestVersion", RemoteVersion) : 
				(UpdateCheckFailed ? 
					GetString("ToolTipUpdateCheckFailed") : 
					null);

		public bool UpdateCheckFailed {
			get;
			set {
				field = value;
				InvokeChange();
			}
		} = false;

		public string? UpdateCheckFailedMessage = null;

		public bool PopupUpdateResultOpen { 
			get; 
			set {
				field = value;
				InvokeChange();
			} 
		} = false;

		public ICommand CheckUpdatePopupCommand => new RelayCommand<object>((o) => {
			PopupUpdateResultOpen = !PopupUpdateResultOpen;
		});


		private VersionChecker? Checker = null;
		public CancellationToken CancellationToken = new();
		public bool Checking { 
			get;
			set {
				field = value;
				InvokeChange();
			} 
		} = false;

		public ICommand RetryUpdateCheckCommand => new RelayCommand<object>((o) => CheckUpdate());

		public void CheckUpdate() {
			if (Settings.Default.UpdateCheckFrequency == "never")
				return;
			Task.Run(async () => {
				Checking = true;
				Checker ??= new();
				try {
					(RemoteVersion, IsHistoryVersion) = await Checker.CheckAsync(CancellationToken);
					if (RemoteVersion == null) 
						throw new Exception("Can't get remote version");
					UpdateCheckFailed = false;
					UpdateCheckFailedMessage = null;
				} catch (Exception e) {
					Debug.WriteLine(e.Message);
					RemoteVersion = null;
					IsHistoryVersion = false;
					UpdateCheckFailed = true;
					UpdateCheckFailedMessage = e.Message;
				}
				Checking = false;
			});
		}

		public static bool IsCN => Instances.CurrentLanguage == "zh-CN";

		private int mSelectedTab = 0;
		public int SelectedTab {
			get => mSelectedTab;
			set {
				mSelectedTab = value;
				InvokeChange();
				InvokeChange(nameof(CurrentInfo));
			}
		}

		private AuthorInfo? BiliAuthorInfo {
			get {
				field ??= new BiliAuthorInfo();
				return field;
			}
		}
		private AuthorInfo? GithubAuthorInfo {
			get {
				field ??= new GithubAuthorInfo();
				return field;
			}
		}
		private AuthorInfo? SteamAuthorInfo {
			get {
				field ??= new SteamAuthorInfo();
				return field;
			}
		}

		public AuthorInfo? CurrentInfo => SelectedTab switch {
			0 => BiliAuthorInfo,
			1 => GithubAuthorInfo,
			2 => SteamAuthorInfo,
			_ => null,
		};

		public static ICommand CommandAuthorLink => new RelayCommand<object>(o => {
			if (o is string authorLink) {
				Process.Start(new ProcessStartInfo {
					FileName = authorLink,
					UseShellExecute = true
				});
			}
		});

		public static ICommand Command302 => new RelayCommand<object>(o => {
			Process.Start(new ProcessStartInfo {
				FileName = "https://www.dogfight360.com/blog/18682/",
				UseShellExecute = true
			});
		});

		private bool loaded = false;
		private readonly ObservableCollection<string> mListSpBili = [];
		public ObservableCollection<string> ListSpBili {
			get {
				if (!loaded && !loading) 
					_ = LoadSpRawFilesAsync();
				return mListSpBili;
			}
		}

		private readonly ObservableCollection<string> mListSpKofi = [];
		public ObservableCollection<string> ListSpKofi {
			get {
				if (!loaded && !loading) 
					_ = LoadSpRawFilesAsync();
				return mListSpKofi;
			}
		}

		public string mExtraDesc;
		public string ExtraDesc {
			get {
				if (!loaded && !loading)
					_ = LoadSpRawFilesAsync();
				return mExtraDesc;
			}
			set {
				if (value.Contains("$#10;"))
					value = value.Replace("$#10;", "\n");
				mExtraDesc = value;
				InvokeChange();
			}
		}

		private bool loading = false;
		public async Task LoadSpRawFilesAsync() {
			loading = true;
			mListSpBili!.Clear();
			mListSpKofi!.Clear();
			await LoadSpRawFileAsync("bili");
			await LoadSpRawFileAsync("kofi");
			await LoadExtraDescRawFileAsync();

			loaded = true;
			loading = false;
		}

		private async Task LoadSpRawFileAsync(string type) {
			ObservableCollection<string> list;
			string url;
			if (type == "bili") {
				list = mListSpBili!;
				url = $"https://raw.githubusercontent.com/Glitch524/SCS-Mod-Helper/refs/heads/master/Main/About/SpBili.txt";
			} else if (type == "kofi") {
				list = mListSpKofi!;
				url = $"https://raw.githubusercontent.com/Glitch524/SCS-Mod-Helper/refs/heads/master/Main/About/SpKofi.txt";
			} else
				return;

			await using var stream = await HttpClient.GetStreamAsync(url);
			using var reader = new StreamReader(stream);

			var buffer = new List<string>();
			string? line;
			
			while((line = await reader.ReadLineAsync()) != null) {
				CancellationToken.ThrowIfCancellationRequested();
				if (!string.IsNullOrWhiteSpace(line))
					buffer.Add(line);

				if (buffer.Count >= 50) {
					foreach (var item in buffer)
						list.Add(item);
					buffer.Clear();
					await Task.Yield();
				}
			}
			if (buffer.Count > 0) {
				foreach (var item in buffer) {
					list.Add(item);
				}
			}
		}

		private async Task LoadExtraDescRawFileAsync() {
			var extraDescFile = new FileInfo(Paths.ExtraDescPath());
			if(extraDescFile.Exists) {
				var creationTime = extraDescFile.CreationTime;
				if (DateTime.Now - creationTime < TimeSpan.FromDays(1)) {
					var lines = await File.ReadAllLinesAsync(extraDescFile.FullName);
					foreach (var line in lines) {
						if (line.StartsWith(Instances.CurrentLanguage)) {
							Application.Current.Dispatcher.Invoke(() => {
								ExtraDesc = line[(line.IndexOf('=') + 1)..];
							});
						}
					}
					return;
				}
			}

			string url = $"https://raw.githubusercontent.com/Glitch524/SCS-Mod-Helper/refs/heads/master/Main/About/ExtraDesc.txt";
			var desc = await HttpClient.GetStringAsync(url);
			await File.WriteAllTextAsync(extraDescFile.FullName, desc);
			var split = desc.Split("\n");
			foreach(var line in split) {
				if (line.StartsWith(Instances.CurrentLanguage)) {
					Application.Current.Dispatcher.Invoke(() => {
						ExtraDesc = line[('=' + 1)..];
					});
				}
			}
		}


		private HttpClient? mHttpClient = null;
		private HttpClient HttpClient {
			get {
				if(mHttpClient == null) {
					mHttpClient = new(new SocketsHttpHandler {
						PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2)
					}) {
						Timeout = TimeSpan.FromSeconds(30)
					};
					mHttpClient.DefaultRequestHeaders.Add("User-Agent", "SCS Mod Helper/2.0");
					mHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
				}
				return mHttpClient;
			}
		}
	}
}
