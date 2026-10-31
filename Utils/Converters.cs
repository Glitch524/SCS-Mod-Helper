using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SCS_Mod_Helper.Utils {
	public class FloatConverter: IValueConverter {
		public static FloatConverter Instance { get; } = new();
		public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
			if (value is float f) {
				return f.ToString("0.0#####");
			}
			return string.Empty;
		}
		public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null) 
				return targetType == typeof(float?) ? (float?)null : 0f;
			if (value is string text) {
				if (string.IsNullOrWhiteSpace(text)) {
					return targetType == typeof(float?) ? (float?)null : 0f;
				}
				if (float.TryParse(text,
					   NumberStyles.Float | NumberStyles.AllowThousands,
					   culture,
					   out float result)) {
					return result;
				}
			}
			return DependencyProperty.UnsetValue;
		}
	}

	public class UIntConverter: IValueConverter {
		public static UIntConverter Instance { get; } = new();
		public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
			if (value is uint i) {
				return i.ToString();
			}
			return "";
		}

		public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			string text = (string)value;
			try {
				return uint.Parse(text);
			} catch {
				return null;
			}
		}
	}
	public class LongConverter: IValueConverter {
		public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
			if (value is long i) {
				return i.ToString();
			}
			return "";
		}

		public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			string text = (string)value;
			try {
				return long.Parse(text);
			} catch {
				return null;
			}
		}
	}
}
