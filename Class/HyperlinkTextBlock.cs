using System.Windows;
using System.Windows.Documents;
using Wpf.Ui.Controls;

namespace SCS_Mod_Helper.Class; 
public class HyperlinkTextBlock: TextBlock {
	public static readonly RoutedEvent HyperlinkClickEvent = 
		EventManager.RegisterRoutedEvent(
			"HyperlinkClick", 
			RoutingStrategy.Bubble, 
			typeof(RoutedEventHandler), 
			typeof(HyperlinkTextBlock));

	public event RoutedEventHandler HyperlinkClick {
		add => AddHandler(HyperlinkClickEvent, value);
		remove => RemoveHandler(HyperlinkClickEvent, value);
	}

	public static readonly DependencyProperty FormattedTextProperty = 
		DependencyProperty.Register(
			nameof(FormattedText),
			typeof(string),
			typeof(HyperlinkTextBlock),
			new PropertyMetadata(string.Empty, OnFormattedTextChanged));

	public string FormattedText {
		get => (string)GetValue(FormattedTextProperty);
		set => SetValue(FormattedTextProperty, value);
	}

	private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is HyperlinkTextBlock hyperlinkTextBlock) {
			hyperlinkTextBlock.Parse(e.NewValue as string);
		}
	}

	private void Parse(string? text) {
		Inlines.Clear();
		if (string.IsNullOrEmpty(text)) {
			return;
		}
		var parts = text.Split(["[link]", "[/link]"], StringSplitOptions.None);
		for (int i = 0; i < parts.Length; i++) {
			if (i % 2 == 0) {
				Inlines.Add(parts[i]);
			} else {
				var hyperlink = new Hyperlink(new Run(parts[i])) {
					Tag = parts[i]
				};
				hyperlink.Click += (sender, e) => RaiseEvent(new RoutedEventArgs(HyperlinkClickEvent, this));
				Inlines.Add(hyperlink);
			}
		}
	}
}
