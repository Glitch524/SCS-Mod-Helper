using SCS_Mod_Helper.Class;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace SCS_Mod_Helper.Modding.Accessories.Physics {
	internal class AccessoryPhysicsIO: AppIO {
		private const string TITLE_PHYSICS = "Physics";
		private const string ATTR_PHYS_NAME = "PhysName";

		private const string TITLE_PHYS_TOY_DATA = "PhysicsToyData";
		private const string ATTR_PHYS_MODEL = "PhysModel";
		private const string ATTR_PHYS_COLL = "PhysModelColl";
		private const string ATTR_PHYS_LOOK = "PhysModelLook";
		private const string ATTR_PHYS_Variant = "PhysModelVariant";
		private const string ATTR_TOY_TYPE = "ToyType";
		private const string ATTR_TOY_MASS = "ToyMass";
		private const string ATTR_TOY_COG_OFFSET = "ToyCogOffset";
		private const string ATTR_LINEAR_STIFFNESS = "LinearStiffness";
		private const string ATTR_LINEAR_DAMPING = "LinearDamping";
		private const string ATTR_ANGULAR_STIFFNESS = "AngularStiffness";
		private const string ATTR_ANGULAR_DAMPING = "AngularDamping";
		private const string ATTR_ANGULAR_AMPLITUDE = "AngularAmplitude";
		private const string ATTR_NODE_DAMPING = "NodeDamping";
		private const string ATTR_LOCATOR_HOOK_OFFSET = "LocatorHookOffset";
		private const string ATTR_REST_POSITION_OFFSET = "RestPositionOffset";
		private const string ATTR_REST_ROTATION_OFFSET = "RestRotationOffset";
		private const string ATTR_INSTANCE_OFFSET_LIST = "InstanceOffsetList";
		private const string ATTR_INSTANCE_OFFSET = "InstanceOffset";
		private const string ATTR_ROPE_WIDTH = "RopeWidth";
		private const string ATTR_ROPE_LENGTH = "RopeLength";
		private const string ATTR_ROPE_HOOK_OFFSET = "RopeHookOffset";
		private const string ATTR_ROPE_TOY_OFFSET = "RopeToyOffset";
		private const string ATTR_ROPE_RESOLUTION = "RopeResolution";
		private const string ATTR_ROPE_LINEAR_DENSITY = "RopeLinearDensity";
		private const string ATTR_POSITION_ITERATIONS = "PositionIterations";
		private const string ATTR_ROPE_MATERIAL = "RopeMaterial";



		private const string TITLE_PHYS_PATCH_DATA = "PhysicsPatchData";

		public const string TITLE_MATERIAL = "Material";
		public const string TITLE_AREA_DENSITY = "AreaDensity";
		public const string TITLE_AERO_MODEL_TYPE = "AeroModelType";
		public const string TITLE_TC_MIN_FIRST = "TcMinFirst";
		public const string TITLE_TC_MAX_FIRST = "TcMaxFirst";
		public const string TITLE_TC_MIN_SECOND = "TcMinSecond";
		public const string TITLE_TC_MAX_SECOND = "TcMaxSecond";
		public const string TITLE_X_RES = "XRes";
		public const string TITLE_Y_RES = "YRes";
		public const string TITLE_X_SIZE = "XSize";
		public const string TITLE_Y_SIZE = "YSize";
		public const string TITLE_LINEAR_STIFFNESS = "LinearStiffness";
		public const string TITLE_DRAG_COEFFICIENT = "DragCoefficient";
		public const string TITLE_LIFT_COEFFICIENT = "LiftCoefficient";


		private static ObservableCollection<PhysicsData>? mPhysicsItems;
		public static ObservableCollection<PhysicsData> PhysicsItems {
			get {
				if (mPhysicsItems == null) 
					new AccessoryPhysicsIO().LoadPhysics();
				return mPhysicsItems!;
			}
		}
		public static void AddPhysicsToList(PhysicsData? physics) {
			if (physics == null) 
				return;
			PhysicsItems.Add(physics);
		}

		public void SavePhysics() {
			if (mPhysicsItems == null)
				return;
			WriteXmlDeclaration();
			SkipCreatingIFEmpty = true;
			WriteElement(TITLE_PHYSICS, () => {
				foreach (var phys in PhysicsItems) {
					if (phys is PhysicsToyData toyData) {
						WritePhysicsToyData(toyData);
					} else if (phys is PhysicsPatchData patchData) {
						WritePhysicsPatchData(patchData);
					}
				}
			});
			SaveDocument(Paths.SavedPhysicsFile());
		}
		private void WritePhysicsToyData(PhysicsToyData toyData) {
			WriteElement(TITLE_PHYS_TOY_DATA, () => {
				WriteAttribute(ATTR_PHYS_NAME, toyData.PhysicsName);

				WriteValueElement(ATTR_PHYS_MODEL, toyData.ModelPath);

				WriteValueElement(ATTR_PHYS_COLL, toyData.CollPath);
				WriteValueElement(ATTR_PHYS_LOOK, toyData.Look);
				WriteValueElement(ATTR_PHYS_Variant, toyData.Variant);

				WriteValueElement(ATTR_TOY_TYPE, toyData.ToyType);
				WriteValueElement(ATTR_TOY_MASS, toyData.Mass);
				WriteValueElement(ATTR_TOY_COG_OFFSET, toyData.CogOffset);
				WriteValueElement(ATTR_LINEAR_STIFFNESS, toyData.LinearStiffness);
				WriteValueElement(ATTR_LINEAR_DAMPING, toyData.LinearDamping);
				WriteValueElement(ATTR_LOCATOR_HOOK_OFFSET, toyData.LocatorHookOffset);
				WriteValueElement(ATTR_REST_POSITION_OFFSET, toyData.RestPositionOffset);
				WriteValueElement(ATTR_REST_ROTATION_OFFSET, toyData.RestRotationOffset);
				if (toyData.InstanceOffsetList.Count > 0) {
					WriteElement(ATTR_INSTANCE_OFFSET_LIST, () => {
						foreach (var offset in toyData.InstanceOffsetList) {
							WriteValueElement(ATTR_INSTANCE_OFFSET, offset);
						}
					});
				}
				WriteValueElement(ATTR_ROPE_MATERIAL, toyData.RopeMaterial);

				WriteValueElement(ATTR_ANGULAR_STIFFNESS, toyData.AngularStiffness);
				WriteValueElement(ATTR_ANGULAR_DAMPING, toyData.AngularDamping);
				WriteValueElement(ATTR_ANGULAR_AMPLITUDE, toyData.AngularAmplitude);

				WriteValueElement(ATTR_ROPE_WIDTH, toyData.RopeWidth);
				WriteValueElement(ATTR_ROPE_LENGTH, toyData.RopeLength);
				WriteValueElement(ATTR_ROPE_HOOK_OFFSET, toyData.RopeHookOffset);
				WriteValueElement(ATTR_ROPE_TOY_OFFSET, toyData.RopeToyOffset);
				WriteValueElement(ATTR_ROPE_RESOLUTION, toyData.RopeResolution);
				WriteValueElement(ATTR_POSITION_ITERATIONS, toyData.PositionIterations);
				WriteValueElement(ATTR_ROPE_LINEAR_DENSITY, toyData.RopeLinearDensity);
				WriteValueElement(ATTR_NODE_DAMPING, toyData.NodeDamping);
			});
		}

		private void WritePhysicsPatchData(PhysicsPatchData patchData) {
			WriteElement(TITLE_PHYS_PATCH_DATA, () => {
				WriteAttribute(ATTR_PHYS_NAME, patchData.PhysicsName);

				WriteValueElement(TITLE_MATERIAL, patchData.Material);
				WriteValueElement(TITLE_AREA_DENSITY, patchData.AreaDensity);
				WriteValueElement(TITLE_AERO_MODEL_TYPE, patchData.AeroModelType);
				WriteValueElement(TITLE_TC_MIN_FIRST, patchData.TCMinFirst);
				WriteValueElement(TITLE_TC_MAX_FIRST, patchData.TCMaxFirst);
				WriteValueElement(TITLE_TC_MIN_SECOND, patchData.TCMinSecond);
				WriteValueElement(TITLE_TC_MAX_SECOND, patchData.TCMaxSecond);
				WriteValueElement(TITLE_X_RES, patchData.XRes);
				WriteValueElement(TITLE_Y_RES, patchData.YRes);
				WriteValueElement(TITLE_X_SIZE, patchData.XSize);
				WriteValueElement(TITLE_Y_SIZE, patchData.YSize);
				WriteValueElement(TITLE_LINEAR_STIFFNESS, patchData.LinearStiffness);
				WriteValueElement(TITLE_DRAG_COEFFICIENT, patchData.DragCoefficient);
				WriteValueElement(TITLE_LIFT_COEFFICIENT, patchData.LiftCoefficient);
			});
		}

		public void LoadPhysics() {
			string file = Paths.SavedPhysicsFile();
			mPhysicsItems = [];
			if (!File.Exists(file)) 
				return;
			doc.Load(Paths.SavedPhysicsFile());
			XmlNode? physicsNode = doc.SelectSingleNode(TITLE_PHYSICS);
			if (physicsNode == null)
				return;
			foreach (XmlNode child in physicsNode.ChildNodes) {
				switch (child.Name) {
					case TITLE_PHYS_TOY_DATA:
						LoadPhysicsToyData(child);
						break;
					case TITLE_PHYS_PATCH_DATA:
						LoadPhysicsPatchData(child);
						break;
				}
			}
		}

		private static void LoadPhysicsToyData(XmlNode node) {
			string name = GetAttribute(node, ATTR_PHYS_NAME);
			PhysicsToyData toyData = new(name);
			foreach (XmlNode child in node.ChildNodes) {
				try {
					switch (child.Name) {
						case ATTR_PHYS_MODEL:
							toyData.ModelPath = child.InnerText;
							break;
						case ATTR_PHYS_COLL:
							toyData.CollPath = child.InnerText;
							break;
						case ATTR_PHYS_LOOK:
							toyData.Look = child.InnerText;
							break;
						case ATTR_PHYS_Variant:
							toyData.Variant = child.InnerText;
							break;
						case ATTR_TOY_TYPE:
							toyData.ToyType = child.InnerText;
							break;
						case ATTR_TOY_MASS:
							toyData.Mass = float.Parse(child.InnerText);
							break;
						case ATTR_TOY_COG_OFFSET:
							ToFloats(toyData.CogOffset, child.InnerText);
							break;
						case ATTR_LINEAR_STIFFNESS:
							toyData.LinearStiffness = float.Parse(child.InnerText);
							break;
						case ATTR_LINEAR_DAMPING:
							toyData.LinearDamping = float.Parse(child.InnerText);
							break;
						case ATTR_LOCATOR_HOOK_OFFSET:
							ToFloats(toyData.LocatorHookOffset, child.InnerText);
							break;
						case ATTR_REST_POSITION_OFFSET:
							ToFloats(toyData.RestPositionOffset, child.InnerText);
							break;
						case ATTR_REST_ROTATION_OFFSET:
							ToFloats(toyData.RestRotationOffset, child.InnerText);
							break;
						case ATTR_INSTANCE_OFFSET_LIST:
							foreach (XmlNode childNode in child.ChildNodes) {
								if (childNode.Name == ATTR_INSTANCE_OFFSET) {
									var s = childNode.InnerText.Split(",");
									InstanceOffset offset = new();
									for (int i = 0; i < 3; i++) {
										offset[i] = float.Parse(s[i]);
									}
									toyData.InstanceOffsetList.Add(offset);
								}
							}
							break;
						case ATTR_ROPE_MATERIAL:
							toyData.RopeMaterial = child.InnerText;
							break;
						case ATTR_ANGULAR_STIFFNESS:
							ToFloats(toyData.AngularStiffness, child.InnerText);
							break;
						case ATTR_ANGULAR_DAMPING:
							ToFloats(toyData.AngularDamping, child.InnerText);
							break;
						case ATTR_ANGULAR_AMPLITUDE:
							ToFloats(toyData.AngularAmplitude, child.InnerText);
							break;
						case ATTR_ROPE_WIDTH:
							toyData.RopeWidth = float.Parse(child.InnerText);
							break;
						case ATTR_ROPE_LENGTH:
							toyData.RopeLength = float.Parse(child.InnerText);
							break;
						case ATTR_ROPE_HOOK_OFFSET:
							toyData.RopeHookOffset = float.Parse(child.InnerText);
							break;
						case ATTR_ROPE_TOY_OFFSET:
							toyData.RopeToyOffset = float.Parse(child.InnerText);
							break;
						case ATTR_ROPE_RESOLUTION:
							toyData.RopeResolution = uint.Parse(child.InnerText);
							break;
						case ATTR_POSITION_ITERATIONS:
							toyData.PositionIterations = uint.Parse(child.InnerText);
							break;
						case ATTR_ROPE_LINEAR_DENSITY:
							toyData.RopeLinearDensity = float.Parse(child.InnerText);
							break;
						case ATTR_NODE_DAMPING:
							toyData.NodeDamping = float.Parse(child.InnerText);
							break;
					}
				} catch {

				}
			}
			PhysicsItems.Add(toyData);
		}

		private static void LoadPhysicsPatchData(XmlNode node) {
			string name = GetAttribute(node, ATTR_PHYS_NAME);
			PhysicsPatchData patchData = new(name);
			foreach (XmlNode child in node.ChildNodes) {
				try {
					switch (child.Name) {
						case TITLE_MATERIAL:
							patchData.Material = child.InnerText;
							break;
						case TITLE_AREA_DENSITY:
							patchData.AreaDensity = float.Parse(child.InnerText);
							break;
						case TITLE_AERO_MODEL_TYPE:
							patchData.AeroModelType = child.InnerText;
							break;
						case TITLE_TC_MIN_FIRST:
							ToFloats(patchData.TCMinFirst, child.InnerText);
							break;
						case TITLE_TC_MAX_FIRST:
							ToFloats(patchData.TCMaxFirst, child.InnerText);
							break;
						case TITLE_TC_MIN_SECOND:
							ToFloats(patchData.TCMinSecond, child.InnerText);
							break;
						case TITLE_TC_MAX_SECOND:
							ToFloats(patchData.TCMaxSecond, child.InnerText);
							break;
						case TITLE_X_RES:
							patchData.XRes = uint.Parse(child.InnerText);
							break;
						case TITLE_Y_RES:
							patchData.YRes = uint.Parse(child.InnerText);
							break;
						case TITLE_X_SIZE:
							patchData.XSize = float.Parse(child.InnerText);
							break;
						case TITLE_Y_SIZE:
							patchData.YSize = float.Parse(child.InnerText);
							break;
						case TITLE_LINEAR_STIFFNESS:
							patchData.LinearStiffness = float.Parse(child.InnerText);
							break;
						case TITLE_DRAG_COEFFICIENT:
							patchData.DragCoefficient = float.Parse(child.InnerText);
							break;
						case TITLE_LIFT_COEFFICIENT:
							patchData.LiftCoefficient = float.Parse(child.InnerText);
							break;
					}
				} catch {

				}
			}
			PhysicsItems.Add(patchData);
		}

		private static void ToFloats(Floats floats, string text) {
			if (text == ",," || text == ",")
				return;
			var s = text.Split(",");
			for (int i = 0; i < floats.Length; i++) {
				if (s[i].Length == 0 || s[i] == "null")
					continue;
				floats[i] = float.Parse(s[i]);
			}
		}
	}
}
