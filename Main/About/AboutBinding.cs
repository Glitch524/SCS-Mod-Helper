using SCS_Mod_Helper.Main.About.AvatarNameService;
using SCS_Mod_Helper.Main.About.VersionCheck;
using SCS_Mod_Helper.Setting;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

		public ObservableCollection<string> ListSponserBili { get; } = [];

		public ObservableCollection<string> ListSponserKofi { get; } = [];
	}
}
