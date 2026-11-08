using SCS_Mod_Helper.Class.Base;

namespace SCS_Mod_Helper.Class;

public abstract class Floats(int length): BaseBinding {
	public int mLength = length;
	public int Length => mLength;

	public abstract string ToSiiValue();

	public abstract string ToAppValue();

	protected static string? FloatToString(float? f) => f?.ToString("0.0#####");


	public float? this[int index] {
		get => GetValue(index);
		set {
			SetValue(index, value);
			InvokeChange();
		}
	}

	protected abstract float? GetValue(int index);
	protected abstract void SetValue(int index, float? value);
}

public class Float2(float? f0, float? f1): Floats(2) {
	public Float2() : this(null, null) {

	}
	private float? mF0 = f0;
	public float? F0 {
		get => mF0;
		set {
			mF0 = value;
			InvokeChange();
		}
	}
	public string? F0String {
		get => FloatToString(F0);
		set {
			mF0 = string.IsNullOrEmpty(value) ? null : float.Parse(value);
			InvokeChange();
		}
	}

	private float? mF1 = f1;
	public float? F1 {
		get => mF1;
		set {
			mF1 = value;
			InvokeChange();
		}
	}
	public string? F1String {
		get => FloatToString(F1);
		set {
			mF1 = string.IsNullOrEmpty(value) ? null : float.Parse(value);
			InvokeChange();
		}
	}

	protected override float? GetValue(int index) => index switch {
		0 => mF0,
		1 => mF1,
		_ => throw new IndexOutOfRangeException(nameof(index))
	};

	protected override void SetValue(int index, float? value) {
		switch (index) {
			case 0:
				mF0 = value;
				break;
			case 1:
				mF1 = value;
				break;
			default:
				throw new IndexOutOfRangeException(nameof(index));
		}
	}

	public override string ToSiiValue() => $"({FloatToString(F0)},{FloatToString(F1)})";

	public override string ToAppValue() => $"{F0},{F1}";

	public bool IsEqual(float? f0, float? f1) => F0 == f0 && F1 == f1;
}

public class Float3(float? f0, float? f1, float? f2): Floats(3) {
	public Float3() : this(null, null, null) {

	}
	private float? mF0 = f0;
	public float? F0 {
		get => mF0;
		set {
			mF0 = value;
			InvokeChange();
		}
	}
	public string? F0String {
		get => FloatToString(F0);
		set {
			mF0 = string.IsNullOrEmpty(value) ? null : float.Parse(value);
			InvokeChange();
		}
	}

	private float? mF1 = f1;
	public float? F1 {
		get => mF1;
		set {
			mF1 = value;
			InvokeChange();
		}
	}
	public string? F1String {
		get => FloatToString(F1);
		set {
			mF1 = string.IsNullOrEmpty(value) ? null : float.Parse(value);
			InvokeChange();
		}
	}
	private float? mF2 = f2;
	public float? F2 {
		get => mF2;
		set {
			mF2 = value;
			InvokeChange();
		}
	}
	public string? F2String {
		get => FloatToString(F2);
		set {
			mF2 = string.IsNullOrEmpty(value) ? null : float.Parse(value);
			InvokeChange();
		}
	}

	protected override float? GetValue(int index) => index switch {
		0 => mF0,
		1 => mF1,
		2 => mF2,
		_ => throw new IndexOutOfRangeException(nameof(index))
	};

	protected override void SetValue(int index, float? value) {
		switch (index) {
			case 0:
				mF0 = value;
				break;
			case 1:
				mF1 = value;
				break;
			case 2:
				mF2 = value;
				break;
			default:
				throw new IndexOutOfRangeException(nameof(index));
		}
	}

	public override string ToSiiValue() => $"({FloatToString(F0)},{FloatToString(F1)},{FloatToString(F2)})";

	public override string ToAppValue() => $"{F0},{F1},{F2}";
	public bool IsEqual(float? f0, float? f1, float? f2) => F0 == f0 && F1 == f1 && F2 == f2;
}
