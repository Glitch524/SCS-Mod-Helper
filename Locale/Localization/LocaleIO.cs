using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Language;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace SCS_Mod_Helper.Locale.Localization {
	internal class LocaleIO: SCSIO {
		private const string NameLocHeader = "localization_db : .localization";
		private const string DictPreffix = "local_module";
		private const string NameKey = "key";
		private const string NameValue = "val";
		private const string KeyGenerated = "# Generated";//用来判断locale文件是否为自动创建的文件
		private void WriteLocalHeader(StreamWriter sw, bool generated, Action data) {
			sw.Write(new string('\t', TabCount));
			sw.Write(NameLocHeader);
			if (generated)
				sw.Write(" " + KeyGenerated);
			sw.WriteLine();
			BraceIn(sw);
			data();
			BraceOut(sw);
		}
		public LocaleIO() {
			ListIsArray.AddRange([NameKey, NameValue]);
			ListHasQuote.AddRange([NameKey, NameValue]);

		}

		/// <summary>
		/// 异步读取本地化目录，解析并返回所有地区的本地化模块列表。
		/// </summary>
		/// <remarks>遇到格式错误或异常时会弹出消息框提示错误信息。</remarks>
		/// <returns>包含所有已解析本地化模块的列表。如果目录不存在则返回空列表。</returns>
		public static List<LocaleModule> ReadLocaleDict() {
			Dictionary<string, LocaleModule> modules = [];
			var localeDir = new DirectoryInfo(Paths.LocaleDir());
			if (!localeDir.Exists)
				return [.. modules.Values];
			foreach (var dir in localeDir.GetDirectories()) {//地区文件夹，文件夹名为各个地区的locale名
				foreach (var file in dir.GetFiles()) {
					if (file.Name.StartsWith(DictPreffix)) {
						try {
							string moduleName = GetContent(file.Name, '.');
							if (!modules.TryGetValue(moduleName, out LocaleModule? module)) {
								module = new(moduleName);
								modules.Add(moduleName, module);
							}
							if (module == null)
								return [.. modules.Values];
							var locale = dir.Name;
							var langDict = module.GetLocale(locale)!;
							ReadLocaleFile(module, file, langDict);

						} catch (Exception ex) {
							MessageBox.Show(Resource.GetString("MessageLoadErr") + "\n" + ex.Message);
						}
					}
				}
			}
			return [.. modules.Values];
		}

		private static void ReadLocaleFile(LocaleModule module, FileInfo file, ModLocale langDict) {
			using StreamReader sr = new(file.FullName);
			string? line = sr.ReadLine()?.Trim();
			if (line == null || line != FileHeader)
				throw new(Resource.GetString("MessageLoadErrNotLocale"));
			string? stringKey = null;
			bool copyToUni = false;
			while ((line = sr.ReadLine()?.Trim()) != null) {
				if (line.Length == 0 || line == "{")
					continue;
				if (line.StartsWith("localization_db")) {
					if (line.EndsWith(KeyGenerated)) {
						if (module.UniversalDict.Count == 0) {
							copyToUni = true;
						} else {
							break;//如果在已经使用自动生成的文件收集到universal以后，其他自动生成文件已经没有必要读了，所以直接停止读文件
						}
					}
					continue;
				}
				if (line == "}")
					break;
				if (line.StartsWith(NameKey)) {
					stringKey = GetContent(line);
				} else if (line.StartsWith(NameValue)) {
					var stringValue = GetContent(line);
					if (stringKey != null) {
						if (copyToUni) {
							module.UniversalDict.Add(new(stringKey, stringValue));
						} else {
							langDict.AddPair(stringKey, stringValue);
						}
					}
					stringKey = null;
				}
			}
		}

		/// <summary>
		/// 提取位于指定字符之间的子字符串。
		/// 用于提取locale文件名内的名称
		/// 也用于提取locale文件内的key和value的值
		/// </summary>
		/// <param name="line">包含要提取内容的字符串。</param>
		/// <param name="indexValue">用于界定内容的字符，默认为双引号（"）。</param>
		/// <returns>位于两个指定字符之间的子字符串。</returns>
		private static string GetContent(string line, char indexValue = '"') {
			var start = line.IndexOf(indexValue);
			var end = line.LastIndexOf(indexValue);
			return line[(start + 1)..end];
		}

		public static void SaveLocaleDict(Window window, ObservableCollection<LocaleModule> moduleList, ObservableCollection<LocaleModule> deletedModuleList) {
			new LocaleIO().SaveLocaleDictInternal(window, moduleList, deletedModuleList);
		}

		public void SaveLocaleDictInternal(Window window, ObservableCollection<LocaleModule> moduleList, ObservableCollection<LocaleModule> deletedModuleList) {
			var unnamedCount = 0;
			foreach (var module in moduleList) {
				if (module.ModuleName.Length == 0) {
					unnamedCount++;
				}
			}
			MessageBox.Show(window, Resource.GetQuantityString("MessageSaveErrNoName", unnamedCount));
			foreach (var module in moduleList) {
				var moduleName = module.ModuleName;
				var universal = module.UniversalDict;
				foreach (var locale in module.LocaleList) {
					if (locale.LocaleValue == Locale.LocaleValueUni) {
						foreach (var pair in locale.Dictionary) {
							Instances.LocaleDictAdd(pair.Key, pair.Value);
						}
						continue;
					}
					CreateLocaleSii(moduleName, locale, universal);
				}
			}
			foreach (var module in deletedModuleList) {
				DeleteLocaleSii(module);
			}
			Instances.CleanLocaleModules();//mod localization更改并保存后，清理Instances内的locale字典，使用时重新加载
		}

		private void CreateLocaleSii(string moduleName, ModLocale locale, ObservableCollection<LocalePair> universal) {
			ObservableCollection<LocalePair> dict;
			bool Genearated = false;
			var localeFile = Paths.LocaleFile(Instances.ProjectLocation, locale.LocaleValue, moduleName);
			if (locale.Dictionary.Count > 0)//如果当前字典内有值，就输出字典的值
				dict = locale.Dictionary;
			else if (universal.Count > 0) {//如果有通用字典，则输出通用字典内容，并将代表通用字典内容的generated设置为true
				dict = universal;
				Genearated = true;
			} else {//如果通用字典为空，就删除已有locale文件
				File.Delete(localeFile);
				var parent = Directory.GetParent(localeFile);
				if (parent != null && parent.GetFiles().Length == 0)
					parent.Delete();
				return;
			}
			var hasValue = false;
			{
				using StreamWriter sw = new(localeFile);
				WriteFileStructure(sw, () => {
					WriteLocalHeader(sw, Genearated, () => {
						foreach (var pair in dict) {
							if (pair.Key.Length == 0)
								continue;
							WriteEmptyLine(sw);
							if (!hasValue) {
								hasValue = true;
							}
							WriteLine(sw, NameKey, pair.Key);
							WriteLine(sw, NameValue, pair.Value);
							if (!Genearated)
								Instances.LocaleDictAdd(pair.Key, pair.Value);
						}
					});
				});
			}
			if (!hasValue) {
				File.Delete(localeFile);
				var parent = Directory.GetParent(localeFile);
				if (parent != null && parent.GetFiles().Length == 0)
					parent.Delete();
			}
		}

		private static void DeleteLocaleSii(LocaleModule module) {
			var moduleName = module.ModuleName;
			foreach (var locale in module.LocaleList) {
				if (locale.IsUniversal)
					continue;
				var localeFile = Paths.LocaleFile(Instances.ProjectLocation, locale.LocaleValue, moduleName, false);
				File.Delete(localeFile);
				var parent = Directory.GetParent(localeFile)!;
				if (parent.GetFiles().Length == 0)
					parent.Delete();
			}
			var localeDir = new DirectoryInfo(Paths.LocaleDir(Instances.ProjectLocation));
			if (localeDir.GetDirectories().Length == 0 && localeDir.GetFiles().Length == 0)
				localeDir.Delete();
		}
	}
}
