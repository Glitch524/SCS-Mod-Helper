using SCS_Mod_Helper.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCS_Mod_Helper.Class;

[TemplatePart(Name = "TextBox0", Type = typeof(TextBox))]
[TemplatePart(Name = "TextBox1", Type = typeof(TextBox))]
[TemplatePart(Name = "TextBox2", Type = typeof(TextBox))]
public class Float3Box: Control {
	static Float3Box() {
		DefaultStyleKeyProperty.OverrideMetadata(
			typeof(Float3Box),
			new FrameworkPropertyMetadata(typeof(Float3Box)));
	}

	public Floats Floats {
		get => (Floats)GetValue(FloatsProperty);
		set => SetValue(FloatsProperty, value);
	}

	public static readonly DependencyProperty FloatsProperty = DependencyProperty.Register(
		nameof(Floats),
		typeof(Floats),
		typeof(Float3Box),
		new PropertyMetadata(null, OnValuesChanged));

	public int Length {
		get => (int)GetValue(LengthProperty);
		set => SetValue(LengthProperty, value);
	}

	public static readonly DependencyProperty LengthProperty = DependencyProperty.Register(
		nameof(Length),
		typeof(int),
		typeof(Float3Box),
		new PropertyMetadata(3, OnValuesChanged));


	public float? DefaultValue0 {
		get => (float?)GetValue(DefaultValue0Property);
		set => SetValue(DefaultValue1Property, value);
	}
	public static readonly DependencyProperty DefaultValue0Property = DependencyProperty.Register(
		nameof(DefaultValue0),
		typeof(float?),
		typeof(Float3Box),
		new PropertyMetadata(null, OnValuesChanged));

	public float? DefaultValue1 {
		get => (float?)GetValue(DefaultValue1Property);
		set => SetValue(DefaultValue1Property, value);
	}
	public static readonly DependencyProperty DefaultValue1Property = DependencyProperty.Register(
		nameof(DefaultValue1),
		typeof(float?),
		typeof(Float3Box),
		new PropertyMetadata(null, OnValuesChanged));

	public float? DefaultValue2 {
		get => (float?)GetValue(DefaultValue2Property);
		set => SetValue(DefaultValue2Property, value);
	}
	public static readonly DependencyProperty DefaultValue2Property = DependencyProperty.Register(
		nameof(DefaultValue2),
		typeof(float?),
		typeof(Float3Box),
		new PropertyMetadata(null, OnValuesChanged));


	private Grid? GridMain;
	private ColumnDefinition? Col3, Col4;
	private TextBox? TextBox0;
	private TextBox? TextBox1;
	private TextBox? TextBox2;


	public override void OnApplyTemplate() {
		GridMain = GetTemplateChild(nameof(GridMain)) as Grid;

		Col3 = GetTemplateChild(nameof(Col3)) as ColumnDefinition;
		Col4 = GetTemplateChild(nameof(Col4)) as ColumnDefinition;
		//SetColWidth(Length);


		TextBox0 = GridMain?.FindName(nameof(TextBox0)) as TextBox;
		TextBox1 = GridMain?.FindName(nameof(TextBox1)) as TextBox;
		TextBox2 = GridMain?.FindName(nameof(TextBox2)) as TextBox;

		TextBox0?.PreviewTextInput += FloatOnly;
		TextBox1?.PreviewTextInput += FloatOnly;
		TextBox2?.PreviewTextInput += FloatOnly;
	}
	private void FloatOnly(object sender, TextCompositionEventArgs e) => TextControl.FloatOnly(sender, e);

	private void PART_First_TextChanged(object sender, TextChangedEventArgs e) {
	}
	private void OnValuesChanged(string propertyName, object newValue) {
		switch (propertyName) {
			case nameof(Floats):
				GridMain?.DataContext = newValue;
				break;
			case nameof(Length):
				//SetColWidth(Length);
				break;
		}
	}

	private static void OnValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is Float3Box box)
			box.OnValuesChanged(e.Property.Name, e.NewValue);
	}
}