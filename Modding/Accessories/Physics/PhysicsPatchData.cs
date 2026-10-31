using SCS_Mod_Helper.Class;

namespace SCS_Mod_Helper.Modding.Accessories.Physics {
	public class PhysicsPatchData(string physicsName = ""): PhysicsData(physicsName) {
		//https://dariowouters.gitlab.io/scs-updates/unit/physics_patch_data/
		private string mMaterial = "";
		public string Material {
			get => mMaterial;
			set {
				mMaterial = value;
				InvokeChange();
			}
		}

		private float? mAreaDensity = null;
		public float? AreaDensity {
			get => mAreaDensity;
			set {
				mAreaDensity = value;
				InvokeChange();
			}
		}

		private string mAeroModelType = "AT_two_side_lift_drag";

		public string AeroModelType {
			get => mAeroModelType;
			set {
				mAeroModelType = value;
				InvokeChange();
			}
		}

		private float? mLinearStiffness = null;
		public float? LinearStiffness {
			get => mLinearStiffness;
			set {
				mLinearStiffness = value;
				InvokeChange();
			}
		}

		private float? mDragCoefficient = null;
		public float? DragCoefficient {
			get => mDragCoefficient;
			set {
				mDragCoefficient = value;
				InvokeChange();
			}
		}

		private float? mLiftCoefficient = null;
		public float? LiftCoefficient {
			get => mLiftCoefficient;
			set {
				mLiftCoefficient = value;
				InvokeChange();
			}
		}

		private Float2 mTCMinFirst = new(0f, 0f);
		public Float2 TCMinFirst {
			get => mTCMinFirst;
			set {
				mTCMinFirst = value;
				InvokeChange();
			}
		}

		private Float2 mTCMaxFirst = new(1f, 1f);
		public Float2 TCMaxFirst {
			get => mTCMaxFirst;
			set {
				mTCMaxFirst = value;
				InvokeChange();
			}
		}

		private Float2 mTCMinSecond = new(0f, 0f);
		public Float2 TCMinSecond {
			get => mTCMinSecond;
			set {
				mTCMinSecond = value;
				InvokeChange();
			}
		}

		private Float2 mTCMaxSecond = new(1f, 1f);
		public Float2 TCMaxSecond {
			get => mTCMaxSecond;
			set {
				mTCMaxSecond = value;
				InvokeChange();
			}
		}

		private uint? mXRes = null;
		public uint? XRes {
			get => mXRes;
			set {
				mXRes = value;
				InvokeChange();
			}
		}

		private uint? mYRes = null;
		public uint? YRes {
			get => mYRes;
			set {
				mYRes = value;
				InvokeChange();
			}
		}

		private float? mXSize = null;
		public float? XSize {
			get => mXSize;
			set {
				mXSize = value;
				InvokeChange();
			}
		}

		private float? mYSize = null;
		public float? YSize {
			get => mYSize;
			set {
				mYSize = value;
				InvokeChange();
			}
		}
	}
}
