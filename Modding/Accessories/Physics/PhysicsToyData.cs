using SCS_Mod_Helper.Class;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace SCS_Mod_Helper.Modding.Accessories.Physics; 
public class PhysicsToyData(string physicsName = ""): PhysicsData(physicsName) {
	//https://dariowouters.gitlab.io/scs-updates/unit/physics_toy_data/
	private string mModelPath = "";
	public string ModelPath {
		get => mModelPath;
		set {
			mModelPath = value;
			InvokeChange(nameof(ModelPath));

			LoadLooksAndVariants();
		}
	}

	private string mCollPath = "";
	public string CollPath {
		get => mCollPath;
		set {
			mCollPath = value;
			InvokeChange(nameof(CollPath));
		}
	}

	private string mLook = "default";
	public string Look {
		get => mLook;
		set {
			mLook = value;
			InvokeChange(nameof(Look));
		}
	}

	private string mVariant = "default";
	public string Variant {
		get => mVariant;
		set {
			mVariant = value;
			InvokeChange(nameof(Variant));
		}
	}

	private ObservableCollection<string>? mLookList = null;
	private ObservableCollection<string>? mVariantList = null;
	public ObservableCollection<string> LookList {
		get {
			if (mLookList == null)
				LoadLooksAndVariants();
			return mLookList!;
		}
	}
	public ObservableCollection<string> VariantList {
		get {
			if (mVariantList == null)
				LoadLooksAndVariants();
			return mVariantList!;
		}
	}

	public void LoadLooksAndVariants() {
		string oldLook = Look, oldVariant = Variant;
		if (mLookList == null)
			mLookList = [];
		else
			mLookList.Clear();
		if (mVariantList == null)
			mVariantList = [];
		else
			mVariantList.Clear();
		var path = mModelPath;
		if (path.Length >= 5 && !path.EndsWith('.')) {
			path = path.Replace('/', '\\');
			path = path[..^4] + ".pit";
			path = Instances.ProjectLocation + path;

			static void setValue(ObservableCollection<string> list, string oldValue, Action<string> set) {
				if (list.Count > 0) {
					if (list.Contains(oldValue)) {
						set(oldValue);
					} else
						set(list[0]);
				}
			}
			AccSCSIO.ReadLookAndVariant(path, mLookList, mVariantList);
			setValue(mLookList, oldLook, (set) => Look = set);
			setValue(mVariantList, oldVariant, (set) => Variant = set);
		} else {
			mLookList.Add("default");
			mVariantList.Add("default");
		}
	}

	private string mToyType = "TT_rope";
	public string ToyType {
		get => mToyType;
		set {
			mToyType = value;
			InvokeChange(nameof(ToyType));

			InvokeChange(nameof(AngularVisibility));
			InvokeChange(nameof(RopeDataVisibility));
		}
	}
	public Visibility AngularVisibility =>
		ToyType == "TT_joint" || ToyType == "TT_joint_free" ? Visibility.Visible : Visibility.Collapsed;
	public Visibility RopeDataVisibility =>
		ToyType == "TT_rope" || ToyType == "TT_double_rope" ? Visibility.Visible : Visibility.Hidden;

	private float? mMass = null;
	public float? Mass {
		get => mMass;
		set {
			mMass = value;
			InvokeChange(nameof(Mass));
		}
	}

	private Float3 mCogOffset = new();
	public Float3 CogOffset {
		get => mCogOffset;
		set {
			mCogOffset = value;
			InvokeChange(nameof(CogOffset));
		}
	}

	private float? mLinearStiffness = null;
	public float? LinearStiffness {
		get => mLinearStiffness;
		set {
			mLinearStiffness = value;
			InvokeChange(nameof(LinearStiffness));
		}
	}

	private float? mLinearDamping = null;
	public float? LinearDamping {
		get => mLinearDamping;
		set {
			mLinearDamping = value;
			InvokeChange(nameof(LinearDamping));
		}
	}

	private Float3 mLocatorHookOffset = new();
	public Float3 LocatorHookOffset {
		get => mLocatorHookOffset;
		set {
			mLocatorHookOffset = value;
			InvokeChange(nameof(LocatorHookOffset));
		}
	}

	private Float3 mRestPositionOffset = new();
	public Float3 RestPositionOffset {
		get => mRestPositionOffset;
		set {
			mRestPositionOffset = value;
			InvokeChange(nameof(RestPositionOffset));
		}
	}

	private Float3 mRestRotationOffset = new();
	public Float3 RestRotationOffset {
		get => mRestRotationOffset;
		set {
			mRestRotationOffset = value;
			InvokeChange(nameof(RestRotationOffset));
		}
	}

	private ObservableCollection<InstanceOffset> mInstanceOffsetList = [];
	public ObservableCollection<InstanceOffset> InstanceOffsetList {
		get => mInstanceOffsetList;
		set {
			mInstanceOffsetList = value;
			InvokeChange();
		}
	}

	private InstanceOffset? mCurrentInstanceOffset = null;
	public InstanceOffset? CurrentInstanceOffset {
		get => mCurrentInstanceOffset;
		set {
			mCurrentInstanceOffset = value;
			InvokeChange();
		}
	}

	public ICommand AddInstanceOffsetCommand => new RelayCommand<object>(AddInstanceOffset);
	private void AddInstanceOffset(object? parameter) {
		InstanceOffset io = new();
		InstanceOffsetList.Add(io);
		CurrentInstanceOffset = io;
		Debug.WriteLine("add");
	}
	public ICommand RemoveInstanceOffsetCommand => new RelayCommand<InstanceOffset?>(RemoveInstanceOffset);
	private void RemoveInstanceOffset(InstanceOffset? parameter) {
		if (parameter == null)
			return;
		InstanceOffsetList.Remove(parameter);
	}


	private string mRopeMaterial = "";
	public string RopeMaterial {
		get => mRopeMaterial;
		set {
			mRopeMaterial = value;
			InvokeChange(nameof(RopeMaterial));
		}
	}

	private Float2 mAngularStiffness = new();
	public Float2 AngularStiffness {
		get => mAngularStiffness;
		set {
			mAngularStiffness = value;
			InvokeChange(nameof(AngularStiffness));
		}
	}

	private Float2 mAngularDamping = new();
	public Float2 AngularDamping {
		get => mAngularDamping;
		set {
			mAngularDamping = value;
			InvokeChange(nameof(AngularDamping));
		}
	}

	private Float3 mAngularAmplitude = new();
	public Float3 AngularAmplitude {
		get => mAngularAmplitude;
		set {
			mAngularAmplitude = value;
			InvokeChange(nameof(AngularAmplitude));
		}
	}

	private float? mRopeWidth = null;
	public float? RopeWidth {
		get => mRopeWidth;
		set {
			mRopeWidth = value;
			InvokeChange(nameof(RopeWidth));
		}
	}
	private float? mRopeLenght = null;
	public float? RopeLength {
		get => mRopeLenght;
		set {
			mRopeLenght = value;
			InvokeChange(nameof(RopeLength));
		}
	}
	private float? mRopeHookOffset = null;
	public float? RopeHookOffset {
		get => mRopeHookOffset;
		set {
			mRopeHookOffset = value;
			InvokeChange(nameof(RopeHookOffset));
		}
	}
	private float? mRopeToyOffset = null;
	public float? RopeToyOffset {
		get => mRopeToyOffset;
		set {
			mRopeToyOffset = value;
			InvokeChange(nameof(RopeToyOffset));
		}
	}
	private uint? mRopeResolution = null;
	public uint? RopeResolution {
		get => mRopeResolution;
		set {
			mRopeResolution = value;
			InvokeChange(nameof(RopeResolution));
		}
	}
	private uint? mPositionIterations = null;
	public uint? PositionIterations {
		get => mPositionIterations;
		set {
			mPositionIterations = value;
			InvokeChange(nameof(PositionIterations));
		}
	}
	private float? mRopeLinearDensity = null;
	public float? RopeLinearDensity {
		get => mRopeLinearDensity;
		set {
			mRopeLinearDensity = value;
			InvokeChange(nameof(RopeLinearDensity));
		}
	}

	private float? mNodeDamping = null;
	public float? NodeDamping {
		get => mNodeDamping;
		set {
			mNodeDamping = value;
			InvokeChange(nameof(mNodeDamping));
		}
	}
}

public class InstanceOffset(float? f0, float? f1, float? f2) : Float3(f0, f1, f2) {

	public InstanceOffset(): this(0f, 0f, 0f) {

	}

	public float? Offset0 {
		get => F0;
		set {
			F0 = value;
			InvokeChange();
		}
	}
	public float? Offset1 {
		get => F1;
		set {
			F1 = value;
			InvokeChange();

		}
	}
	public float? Offset2 {
		get => F2;
		set {
			F2 = value;
			InvokeChange();
		}
	}

	public override string ToString() => $"InstanceOffset=({F0},{F1},{F2})";
}