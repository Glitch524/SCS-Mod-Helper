using SCS_Mod_Helper.Class.Base;
using System.Windows;

namespace SCS_Mod_Helper.Class {
	/// <summary>
	/// ExtractMessageBox.xaml 的交互逻辑
	/// </summary>
	public partial class NotShowableMessageBox: BaseWindow {
		public NotShowableMessageBox(string message, string title, MessageBoxButton messageBoxButton, bool showNotShowAgain) {
			InitializeComponent();
			SetValue(message, title, messageBoxButton, showNotShowAgain);
		}
		public NotShowableMessageBox(string message, string title, MessageBoxButton messageBoxButton) {
			InitializeComponent();
			SetValue(message, title, messageBoxButton);

		}
		public NotShowableMessageBox(string message, MessageBoxButton messageBoxButton, bool showNotShowAgain) {
			InitializeComponent();
			SetValue(message, null, messageBoxButton, showNotShowAgain);
		}
		public NotShowableMessageBox(string message, MessageBoxButton messageBoxButton) {
			InitializeComponent();
			SetValue(message, null, messageBoxButton);
		}
		public NotShowableMessageBox(string message) {
			InitializeComponent();
			SetValue(message);
		}

		private void SetValue(string message, string? title = null, MessageBoxButton messageBoxButton = MessageBoxButton.OK, bool showNotShowAgain = false) {
			TextMessage.Text = message;
			Title = title ?? GetString("MessageTitleNotice");
			TitleBar.Title = title ?? GetString("MessageTitleNotice");
			CheckBoxDontShowAgain.Visibility = showNotShowAgain ? Visibility.Visible : Visibility.Collapsed;
			switch (messageBoxButton) {
				case MessageBoxButton.OK:
					ButtonPositive.Visibility = Visibility.Visible;
					ButtonNegative.Visibility = Visibility.Collapsed;
					ButtonNeutral.Visibility = Visibility.Collapsed;
					ButtonPositive.Content = GetString("ButtonOK");
					break;
				case MessageBoxButton.OKCancel:
					ButtonPositive.Visibility = Visibility.Visible;
					ButtonNegative.Visibility = Visibility.Collapsed;
					ButtonNeutral.Visibility = Visibility.Visible;
					ButtonPositive.Content = GetString("ButtonOK");
					ButtonNeutral.Content = GetString("ButtonCancel");
					break;
				case MessageBoxButton.AbortRetryIgnore:
					break;
				case MessageBoxButton.YesNoCancel:
					ButtonPositive.Visibility = Visibility.Visible;
					ButtonNegative.Visibility = Visibility.Visible;
					ButtonNeutral.Visibility = Visibility.Visible;
					ButtonPositive.Content = GetString("ButtonYes");
					ButtonNegative.Content = GetString("ButtonNo");
					ButtonNeutral.Content = GetString("ButtonCancel");
					break;
				case MessageBoxButton.YesNo:
					ButtonPositive.Visibility = Visibility.Visible;
					ButtonNegative.Visibility = Visibility.Visible;
					ButtonNeutral.Visibility = Visibility.Collapsed;
					ButtonPositive.Content = GetString("ButtonYes");
					ButtonNegative.Content = GetString("ButtonNo");
					break;
				case MessageBoxButton.RetryCancel:
					break;
				case MessageBoxButton.CancelTryContinue:
					break;
			}
		}

		public bool DontShowAgain = false;
		private void ButtonResult(object sender, RoutedEventArgs e) {
			DialogResult = true;
			DontShowAgain = CheckBoxDontShowAgain.IsChecked == true;
			Close();
		}
	}
}
