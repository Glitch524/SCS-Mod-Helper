using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Text;

namespace SCS_Mod_Helper.Locale.StringRes; 
internal class StringResUtil {
	private static ObservableCollection<StringResItem>? mStringResList = null;
	public static ObservableCollection<StringResItem> StringResList {
		get {
			if (mStringResList == null) {
				SetupStringResList();
			}
			return mStringResList!;
		}
	}
	public static void CleanStringResList() => mStringResList = null;

	/// <summary>
	/// 初始化或重置字符串资源列表，并根据本地化字典内容填充资源项。
	/// </summary>
	public static void SetupStringResList() {
		if (mStringResList == null)
			mStringResList = [];
		else
			mStringResList.Clear();
		var dict = Instances.LocaleDict;
		foreach (var pair in dict) {
			StringBuilder tips = new();
			tips.Append(Util.GetString("ResourceValueHeader"));
			foreach (var value in pair.Value) {
				tips.Append('\n');
				tips.Append($"    {value}");
			}
			mStringResList.Add(new(pair.Key, tips.ToString()));
		}
	}

	/// <summary>
	/// 根据指定格式的 displayName，返回多语言资源字符串，并输出首选本地化名称。
	/// </summary>
	/// <remarks>偶数部分为普通文本，奇数部分作为资源键进行本地化查询。优先返回系统语言或通用语言的本地化结果。</remarks>
	/// <param name="displayName">包含资源键和普通文本的字符串，使用 "@@" 分隔。</param>
	/// <param name="localizedName">输出一条最匹配的本地化结果，用于保存配置文件时默认的名称</param>
	/// <returns>拼接后的多语言资源字符串。</returns>
	public static string GetStringResResults(string displayName, out string localizedName) {
		var localeModules = Instances.LocaleModules;
		if (displayName.Contains("@@")) {
			var split = displayName.Split("@@");
			Dictionary<string, string[]> values = [];//key: locale名, value: string数组 与split对应
			foreach (var localeModule in localeModules) {
				foreach (var dict in localeModule.LocaleList) {
					if (dict.Dictionary.Count == 0)
						continue;
					var localeValue = dict.LocaleValue;
					if (values.TryGetValue(localeValue, out string[]? splitValues)) {//多个Module可能会有相同的Locale，如果前面有未找到结果的key，则在另一个module的相同locale内寻找
						for (int i = 0; i < split.Length; i++) {
							if (i % 2 == 1) {//偶数部分不需要修改
								var resultValue = dict.GetValue(splitValues[i]);
								if (resultValue != null) {
									splitValues[i] = resultValue;//如果找到对应Key，就替换为对应文本
								}
							}
						}
					} else {
						splitValues = new string[split.Length];//初始化数组
						for (int i = 0; i < split.Length; i++) {
							if (i % 2 == 1) {
								var resultValue = dict.GetValue(split[i]);
								splitValues[i] = resultValue ?? split[i];//如果找到对应Key，就替换为对应文本，否则保持原样
							} else
								splitValues[i] = split[i];//偶数部分不需要处理，直接赋值
						}
						values.Add(localeValue, splitValues);
					}
				}
			}
			if (values.Count == 0) {//没有获得任何语言数据，直接返回原始文本
				localizedName = displayName.Replace("@@", "");
				return localizedName;
			}

			string? localized = null;//用于保存配置文件时默认的名称
			StringBuilder finalValue = new();
			for (int i = 0; i < values.Count; i++) {
				if (i > 0)
					finalValue.Append('\n');
				var pair = values.ElementAt(i);
				var final = string.Join("", pair.Value);
				finalValue.Append(final);

				if (localized == null) {
					if (pair.Key == Util.SystemLocaleForSCS()) {//优先选择与系统语言相同的语言
						localized = final;
					} else if (pair.Key == "universal" || pair.Key.StartsWith("en")) {//然后优先选择universal或en开头的语言
						localized = final;
					}
				}
			}
			localized ??= displayName.Replace("@@", "");//如果最终还是没有赋值，就使用原始文本
			localizedName = localized;
			return finalValue.ToString();
		} else {//displayName没有@@的话，直接返回原始文本，没必要各种进行各种foreach
			localizedName = displayName;
			return localizedName;
		}
	}
}

public class StringResItem(string header, string values): BaseBinding {
	private string mHeader = header;
	public string Header {
		get => mHeader;
		set {
			mHeader = value;
			InvokeChange();
		}
	}

	private string mValues = values;
	public string Values {
		get => mValues;
		set {
			mValues = value;
			InvokeChange();
		}
	}
}
