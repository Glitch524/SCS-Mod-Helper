using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SCS_Mod_Helper.Class; 
internal class EditableTextBlock: Control {
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
		new FrameworkPropertyMetadata(false, OnPropertyChanged));

	public string? Text {
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
		nameof(Text),
		typeof(string),
		typeof(EditableTextBlock),
		new FrameworkPropertyMetadata(null,
								FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
								OnPropertyChanged));

	private TextBlock? mTextBlock;
	private TextBox? mTextBox;

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		mTextBlock = GetTemplateChild("PART_TextBlock") as TextBlock;
		mTextBox = GetTemplateChild("PART_TextBox") as TextBox;

		Binding TextBinding = new() {
			Source = this,
			Path = new PropertyPath(nameof(Text)),
			Mode = BindingMode.TwoWay,
			UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
		};
		mTextBlock?.SetBinding(TextBlock.TextProperty, TextBinding);
		mTextBox?.SetBinding(TextBox.TextProperty, TextBinding);

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
				Text = (string?) prop.GetValue(DataContext);
			}
			IsInEditMode = false;
			e.Handled = true;
		}
	}

	private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is EditableTextBlock editableTextBlock) {
			switch(e.Property.Name) {
				case nameof(Text):
					editableTextBlock.Text = (string) e.NewValue;
					break;
				case nameof(IsInEditMode):
					if ((bool)e.NewValue) {
						editableTextBlock.Dispatcher.BeginInvoke(() => {
							editableTextBlock.mTextBox?.Focus();
							editableTextBlock.mTextBox?.SelectAll();
						});
					}
					break;
			}
		}
	}
}
