using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Vehicles;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Modding.PaintJob {
	public class PaintJobOverrideData(int index): BaseBinding {
		//https://dariowouters.gitlab.io/scs-updates/unit/simple_paint_job_data/

		public override string ToString() => OvrName;

		private int mIndex = index;
		public int Index {
			get => mIndex;
			set {
				mIndex = value;
				InvokeChange();
				InvokeChange(nameof(OvrName));
			}
		}

		public string OvrName => ".ovr" + Index;

		private string mAccTex = "";
		public string AccTex {
			get => mAccTex;
			set {
				mAccTex = value;
				InvokeChange();
				InvokeChange(nameof(AccTexImage));
			}
		}

		private string? LoadedAccTex = null;
		protected BitmapSource? mAccTextureImage = null;
		public BitmapSource? AccTexImage {
			get => mAccTex == LoadedAccTex ? mAccTextureImage : null;
			set {
				mAccTextureImage?.Freeze();
				mAccTextureImage = value;
				LoadedAccTex = mAccTex;
				InvokeChange();
			}
		}

		private float? mAccFlakeUVScale = 32f;

		public float? AccFlakeUVScale {
			get => mAccFlakeUVScale;
			set {
				mAccFlakeUVScale = value;
				InvokeChange();
			}
		}

		private float mAccFlakeVRatio = 1f;
		public float AccFlakeVRatio {
			get => mAccFlakeVRatio;
			set {
				mAccFlakeVRatio = value;
				InvokeChange();
			}
		}

		private readonly ObservableCollection<Accessory> mAccList = [];

		public ObservableCollection<Accessory> AccList => mAccList;

		public void RemoveAccList() {
			foreach (var acc in AccList) {
				acc.BelongingOverride = null;
			}
		}
	}
}
