using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCS_Mod_Helper.Utils; 
public static partial class TextControl {
	public static void NumberOnly(object sender, TextCompositionEventArgs e) => e.Handled = RegexNumber().IsMatch(e.Text);

	[GeneratedRegex("[^0-9]+")]
	private static partial Regex RegexNumber();
	public static void FloatOnly(object sender, TextCompositionEventArgs e) {
		var currentText = ((TextBox)sender).Text;
		string input = e.Text;
		if (input.Length == 0) {
			e.Handled = false;
			return;
		}
		var match = RegexFloat().IsMatch(e.Text);
		if (match) {
			e.Handled = true;
			return;
		}
		if (input[0] == '.') {
			if (currentText.Length == 0 || currentText.Contains('.')) {
				e.Handled = true;
				return;
			}
		}
		e.Handled = false;
	}

	[GeneratedRegex("[^0-9.-]+")]
	private static partial Regex RegexFloat();
}
