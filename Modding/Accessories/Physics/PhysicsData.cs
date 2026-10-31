using SCS_Mod_Helper.Class.Base;

namespace SCS_Mod_Helper.Modding.Accessories.Physics;
public class PhysicsData: BaseBinding {

	protected PhysicsData(string physicsName) {
		mPhysicsName = physicsName;
	}

	private string mPhysicsName;

	public string PhysicsName {
		get => mPhysicsName;
		set {
			mPhysicsName = value;
			InvokeChange();
		}
	}
}
