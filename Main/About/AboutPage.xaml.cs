using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Main.About.AvatarNameService;
using SCS_Mod_Helper.Utils;
using System.Diagnostics;
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

		Binding.SelectedTab = 0;
	}

	private void HyperlinkClick(object sender, RequestNavigateEventArgs e) {
		Process.Start(new ProcessStartInfo {
			FileName = e.Uri.ToString(),
			UseShellExecute = true
		});
		e.Handled = true;
	}
}

public class AboutBinding(Func<IBaseVisual> windowFunc): WindowBinding(windowFunc) {
	public string AppVersion => Instances.Version;
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
}