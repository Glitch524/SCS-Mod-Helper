using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Windows;
using System.Windows.Media;

namespace SCS_Mod_Helper.Modding.Hookups;

public class HookupBinding(Func<IBaseVisual> visualFunc): WindowBinding(visualFunc) {

	public string MProjectLocation = "";
	public string ProjectLocation {
		get {
			return MProjectLocation;
		}
		set {
			MProjectLocation = value;
			InvokeChange(nameof(ProjectLocationShow));
		}
	}
	public string ProjectLocationShow => GetString("TextProjectLocation") + MProjectLocation;

	private bool mIsAnimHookup = true;
	public bool IsAnimHookup {
		get => mIsAnimHookup;
		set {
			mIsAnimHookup = value;
			InvokeChange();

			InvokeChange(nameof(AnimVisibility));
			InvokeChange(nameof(StartClickable));
		}
	}

	private string mHookupName = "";
	public string HookupName {
		get => mHookupName;
		set {
			mHookupName = value;
			InvokeChange();

			InvokeChange(nameof(StartClickable));
			InvokeChange(nameof(HookupNameForeground));
		}
	}

	public SolidColorBrush HookupNameForeground => new(HookupName.Length > 12 ? Colors.Red : Colors.Black);

	private string mModelLocation = "";
	public string ModelLocation {
		get => mModelLocation;
		set {
			mModelLocation = value;
			InvokeChange();

			InvokeChange(nameof(StartClickable));
		}
	}

	private string mAnimationLocation = "";
	public string AnimationLocation {
		get => mAnimationLocation;
		set {
			mAnimationLocation = value;
			InvokeChange();

			InvokeChange(nameof(StartClickable));
		}
	}

	public Visibility AnimVisibility => IsAnimHookup ? Visibility.Visible : Visibility.Collapsed;

	public bool StartClickable {
		get => HookupName.Length > 0 && ModelLocation.Length > 0 && (!IsAnimHookup || AnimationLocation.Length > 0);
	}
}
