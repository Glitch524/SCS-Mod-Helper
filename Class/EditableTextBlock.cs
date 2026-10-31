using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCS_Mod_Helper.Class; 
internal class EditableTextBlock: ContentControl {
	static EditableTextBlock() {
		DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTextBlock), new FrameworkPropertyMetadata(typeof(EditableTextBlock)));
	}

	public bool IsInEditMode {
		get => (bool)GetValue(IsInEditModeProperty);
		set => SetValue(IsInEditModeProperty, value);
	}

	public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register(
		nameof(IsInEditMode),
		typeof(bool),
		typeof(EditableTextBlock),
		new FrameworkPropertyMetadata(false, OnIsInEditModeChanged));

	private TextBlock? mTextBlock;
	private TextBox? mTextBox;

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		mTextBlock = GetTemplateChild("PART_TextBlock") as TextBlock;
		mTextBox = GetTemplateChild("PART_TextBox") as TextBox;

		mTextBlock?.MouseLeftButtonDown += OnDoubleClick;
		if (mTextBox != null) {
			mTextBox.KeyDown += OnKeyDown;
			mTextBox.LostFocus += (_, _) => IsInEditMode = false;
		}
	}

	private void OnDoubleClick(object sender, MouseButtonEventArgs e) {
		if (e.ClickCount == 2) {
			IsInEditMode = true;
			mTextBox?.Focus();
			mTextBox?.SelectAll();
			e.Handled = true;
		}
	}

	private void OnKeyDown(object sender, KeyEventArgs e) {
		if (e.Key == Key.Enter) {
			IsInEditMode = false;
			e.Handled = true;
		} else if(e.Key == Key.Escape) {
			if (DataContext != null && DataContext.GetType().GetProperty("Header") is var prop && prop != null) {
				Content = prop.GetValue(DataContext);
			}
			IsInEditMode = false;
			e.Handled = true;
		}
	}

	private static void OnIsInEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is EditableTextBlock editableTextBlock && (bool) e.NewValue) {
			editableTextBlock.Dispatcher.BeginInvoke(() => {
				editableTextBlock.mTextBox?.Focus();
				editableTextBlock.mTextBox?.SelectAll();
			});
		}
	}
}
