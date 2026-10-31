using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SCS_Mod_Helper.Class;

[TemplatePart(Name = "PART_ClearButton", Type = typeof(Button))]
class ClearableTextBox: TextBox {
	static ClearableTextBox() {
		DefaultStyleKeyProperty.OverrideMetadata(
			typeof(ClearableTextBox),
			new FrameworkPropertyMetadata(typeof(ClearableTextBox)));
	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		if (GetTemplateChild("PART_ClearButton") is Button clearButton) {
			clearButton.Click += (_, _) => {
				Text = string.Empty;
				Focus();
			};
		}
	}
}
