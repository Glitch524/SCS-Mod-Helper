using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Modding.Accessories.Physics;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace SCS_Mod_Helper.Modding.Accessories {
	public abstract class AccSCSIO: SCSIO {

		protected const string NameDisplayName = "name";
		protected const string NamePrice = "price";
		protected const string NameUnlock = "unlock";
		protected const string NamePartType = "part_type";
		protected const string NameIconName = "icon";
		protected const string NameCollPath = "coll";
		protected const string NameLook = "look";
		protected const string NameVariant = "variant";
		protected const string NameElectricType = "electric_type";

		public const string NameData = "data";

		public const string NameSuitableFor = "suitable_for";
		public const string NameConflictWith = "conflict_with";
		public const string NameDefaults = "defaults";
		public const string NameOverrides = "overrides";
		public const string NameRequire = "require";

		public AccSCSIO() {
			ListIsArray.AddRange([
				NameSuitableFor,
				NameConflictWith,
				NameDefaults,
				NameOverrides,
				NameRequire]);
			ListHasQuote.AddRange([
				NameDisplayName,
				NameIconName,
				NameCollPath]);
		}


		//physics data
		public const string NamePSuffix = ".phys_data";
		//physics toy data
		protected const string NamePTHeader = "physics_toy_data";
		public const string NamePTModel = "phys_model";
		public const string NamePTColl = "phys_model_coll";
		public const string NamePTLook = "phys_model_look";
		public const string NamePTVariant = "phys_model_variant";
		public const string NamePTToyType = "toy_type";
		public const string NamePTMass = "toy_mass";
		public const string NamePTCogOffset = "toy_cog_offset";
		public const string NamePTLinearStiffness = "linear_stiffness";
		public const string NamePTLinearDamping = "linear_damping";
		public const string NamePTAngularStiffness = "angular_stiffness";
		public const string NamePTAngularDamping = "angular_damping";
		public const string NamePTAngularAmplitude = "angular_amplitude";
		public const string NamePTNodeDamping = "node_damping";
		public const string NamePTLocatorHookOffset = "locator_hook_offset";
		public const string NamePTRestPositionOffset = "rest_position_offset";
		public const string NamePTRestRotationOffset = "rest_rotation_offset";
		public const string NamePTInstanceOffset = "instance_offset";
		public const string NamePTRopeWidth = "rope_width";
		public const string NamePTRopeLength = "rope_length";
		public const string NamePTRopeHookOffset = "rope_hook_offset";
		public const string NamePTRopeToyOffset = "rope_toy_offset";
		public const string NamePTRopeResolution = "rope_resolution";
		public const string NamePTRopeLinearDensity = "rope_linear_density";
		public const string NamePTPositionIterations = "position_iterations";
		public const string NamePTRopeMaterial = "rope_material";


		//physics patch data
		protected const string NamePPHeader = "physics_patch_data";
		public const string NamePPMaterial = "material";
		public const string NamePPAreaDensity = "area_density";
		public const string NamePPAeroModelType = "aero_model_type";
		public const string NamePPTCMinFirst = "tc_min_first";
		public const string NamePPTCMaxFirst = "tc_max_first";
		public const string NamePPTCMinSecond = "tc_min_second";
		public const string NamePPTCMaxSecond = "tc_max_second";
		public const string NamePPXRes = "x_res";
		public const string NamePPYRes = "y_res";
		public const string NamePPXSize = "x_size";
		public const string NamePPYSize = "y_size";
		public const string NamePPLinearStiffness = "linear_stiffness";
		public const string NamePPDragCoefficient = "drag_coefficient";
		public const string NamePPLiftCoefficient = "lift_coefficient";

		private void WritePhysicsToyData(StreamWriter sw, string physicsName, Action data) {
			sw.Write(new string('\t', TabCount));
			sw.WriteLine($"{NamePTHeader} : {physicsName}{NamePSuffix}");
			BraceIn(sw);
			data();
			BraceOut(sw);
		}
		private void WritePhysicsPatchData(StreamWriter sw, string physicsName, Action data) {
			sw.Write(new string('\t', TabCount));
			sw.WriteLine($"{NamePPHeader} : {physicsName}{NamePSuffix}");
			BraceIn(sw);
			data();
			BraceOut(sw);
		}

		protected void WritePhysicsData(StreamWriter sw, PhysicsData phys) {
			if (phys is PhysicsToyData toyData) {
				if (toyData.ModelPath == null)
					return;
				WritePhysicsToyData(sw, toyData.PhysicsName, () => {
					WriteLine(sw, NamePTModel, toyData.ModelPath);
					WriteLine(sw, NamePTColl, toyData.CollPath);
					WriteLine(sw, NamePTLook, toyData.Look);
					WriteLine(sw, NamePTVariant, toyData.Variant);
					WriteEmptyLine(sw);
					WriteLine(sw, NamePTToyType, toyData.ToyType);
					WriteLine(sw, NamePTMass, toyData.Mass, 0.1f);
					WriteLine(sw, NamePTCogOffset, toyData.CogOffset, 0f, 0f, 0f);
					WriteLine(sw, NamePTLinearStiffness, toyData.LinearStiffness, 1f);
					WriteLine(sw, NamePTLinearDamping, toyData.LinearDamping, 0.5f);
					WriteLine(sw, NamePTLocatorHookOffset, toyData.LocatorHookOffset, 0f, 0f, 0f);
					WriteLine(sw, NamePTRestPositionOffset, toyData.RestPositionOffset, 0f, 0f, 0f);
					WriteLine(sw, NamePTRestRotationOffset, toyData.RestRotationOffset, 0f, 0f, 0f);
					foreach (var offset in toyData.InstanceOffsetList) {
						WriteLine(sw, NamePTInstanceOffset, offset);
					}
					WriteEmptyLine(sw);
					WriteLine(sw, NamePTAngularStiffness, toyData.AngularStiffness, 0.2f, 0.2f);
					WriteLine(sw, NamePTAngularDamping, toyData.AngularDamping, 0.2f, 0.2f);
					WriteLine(sw, NamePTAngularAmplitude, toyData.AngularAmplitude, 0f, 0f, 0f);
					WriteEmptyLine(sw);
					WriteLine(sw, NamePTRopeMaterial, toyData.RopeMaterial, string.Empty);
					WriteLine(sw, NamePTRopeWidth, toyData.RopeWidth, 0.5f);
					WriteLine(sw, NamePTRopeLength, toyData.RopeLength, 0f);
					WriteLine(sw, NamePTRopeHookOffset, toyData.RopeHookOffset, 0f);
					WriteLine(sw, NamePTRopeToyOffset, toyData.RopeToyOffset, 0f);
					WriteLine(sw, NamePTRopeResolution, toyData.RopeResolution, 8);
					WriteLine(sw, NamePTRopeLinearDensity, toyData.RopeLinearDensity, 1f);
					WriteLine(sw, NamePTPositionIterations, toyData.PositionIterations, 10);
					WriteLine(sw, NamePTNodeDamping, toyData.NodeDamping, 0f);
				});
			} else if (phys is PhysicsPatchData patchData) {
				if (patchData.Material.Length == 0 || patchData.PhysicsName.Length == 0)
					return;
				WritePhysicsPatchData(sw, patchData.PhysicsName, () => {
					WriteLine(sw, NamePPMaterial, patchData.Material);

					WriteLine(sw, NamePPAreaDensity, patchData.AreaDensity, 2f);
					WriteLine(sw, NamePPAeroModelType, patchData.AeroModelType);

					WriteLine(sw, NamePPLinearStiffness, patchData.LinearStiffness, 0.4f);
					WriteLine(sw, NamePPDragCoefficient, patchData.DragCoefficient, 0.1f);
					WriteLine(sw, NamePPLiftCoefficient, patchData.LiftCoefficient, 0.3f);

					WriteLine(sw, NamePPTCMinFirst, patchData.TCMinFirst, 0f, 0f);
					WriteLine(sw, NamePPTCMaxFirst, patchData.TCMaxFirst, 1f, 1f);
					WriteLine(sw, NamePPTCMinSecond, patchData.TCMinSecond, 0f, 0f);
					WriteLine(sw, NamePPTCMaxSecond, patchData.TCMaxSecond, 1f, 1f);

					WriteLine(sw, NamePPXRes, patchData.XRes, 3);
					WriteLine(sw, NamePPYRes, patchData.YRes, 3);
					WriteLine(sw, NamePPXSize, patchData.XSize, 1f);
					WriteLine(sw, NamePPYSize, patchData.YSize, 1f);
				});
			}
		}

		protected void WriteList(StreamWriter sw, string listName, ObservableCollection<string> list) {
			for (int i = 0; i < list.Count; i++) {
				string? item = list[i];
				if (item.Length == 0)
					continue;
				if (listName == NameData && !item.EndsWith(NamePSuffix))
					item += NamePSuffix;
				WriteLine(sw, listName, item);
			}
		}

		protected static void ReadPhysToyData(StreamReader sr, PhysicsToyData data) {
			string? line;
			while ((line = sr.ReadLine()?.Trim()) != null) {
				if (line == "}")
					break;
				if (line == "{" || line.Length == 0 || line.StartsWith('#'))
					continue;
				int colonIndex = line.IndexOf(':');
				var name = line[..colonIndex].Trim();
				var value = ClipValue(line[(colonIndex + 1)..]);
				switch (name) {
					case NamePTModel:
						data.ModelPath = value;
						break;
					case NamePTColl:
						data.CollPath = value;
						break;
					case NamePTLook:
						data.Look = value;
						break;
					case NamePTVariant:
						data.Variant = value;
						break;
					case NamePTToyType:
						data.ToyType = value;
						break;
					case NamePTMass:
						data.Mass = float.Parse(value);
						break;
					case NamePTCogOffset:
						FloatParse(value, data.CogOffset);
						break;
					case NamePTLinearStiffness:
						data.LinearStiffness = float.Parse(value);
						break;
					case NamePTLinearDamping:
						data.LinearDamping = float.Parse(value);
						break;
					case NamePTAngularStiffness:
						FloatParse(value, data.AngularStiffness);
						break;
					case NamePTAngularDamping:
						FloatParse(value, data.AngularDamping);
						break;
					case NamePTAngularAmplitude:
						FloatParse(value, data.AngularAmplitude);
						break;
					case NamePTNodeDamping:
						data.NodeDamping = float.Parse(value);
						break;
					case NamePTLocatorHookOffset:
						FloatParse(value, data.LocatorHookOffset);
						break;
					case NamePTRestPositionOffset:
						FloatParse(value, data.RestPositionOffset);
						break;
					case NamePTRestRotationOffset:
						FloatParse(value, data.RestRotationOffset);
						break;
					case NamePTInstanceOffset:
						InstanceOffset inf = new();
						FloatParse(value, inf);
						data.InstanceOffsetList.Add(inf);
						break;
					case NamePTRopeWidth:
						data.RopeWidth = float.Parse(value);
						break;
					case NamePTRopeLength:
						data.RopeLength = float.Parse(value);
						break;
					case NamePTRopeHookOffset:
						data.RopeHookOffset = float.Parse(value);
						break;
					case NamePTRopeToyOffset:
						data.RopeToyOffset = float.Parse(value);
						break;
					case NamePTRopeResolution:
						data.RopeResolution = uint.Parse(value);
						break;
					case NamePTRopeLinearDensity:
						data.RopeLinearDensity = float.Parse(value);
						break;
					case NamePTPositionIterations:
						data.PositionIterations = uint.Parse(value);
						break;
					case NamePTRopeMaterial:
						data.RopeMaterial = value;
						break;
				}
			}
		}

		protected static void ReadPhysPatchData(StreamReader sr, PhysicsPatchData data) {
			string? line;
			while ((line = sr.ReadLine()?.Trim()) != null) {
				if (line == "}")
					break;
				if (line == "{" || line.Length == 0 || line.StartsWith('#'))
					continue;
				int colonIndex = line.IndexOf(':');
				var name = line[..colonIndex].Trim();
				var value = ClipValue(line[(colonIndex + 1)..]);
				switch (name) {
					case NamePPMaterial:
						data.Material = value;
						break;
					case NamePPAreaDensity:
						data.AreaDensity = float.Parse(value);
						break;
					case NamePPAeroModelType:
						data.AeroModelType = value;
						break;
					case NamePPLinearStiffness:
						data.LinearStiffness = float.Parse(value);
						break;
					case NamePPDragCoefficient:
						data.DragCoefficient = float.Parse(value);
						break;
					case NamePPLiftCoefficient:
						data.LiftCoefficient = float.Parse(value);
						break;
					case NamePPTCMinFirst:
						FloatParse(value, data.TCMinFirst);
						break;
					case NamePPTCMaxFirst:
						FloatParse(value, data.TCMaxFirst);
						break;
					case NamePPTCMinSecond:
						FloatParse(value, data.TCMinSecond);
						break;
					case NamePPTCMaxSecond:
						FloatParse(value, data.TCMaxSecond);
						break;
					case NamePPXRes:
						data.XRes = uint.Parse(value);
						break;
					case NamePPYRes:
						data.YRes = uint.Parse(value);
						break;
					case NamePPXSize:
						data.XSize = float.Parse(value);
						break;
					case NamePPYSize:
						data.YSize = float.Parse(value);
						break;
				}
			}
		}


		/// <summary>
		/// <para>读取pit文件内的look和variant，pit内有两种方法获取</para>
		/// <para>从blender生成的pit文件前几行会有注释写明包含的所有look和variant</para>
		/// <para>使用转换软件生成的pit则没有，只能文件的具体数据读取</para>
		/// </summary>
		public static void ReadLookAndVariant(string pitFile, ObservableCollection<string> LookList, ObservableCollection<string> VariantList) {
			if (File.Exists(pitFile)) {
				using StreamReader sr = new(pitFile);
				string? line;
				do {
					line = sr.ReadLine()?.Trim();
				} while (string.IsNullOrEmpty(line));
				if (line.StartsWith('#')) {//直接convert获得的pit里，文件头会有#开头，记录look和variant的内容
					do {
						line = line[1..].Trim();
						if (line == "Look Names:")
							ReadNames(sr, LookList);
						else if (line == "Variant Names:")
							ReadNames(sr, VariantList);
					} while ((line = sr.ReadLine()?.Trim()) != null && line.Contains('#'));
				} else {
					int variantCount = -1;
					int variantFound = 0;//读完look还是要读variant，记录了也没用，读完variant才可以直接退出
					while ((line = sr.ReadLine()?.Trim()) != null) {
						if (line.StartsWith("VariantCount:", StringComparison.OrdinalIgnoreCase)) {
							variantCount = int.Parse(line["VariantCount:".Length..].Trim());
						} else if (line.StartsWith("Look {", StringComparison.OrdinalIgnoreCase)) {
							ReadNamesAlt(sr, LookList);
						} else if (line.StartsWith("Variant {", StringComparison.OrdinalIgnoreCase)) {
							if (ReadNamesAlt(sr, VariantList)) {
								variantFound++;
								if (variantCount > 0 && variantFound >= variantCount)
									break;
							}
						}
					}
				}
			} else {
				LookList.Add("default");//即使模型文件不存在，也至少留两个default
				VariantList.Add("default");
			}
		}

		private static void ReadNames(StreamReader sr, ObservableCollection<string> list) {
			Application.Current.Dispatcher.Invoke(() => {
				string? line;
				while ((line = sr.ReadLine()?.Trim()) != null) {
					if (line.StartsWith('#')) {
						line = line[1..].Trim();
						if (line.Length > 0)
							list.Add(line);
						else
							break;
					} else {
						break;
					}
				}
			}, DispatcherPriority.Render);
		}

		private static bool ReadNamesAlt(StreamReader sr, ObservableCollection<string> list) {
			string? line;
			if ((line = sr.ReadLine()?.Trim()) != null && line.Contains("Name:", StringComparison.OrdinalIgnoreCase)) {
				int start = line.IndexOf('\"');
				int end = line.LastIndexOf('\"');
				var look = line.Substring(start + 1, end - start - 1);
				Application.Current.Dispatcher.Invoke(() => {
					list.Add(look);
				}, DispatcherPriority.Render);
				return true;
			}
			return false;
		}
	}
}
