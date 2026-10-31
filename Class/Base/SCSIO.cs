using SCS_Mod_Helper.Modding.PaintJob;
using System.IO;
using System.Windows.Media;

namespace SCS_Mod_Helper.Class.Base {
	public abstract class SCSIO() {

		protected const string FileHeader = "SiiNunit";
		protected int TabCount = 0;

		protected void WriteFileStructure(StreamWriter sw, Action action) {
			TabCount = 0;
			sw.WriteLine(FileHeader);
			BraceIn(sw);
			action();
			BraceOut(sw);
		}

		protected void WriteDataHeader(StreamWriter sw, string header, string name, Action data) {
			sw.Write(new string('\t', TabCount));
			sw.WriteLine($"{header} : {name}");
			BraceIn(sw);
			data();
			BraceOut(sw);
		}

		protected void BraceIn(StreamWriter sw) {//将括号下的内容缩进
			sw.Write(new string('\t', TabCount));
			sw.WriteLine('{');
			TabCount++;
		}
		protected void BraceOut(StreamWriter sw) {
			TabCount--;
			if (TabCount < 0)
				TabCount = 0;
			sw.Write(new string('\t', TabCount));
			sw.WriteLine('}');
		}

		protected abstract bool IsArray(string name);

		protected abstract bool HasQuote(string name);

		protected static void WriteEmptyLine(StreamWriter sw) => sw.WriteLine("");

		protected void WriteLine(StreamWriter sw, string name, string? value) {
			if (string.IsNullOrEmpty(value))
				return;
			Write(sw, name, value);
		}
		protected void WriteLine(StreamWriter sw, string name, string? value, string defaultValue) {
			if (value == defaultValue)
				return;
			WriteLine(sw, name, value);
		}

		protected void WriteLine(StreamWriter sw, string name, bool value) => Write(sw, name, value.ToString());

		protected void WriteLine(StreamWriter sw, string name, bool value, bool defaultValue) {
			if (value == defaultValue)
				return;
			WriteLine(sw, name, value);
		}

		protected void WriteLine(StreamWriter sw, string name, long? value) {
			if (value == null)
				return;
			Write(sw, name, FloatToString(value));
		}

		protected void WriteLine(StreamWriter sw, string name, long? value, long defaultValue) {
			if (value == defaultValue)
				return;
			WriteLine(sw, name, value);
		}

		protected void WriteLine(StreamWriter sw, string name, uint? value) {
			if (value == null)
				return;
			Write(sw, name, FloatToString(value));
		}

		protected void WriteLine(StreamWriter sw, string name, uint? value, uint defaultValue) {
			if (value == defaultValue)
				return;
			WriteLine(sw, name, value);
		}

		protected void WriteLine(StreamWriter sw, string name, float? value) {
			if (value == null)
				return;
			Write(sw, name, FloatToString(value));
		}

		protected void WriteLine(StreamWriter sw, string name, float? value, float defaultValue) {
			if (value == defaultValue)
				return;
			WriteLine(sw, name, value);
		}

		protected void WriteLine(StreamWriter sw, string name, float?[] floats) {
			if (floats == null)
				return;
			string value;

			switch (floats.Length) {
				case 2:
					if (floats[0] == null && floats[1] == null)
						return;
					value = $"({FloatToString(floats[0])},{FloatToString(floats[1])})";
					break;
				case 3:
					if (floats[0] == null && floats[1] == null && floats[2] == null)
						return;
					value = $"({FloatToString(floats[0])},{FloatToString(floats[1])},{FloatToString(floats[2])})";
					break;
				default:
					throw new ArgumentException("");
			}
			Write(sw, name, value);
		}
		protected void WriteLine(StreamWriter sw, string name, Floats floats) {
			if (floats == null)
				return;
			string value;
			switch (floats) {
				case Float2 float2:
					if (float2.F0 == null && float2.F1 == null)
						return;
					value = float2.ToSiiValue();
					break;
				case Float3 float3:
					if (float3.F0 == null && float3.F1 == null && float3.F2 == null)
						return;
					value = float3.ToSiiValue();
					break;
				default:
					throw new ArgumentException("");
			}
			Write(sw, name, value);
		}

		protected void WriteLine(StreamWriter sw, string name, float?[] floats, float[] defaultValue) {
			if (Array.Equals(floats, defaultValue))
				return;
			WriteLine(sw, name, floats);
		}

		protected void WriteLine(StreamWriter sw, string name, float[]? floats) {
			if (floats == null)
				return;
			string value = floats.Length switch {
				2 => $"({FloatToString(floats[0])},{FloatToString(floats[1])})",
				3 => $"({FloatToString(floats[0])},{FloatToString(floats[1])},{FloatToString(floats[2])})",
				_ => throw new ArgumentException(""),
			};
			Write(sw, name, value);
		}

		protected void WriteLine(StreamWriter sw, string name, float[]? floats, float[] defaultValue) {
			if (Equals(floats, defaultValue))
				return;
			WriteLine(sw, name, floats);
		}
		protected void WriteLine(StreamWriter sw, string name, Color? color) {
			if (color == null)
				return;
			float[] floats = ((Color)color).ToFloat3();
			string value = $"({FloatToString(floats[0])},{FloatToString(floats[1])},{FloatToString(floats[2])})";
			Write(sw, name, value);
		}

		protected void WriteLine(StreamWriter sw, string name, Color? color, Color defaultColor) {
			if (Equals(color, defaultColor))
				return;
			WriteLine(sw, name, color);
		}

		private void Write(StreamWriter sw, string name, string value) {
			var isArray = IsArray(name);
			var hasQuote = HasQuote(name);

			sw.Write(new string('\t', TabCount));
			sw.Write(name);
			if (isArray)
				sw.Write("[]");
			sw.Write(": ");
			if (hasQuote) {
				sw.WriteLine($"\"{value}\"");
			} else
				sw.WriteLine(value);
		}

		protected static string FloatToString(float? f) => f?.ToString("0.0#####") ?? "0.0";

		protected static string ClipValue(string value) {
			if (value.Contains('#'))//部分行会有注释
				value = value[..value.IndexOf('#')];
			if (value.Contains('"')) {//string 类数值会有双引号
				value = value[(value.IndexOf('"') + 1)..value.LastIndexOf('"')];
			} else
				value = value.Trim();
			if (value.EndsWith('{'))
				value = value[..^1].Trim();
			return value;
		}

		protected static void FloatParse(string value, Floats floats) {
			value = value[1..^1];
			var split = value.Split(',');
			for (int i = 0; i < split.Length; i++) {
				if (split[i].EndsWith('f'))
					split[i] = split[i][..^1];
				float f;
				try { f = float.Parse(split[i]); } catch { f = 0.0f; }
				floats[i] = f;
			}
		}
	}
}
