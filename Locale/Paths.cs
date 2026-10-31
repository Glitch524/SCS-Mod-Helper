using SCS_Mod_Helper.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SCS_Mod_Helper.Utils;

partial class Paths {
	/// <summary>
	/// 获得指定项目内的locale文件夹路径
	/// </summary>
	public static string LocaleDir(string projectLocation) => Path.Combine(projectLocation, "locale");

	/// <summary>
	/// 获得打开的项目内的locale文件夹路径
	/// </summary>
	public static string LocaleDir() => Path.Combine(Instances.ProjectLocation, "locale");

	/// <summary>
	/// 获得指定项目内，指定地区的locale文件路径
	/// </summary>
	public static string LocaleFile(string projectLocation, string locale, string moduleName, bool createDir = true) {
		string dir = Path.Combine(projectLocation, "locale", locale);
		if (createDir)
			Directory.CreateDirectory(dir);
		return Path.Combine(dir, $"local_module.{moduleName}.sii");
	}

	/// <summary>
	/// 获得打开的项目内，指定地区的locale文件路径
	/// </summary>
	public static string LocaleFile(string locale, string moduleName, bool createDir = true) => LocaleFile(Instances.ProjectLocation, locale, moduleName, createDir);
}
