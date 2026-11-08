using SCS_Mod_Helper.Modding.Accessories.AccAddon.CreatedSii;
using SCS_Mod_Helper.Modding.Accessories.Physics;
using SCS_Mod_Helper.Utils;
using System.IO;

namespace SCS_Mod_Helper.Modding.Accessories.AccAddon {
	public class AccAddonSCSIO(): AccSCSIO {
		private const string NameAccIntHeader = "accessory_addon_int_data";
		private const string NameAccHeader = "accessory_addon_int_data";
		private const string NamePatchHeader = "accessory_addon_patch_data";
		private const string NameAAExtModel = "exterior_model";
		private const string NameAAIntModel = "interior_model";
		private const string NameAAExtModelUK = "exterior_model_uk";
		private const string NameAAIntModelUK = "interior_model_uk";
		private const string NameAAHideIn = "hide_in";

		bool isPatch = false;

		protected void WriteData(StreamWriter sw, bool isPatch, AccAddonBinding binding, string vehicleID, string modelType, Action data) {
			sw.Write(new string('\t', TabCount));
			string header;
			if (isPatch)
				header = NamePatchHeader;
			else if (binding.Data.Count == 0)//todo 通过accType判断是否车内饰品，大概在后续树状结构才能实现
				header = NameAccHeader;
			else
				header = NameAccIntHeader;
			sw.WriteLine($"{header} : {binding.ModelName}.{vehicleID}.{modelType}");
			BraceIn(sw);
			data();
			BraceOut(sw);
		}

		protected new bool IsArray(string name) {
			if (base.IsArray(name))
				return true;
			switch (name) {
				case NameData://如果modeltype为patch，data的key应为“data”，而非一般的“data[]”
					if (isPatch)
						return false;
					else
						return true;
			}
			return false;
		}

		protected override bool HasQuote(string name) {
			if (base.HasQuote(name))
				return true;
			return name switch {
				NameAAIntModel or NameAAExtModel or NameAAIntModelUK or NameAAExtModelUK => true,
				_ => false,
			};
		}

		public static int CreateAccAddonSii(AccAddonBinding binding) => new AccAddonSCSIO().CreateAccAddonSiiInternal(binding);

		private int CreateAccAddonSiiInternal(AccAddonBinding binding) {
			var created = 0;
			if (binding.VehicleExpandedETS2)
				created += CreateSii(binding, true);
			if (binding.VehicleExpandedATS)
				created += CreateSii(binding, false);
			return created;
		}
		private int CreateSii(AccAddonBinding binding, bool isETS2) {
			int numberCreated = 0;
			foreach (var vehicle in isETS2 ? binding.VehiclesETS2 : binding.VehiclesATS) {
				var siiFile = Paths.AccAddonFile(vehicle.VehicleType, vehicle.VehicleID, vehicle.ModelType, binding.ModelName);
				if (vehicle.Check) {
					if (vehicle.ModelType.Length == 0 || vehicle.VehicleID.Length == 0 || vehicle.Look.Length == 0 || vehicle.Variant.Length == 0)//确保modeltype look variant 都有值，否则跳过
						continue;
				} else {
					if (binding.DeleteUnchecked && File.Exists(siiFile))//如果勾选了删除未勾选sii文件，就将未勾选但存在的sii文件删除
						File.Delete(siiFile);
					continue;
				}
				isPatch = vehicle.ModelType switch {//当modeltype为flag等，addon_data的标题应为accessory_addon_patch_data才能让旗子正常显示，而且填写物理模型的data并非数组，不带中括号
					"flag_l" or "flag_r" or "flag_f_l" or "flag_f_r" => true,
					_ => false,
				};
				using StreamWriter sw = new(siiFile);
				WriteFileStructure(sw, () => {
					WriteData(sw, isPatch, binding, vehicle.VehicleID, vehicle.ModelType, () => {
						WriteLine(sw, NameDisplayName, binding.DisplayName);
						WriteLine(sw, NamePrice, binding.Price ?? 1);
						WriteLine(sw, NameUnlock, binding.UnlockLevel ?? 0);
						WriteLine(sw, NamePartType, binding.PartType, "unknown");
						WriteLine(sw, NameIconName, binding.IconName);
						WriteLine(sw, NameAAIntModel, binding.ModelPath);
						if (string.IsNullOrEmpty(binding.ExtModelPath))
							WriteLine(sw, NameAAExtModel, binding.ModelPath);
						else
							WriteLine(sw, NameAAExtModel, binding.ExtModelPath);
						bool hasUK = isETS2 && !string.IsNullOrEmpty(binding.ModelPathUK);
						if (hasUK) {
							WriteLine(sw, NameAAIntModelUK, binding.ModelPathUK);
							if (string.IsNullOrEmpty(binding.ExtModelPathUK))
								WriteLine(sw, NameAAExtModelUK, binding.ModelPathUK);
							else
								WriteLine(sw, NameAAExtModelUK, binding.ExtModelPathUK);
						}
						if (binding.AddonItem.UseCollPath)
							WriteLine(sw, NameCollPath, binding.CollPath);
						WriteLine(sw, NameLook, vehicle.Look, "default");
						WriteLine(sw, NameVariant, vehicle.Variant, "default");
						WriteLine(sw, NameAAHideIn, binding.HideIn, 0);
						WriteLine(sw, NameElectricType, binding.ElectricType, "vehicle");
						WriteList(sw, NameData, binding.Data);
						WriteList(sw, NameSuitableFor, binding.SuitableFor);
						WriteList(sw, NameConflictWith, binding.ConflictWith);
						WriteList(sw, NameDefaults, binding.Defaults);
						WriteList(sw, NameOverrides, binding.Overrides);
						WriteList(sw, NameRequire, binding.Require);
					});

					for (int i = 0; i < binding.Data.Count; i++) {
						string? pn = binding.Data[i];
						pn = pn.EndsWith(NamePSuffix) ? pn[..^NamePSuffix.Length] : pn;

						var physicsList = new List<PhysicsData>();//如果填写内容时添加过物理模型，添加的模型对象会被储存在binding的PhysicsList内，可以直接读取
						physicsList.AddRange(binding.PhysicsList);
						bool dataWriiten = false;
						for (int j = 0; j < physicsList.Count; j++) {
							var phys = physicsList[j];
							if (pn == phys.PhysicsName) {
								WritePhysicsData(sw, phys);
								physicsList.RemoveAt(j);
								dataWriiten = true;//如果在physicsList找到对应物理模型，就直接用来输出，因为无法直接对外层continue，只能使用变量dataWritten在外层continue。没有dataWriiten的话会导致物理模型被输出两次。
								break;
							}
						}
						if (dataWriiten)
							continue;
						foreach (var physItem in AccessoryPhysicsIO.PhysicsItems) {
							if (pn == physItem.PhysicsName) {
								WritePhysicsData(sw, physItem);
								break;
							}
						}
					}
				});
				numberCreated++;
			}
			return numberCreated;
		}

		public static void ReadAccAddon(CreatedModelItem item) {
			if (File.Exists(item.Path)) {
				using StreamReader sr = new(item.Path);
				string? line;
				while ((line = sr.ReadLine()?.Trim()) != null) {
					if (line == "}")
						break;
					if (line == "{" || line == "SiiNunit" || line.Length == 0 || line.StartsWith('#'))
						continue;
					int colonIndex = line.IndexOf(':');
					var name = line[..colonIndex].Trim();
					var value = ClipValue(line[(colonIndex + 1)..]);
					switch (name) {
						case NameDisplayName:
							item.IngameName = value;
							break;
						case NameLook:
							item.Look = value;
							break;
						case NameVariant:
							item.Variant = value;
							break;
					}
				}
			}
		}
	}
}
