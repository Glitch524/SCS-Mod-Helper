using SCS_Mod_Helper.Language;
using SCS_Mod_Helper.Modding.Accessories.Physics;
using SCS_Mod_Helper.Utils;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace SCS_Mod_Helper.Modding.Accessories.AccHookup {
	public class AccHookupDataIO: AccSCSIO {
		//addon hookup storage
		private const string NameAHSPreffix = "addon_hookup_storage";

		private const string NameAHHeader = "accessory_hookup_int_data";
		private const string NameAHSuffix = ".addon_hookup";
		private const string NameAHModel = "model";

		public AccHookupDataIO() {
			ListHasQuote.Add(NameAHModel);
		}

		protected void WriteData(StreamWriter sw, string modelName, Action data) {
			sw.Write(new string('\t', TabCount));
			sw.WriteLine($"{NameAHHeader} : {modelName}{NameAHSuffix}");
			BraceIn(sw);
			data();
			BraceOut(sw);
		}

		public static void SaveAddonHookup(Window window, AccHookupBinding viewModel) => new AccHookupDataIO().SaveAddonHookupInternal(window, viewModel);

		private void SaveAddonHookupInternal(Window window, AccHookupBinding viewModel) {
			MessageBox.Show(Resource.GetString("MessageSaveBeforeStart"));
			if (viewModel.StorageName.Length == 0) {
				MessageBox.Show(window, Resource.GetString("MessageSaveNoName"));
				return;
			}
			if (viewModel.SuiItems.Count == 0) {
				MessageBox.Show(window, Resource.GetString("MessageSaveSui0"));
				return;
			}
			var StorageDir = Paths.HookupStorageDir(Instances.ProjectLocation);
			Directory.CreateDirectory(StorageDir);
			var storageFilename = $"{NameAHSPreffix}.{viewModel.StorageName}.sii";
			var storageFile = Path.Combine(StorageDir, storageFilename);
			{
				TabCount = 0;
				using StreamWriter sw = new(storageFile);
				WriteFileStructure(sw, () => {
					WriteStorageTips(sw);
					WriteInclude(sw, viewModel);
				});
			}
			MessageBox.Show(window, Resource.GetString("MessageSaved"));
		}

		private static void WriteStorageTips(StreamWriter sw) {
			sw.WriteLine("# For modders: Please do not modify this file if you want to add a new entry. Create in");
			sw.WriteLine("# this directory a new file \"<base_name>.<idofyourmod>.sii\" where <base_name> is name of");
			sw.WriteLine("# base file without the extension (e.g. \"city\" for \"/def/city.sii\") and <idofyourmod> is");
			sw.WriteLine("# some string which is unlikely to conflict with other mod.");
			sw.WriteLine("#");
			sw.WriteLine("# Warning: Even if the units are specified in more than one source file, they share the");
			sw.WriteLine("# same namespace so suffixes or prefixes should be used to avoid conflicts.");
			sw.WriteLine("");
		}
		private void WriteInclude(StreamWriter sw, AccHookupBinding viewModel) {
			string includeFormat = "@include \"addon_hookups/{0}.sui\"";
			foreach (var sui in viewModel.SuiItems) {
				if (sui.SuiFilename.Length == 0 || sui.HookupItems.Count == 0 && sui.PhysicsItems.Count == 0)
					continue;
				sw.WriteLine(string.Format(includeFormat, sui.SuiFilename));//@include的前面似乎不能有空格或制表符，否则会无法读取

				var cTab = TabCount;//因为这时候要把acchookup写入另一个文件内
				TabCount = 0;//所以tabcount要从0开始
				CreateAddonHookupSui(sui);
				TabCount = cTab;//这里暂时记录目前文件的tabcount，过后读回来
			}
		}

		private void CreateAddonHookupSui(SuiItem sui) {
			var suiPath = Paths.AddonHookupsDir(Instances.ProjectLocation, sui.SuiFilename);
			var physicsDatas = new List<PhysicsData>();
			physicsDatas.AddRange(sui.PhysicsItems);
			using StreamWriter sw = new(suiPath);
			foreach (var hookup in sui.HookupItems) {
				WriteData(sw, hookup.ModelName, () => {
					WriteLine(sw, NameDisplayName, hookup.DisplayName);
					WriteLine(sw, NamePrice, hookup.Price ?? 1);
					WriteLine(sw, NameUnlock, hookup.UnlockLevel ?? 0);
					WriteLine(sw, NameIconName, hookup.IconName);
					WriteLine(sw, NamePartType, hookup.PartType, "unknown");
					WriteLine(sw, NameAHModel, hookup.ModelPath);
					WriteLine(sw, NameCollPath, hookup.CollPath);
					WriteLine(sw, NameLook, hookup.Look, "default");
					WriteLine(sw, NameVariant, hookup.Variant, "default");
					WriteLine(sw, NameElectricType, hookup.ElectricType, "vehicle");

					WriteList(sw, NameData, hookup.Data);
					WriteList(sw, NameSuitableFor, hookup.SuitableFor);
					WriteList(sw, NameConflictWith, hookup.ConflictWith);
					WriteList(sw, NameDefaults, hookup.Defaults);
					WriteList(sw, NameOverrides, hookup.Overrides);
					WriteList(sw, NameRequire, hookup.Require);
				});

				for (int i = 0; i < hookup.Data.Count; i++) {
					string? pn = hookup.Data[i];
					pn = pn.EndsWith(NamePSuffix) ? pn[..^NamePSuffix.Length] : pn;

					for (int j = 0; j < physicsDatas.Count; j++) {
						var phys = physicsDatas[j];
						if (pn == phys.PhysicsName) {

							WritePhysicsData(sw, phys);
							physicsDatas.RemoveAt(j);
							break;
						}
					}
				}
			}
		}
		public static void LoadAddonHookup(AccHookupBinding viewModel) {
			DirectoryInfo accHookupDir = new(Paths.HookupStorageDir(Instances.ProjectLocation));
			FileInfo? storageFile = null;
			bool skip = true;
			foreach (var file in accHookupDir.GetFiles()) {
				var filename = file.Name;
				if (filename.StartsWith(NameAHSPreffix) && filename.EndsWith(".sii")) {
					storageFile = file;
					if (skip) {
						skip = false;
						continue;
					}
					break;
				}
			}
			if (storageFile == null)
				return;
			viewModel.StorageName = storageFile.Name[21..^4];
			{
				using StreamReader sReader = new(storageFile.FullName);
				var line = sReader.ReadLine()?.Trim();
				if (line == null || line != FileHeader)
					return;
				while ((line = sReader.ReadLine()?.Trim()) != null) {
					if (line.StartsWith("@include")) {
						var start = line.IndexOf('"');
						var end = line.LastIndexOf('"');
						var suiPath = line[(start + 1)..end];
						suiPath = suiPath.Replace('/', '\\');
						var hookupFile = new FileInfo(Path.Combine(accHookupDir.FullName, suiPath));
						var suiFilename = hookupFile.Name;
						SuiItem suiItem = new(suiFilename[..^4]);
						LoadAddonHookupSui(suiItem, hookupFile);

						Application.Current.Dispatcher.Invoke(new(() => {
							viewModel.SuiItems.Add(suiItem);
						}), DispatcherPriority.DataBind);
					}
				}
				Application.Current.Dispatcher.Invoke(new(() => {
					viewModel.CurrentSuiItem = viewModel.SuiItems.FirstOrDefault();
				}), DispatcherPriority.DataBind);
			}
		}

		private static void LoadAddonHookupSui(SuiItem sui, FileInfo hookupFile) {
			if (!hookupFile.Exists) return;
			using StreamReader sr = new(hookupFile.FullName);
			string? line = null;
			while ((line = sr.ReadLine()?.Trim()) != null) {
				if (line.Length == 0) continue;
				var name = line.Split(":")[1].Trim();
				name = name[..name.LastIndexOf('.')];
				if (line.StartsWith("accessory_hookup")) {
					AccessoryHookupData item = new(name);
					ReadAccHookup(sr, item);
					sui.HookupItems.Add(item);
				} else if (line.StartsWith(NamePTHeader)) {
					PhysicsToyData toyData = new(name);
					ReadPhysToyData(sr, toyData);
					sui.PhysicsItems.Add(toyData);
				} else if (line.StartsWith(NamePPHeader)) {
					PhysicsPatchData patchData = new(name);
					ReadPhysPatchData(sr, patchData);
					sui.PhysicsItems.Add(patchData);

				}
			}
		}

		private static void ReadAccHookup(StreamReader sr, AccessoryHookupData item) {
			string? line;
			while ((line = sr.ReadLine()?.Trim()) != null) {
				if (line == "}")
					break;
				if (line == "{" || line.Length == 0 || line.StartsWith('#'))
					continue;
				int colonIndex = line.IndexOf(':');
				var name = line[..colonIndex].Trim();
				if (name.EndsWith("[]"))
					name = name[..^2];
				var value = ClipValue(line[(colonIndex + 1)..]);
				switch (name) {
					case NameDisplayName:
						item.DisplayName = value;
						break;
					case NameIconName:
						item.IconName = value;
						break;
					case NamePrice:
						item.Price = long.Parse(value);
						break;
					case NameUnlock:
						item.UnlockLevel = uint.Parse(value);
						break;
					case NamePartType:
						item.PartType = value;
						break;
					case NameAHModel:
						item.ModelPath = value;
						break;
					case NameCollPath:
						item.CollPath = value;
						break;
					case NameLook:
						item.Look = value;
						break;
					case NameVariant:
						item.Variant = value;
						break;
					case NameData:
						item.Data.Add(value);
						break;
					case NameSuitableFor:
						item.SuitableFor.Add(value);
						break;
					case NameConflictWith:
						item.ConflictWith.Add(value);
						break;
					case NameDefaults:
						item.Defaults.Add(value);
						break;
					case NameOverrides:
						item.Overrides.Add(value);
						break;
					case NameRequire:
						item.Require.Add(value);
						break;
				}
			}
		}
	}
}
