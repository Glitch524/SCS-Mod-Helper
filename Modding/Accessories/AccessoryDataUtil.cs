using Microsoft.Win32;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Modding.Accessories.AccAddon;
using SCS_Mod_Helper.Utils;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Windows.UI;

namespace SCS_Mod_Helper.Modding.Accessories;
public static class AccessoryDataUtil {

	public static string GetInitialPath(params string[] paths) {
		string? currentPath = null;
		foreach (var path in paths) {
			if (string.IsNullOrEmpty(path))
				continue;
			currentPath = path;
			break;
		}
		var projectLocation = Instances.ProjectLocation;
		if (currentPath != null) {
			currentPath = currentPath.Replace('/', '\\');
			if (!currentPath.StartsWith('\\'))
				currentPath = '\\' + currentPath;
			currentPath = projectLocation + currentPath;
			while (currentPath.StartsWith(projectLocation) && currentPath.Length > projectLocation.Length) {
				var parent = new DirectoryInfo(currentPath).Parent;
				if (parent != null && parent.Exists) {
					return parent.FullName;
				}
				currentPath = parent!.FullName;
			}
		}
		var historyModel = AccAddonHistory.Default.ChooseModelHistory;
		if (!string.IsNullOrEmpty(historyModel) && historyModel.StartsWith(projectLocation)) {
			if (Directory.Exists(historyModel)) {
				return historyModel;
			}
		}
		return Path.Combine(projectLocation, "vehicle");
	}

	public static void CheckDragFileType(DragEventArgs e) {
		var files = (string[])e.Data.GetData(DataFormats.FileDrop);
		if (files.Length == 1) {
			var file = new FileInfo(files[0]);
			switch (file.Extension) {
				case ".tobj":
				case ".dds":
				case ".tga":
				case ".jpg":
				case ".jpeg":
				case ".png":
					e.Effects = DragDropEffects.Copy;
					return;
				default:
					break;
			}
		}
		e.Effects = DragDropEffects.None;
	}

	//图标
	public static string? ChooseIcon(Window window, out string? iconPath) {
		iconPath = null;
		try {
			string projectLocation = Instances.ProjectLocation;
			if (projectLocation.Length == 0)
				throw new(Util.GetString("MessageProjectLocationFirst"));
			var pathAcc = Paths.AccessoryIconDir();
			var fileDialog = new OpenFileDialog {
				Multiselect = false,
				DefaultDirectory = pathAcc,
				DefaultExt = "tga",
				Title = Util.GetString("DialogTitleChooseIcon"),
				Filter = Util.GetFilter("DialogFilterChooseIcon")
			};
			if (!fileDialog.InitialDirectory.StartsWith(pathAcc))
				fileDialog.InitialDirectory = pathAcc;
			if (!Directory.Exists(pathAcc) && Directory.CreateDirectory(pathAcc) == null)//确保accessory文件夹存在
				throw new(Util.GetString("MessageCreateAccessoryFail"));
			if (fileDialog.ShowDialog() != true)
				return null;
			return CheckIconPath(window, fileDialog.FileName, out iconPath);
		} catch (Exception ex) {
			MessageBox.Show(window, ex.Message);
		}
		return null;
	}
	public static string? CheckIconPath(Window window, string path, out string? iconPath) {
		iconPath = null;
		string projectLocation = Instances.ProjectLocation;
		var pathAcc = Paths.AccessoryIconDir();
		if (!path.StartsWith(projectLocation) || path.Length == projectLocation.Length) //必须在项目内
			throw new(Util.GetString("MessageIconOutsideProject"));
		var iconFile = new DirectoryInfo(path);
		string pathCheck = path;
		pathCheck = pathCheck.Replace(projectLocation, "");
		if (pathCheck.Split(' ', '(', ')').Length > 1) //路径或文件名不能有空格或括号
			throw new(Util.GetString("MessageIconInvalidChar"));
		var siiIconLocation = CheckIconFileExistence(window, pathAcc, iconFile);
		if (siiIconLocation == null)
			return null;
		siiIconLocation = siiIconLocation.Replace(pathAcc, "");
		siiIconLocation = siiIconLocation.Replace('\\', '/');
		siiIconLocation = siiIconLocation[..^4];
		if (siiIconLocation.StartsWith('/'))
			siiIconLocation = siiIconLocation[1..];
		iconPath = iconFile.FullName;
		return siiIconLocation;
	}

	/// <summary>
	/// 检查图标文件是否有对应的tobj和mat。
	/// 会从图标所在位置以及Accessory文件夹检查
	/// </summary>
	/// <returns>mat文件的绝对路径</returns>
	public static string? CheckIconFileExistence(Window window, string pathAcc, DirectoryInfo iconFile) {//如果图标不在accessory文件夹内，就只能选择accessory文件夹生成
		string projectLocation = Instances.ProjectLocation;

		var iconParent = iconFile.Parent!.FullName;

		var deExt = iconFile.Name[..^4];
		var matName = deExt + ".mat";
		var tobjName = deExt + ".tobj";

		var iconLocationExist = File.Exists(iconParent + '\\' + matName) && File.Exists(iconParent + '\\' + tobjName);
		var accExist = File.Exists(pathAcc + '\\' + matName) && File.Exists(pathAcc + '\\' + tobjName);

		string siiIconLocation;
		if (iconLocationExist)
			siiIconLocation = iconParent + '\\' + matName;
		else if (accExist)
			siiIconLocation = pathAcc + '\\' + matName;
		else {
			var accessoryLocation = Paths.AccessoryIconDir();
			var notInAcc = !iconFile.FullName.StartsWith(accessoryLocation);
			if (pathAcc.EndsWith('\\'))
				pathAcc = pathAcc[..^1];
			var atAcc = iconParent == pathAcc;

			var dialog = new IconDefWIndow(notInAcc, atAcc) {
				Owner = window
			};

			string genLocation;
			var result = dialog.ShowDialog();
			if (result == true) {
				genLocation = dialog.CreateOnAccessory ? pathAcc : iconParent;
			} else
				return null;
			{
				string iconLocation;
				if (notInAcc)
					iconLocation = iconFile.FullName.Replace(projectLocation, "");
				else
					iconLocation = iconFile.Name;
				iconLocation = iconLocation.Replace('\\', '/');
				using StreamWriter sw = new(genLocation + '\\' + tobjName);
				sw.WriteLine($"map\t2d\t{iconLocation}");
				sw.WriteLine("addr\tclamp_to_edge\tclamp_to_edge");
			}
			siiIconLocation = genLocation + '\\' + matName;
			{
				using StreamWriter sw = new(genLocation + '\\' + matName);
				sw.WriteLine("material: \"ui\"");
				sw.WriteLine("{");
				sw.WriteLine($"\ttexture: \"{tobjName}\"");
				sw.WriteLine($"\ttexture_name: \"texture\"");
				sw.WriteLine("}");
			}
		}
		return siiIconLocation;
	}

	public static void CreateTobjFile(string imagePath) {
		DirectoryInfo imageFile = new(imagePath);
		var name = imageFile.Name;
		imagePath = imagePath.Replace(imageFile.Extension, ".tobj");

		using StreamWriter sw = new(imagePath);
		sw.WriteLine($"map\t2d\t{name}");
		sw.WriteLine("addr\tclamp_to_edge\tclamp_to_edge");
	}



	public static BitmapSource? LoadModelIconByIconName(string iconName) {
		if (iconName.Length == 0)
			return null;
		var projectLocation = Instances.ProjectLocation;
		iconName = iconName.Replace('/', '\\');
		var iconPath = Paths.AccessoryIconDir(projectLocation, iconName);
		var iconFile = iconPath + ".tga";
		if (File.Exists(iconFile))
			return LoadModelIcon(iconFile);
		iconFile = iconPath + ".dds";
		if (File.Exists(iconFile))
			return LoadModelIcon(iconFile);
		iconFile = iconPath + ".tobj";
		if (File.Exists(iconFile)) {
			using StreamReader sr = new(iconFile);
			string? line;
			while ((line = sr.ReadLine()?.Trim()) != null) {
				int start = line.IndexOf("/mate");
				if (start != -1) {
					int end = line.IndexOf(".tga");
					if (end == -1)
						end = line.IndexOf(".dds");
					end += 4;
					iconFile = line[start..end];
					iconFile = iconFile.Replace('/', '\\');
					iconFile = projectLocation + iconFile;
					return LoadModelIcon(iconFile);
				}
			}
		}
		return null;
	}

	public static BitmapSource? LoadModelIcon(string? iconPath) {
		if (iconPath == null || !File.Exists(iconPath))
			return null;
		return Util.LoadPfimImage(iconPath);
	}

	public static string? ChooseRope() => ChooseMaterial(Util.GetString("DialogTitleChooseRope"));
	public static string? ChoosePatch() => ChooseMaterial(Util.GetString("DialogTitleChoosePatch"));
	private static string? ChooseMaterial(string title) {
		string projectLocation = Instances.ProjectLocation;
		if (projectLocation.Length == 0)
			throw new(Util.GetString("MessageProjectLocationFirst"));
		var fileDialog = new OpenFileDialog {
			Multiselect = false,
			DefaultDirectory = projectLocation,
			DefaultExt = "mat",
			Title = title,
			Filter = Util.GetFilter("DialogFilterChooseMaterial"),
		};
		if (!fileDialog.InitialDirectory.StartsWith(projectLocation))
			fileDialog.InitialDirectory = projectLocation;
		if (fileDialog.ShowDialog() != true)
			return null;
		string path = fileDialog.FileName;
		if (!path.StartsWith(projectLocation) || path.Length == projectLocation.Length)
			throw new(Util.GetString("MessageModelOutsideProject"));
		if (path.EndsWith(".tga") || path.EndsWith(".dds")) {
			var iconFile = new DirectoryInfo(path);
			string pathCheck = path;
			pathCheck = pathCheck.Replace(projectLocation, "");
			if (pathCheck.Split(' ', '(', ')').Length > 1) //路径或文件名不能有空格或括号
				throw new(Util.GetString("MessageIconInvalidChar"));
			var matLocation = CheckRopeMatExistence(iconFile);
			if (matLocation == null)
				return null;
			matLocation = matLocation.Replace(projectLocation, "");
			return matLocation.Replace('\\', '/');
		} else if (path.EndsWith(".mat")) {
			string inProjectPath = path.Replace(projectLocation, "");
			return inProjectPath.Replace('\\', '/');
		} else
			throw new(Util.GetString("MessageErrorChooseMaterial"));
	}

	private static string? CheckRopeMatExistence(DirectoryInfo iconFile) {
		var deExt = iconFile.FullName[..^4];
		var matPath = deExt + ".mat";
		var tobjPath = deExt + ".tobj";

		if (File.Exists(matPath) && File.Exists(tobjPath))
			return matPath;
		else {
			{
				using StreamWriter sw = new(tobjPath);
				sw.WriteLine($"map	2d	{iconFile.Name}");
				sw.WriteLine("addr	clamp_to_edge	clamp_to_edge");
			}
			tobjPath = new FileInfo(tobjPath).Name;
			{
				using StreamWriter sw = new(matPath);
				sw.WriteLine("material: \"eut2.dif.spec.shadow\"");
				sw.WriteLine("{");
				sw.WriteLine($"\ttexture: \"{tobjPath}\"");
				sw.WriteLine($"\ttexture_name: \"texture_base\"");
				sw.WriteLine("}");
			}
		}
		return matPath;

	}
}

public class StringResItem: BaseBinding {

	public StringResItem(string header, string tag, string? tooltip = null) {
		IsSeparator = false;
		mHeader = header;
		mTag = tag;
		mTooltip = tooltip;
	}

	public static StringResItem Head() => new(Util.GetString("MenuResTip"), "head") { IsEnabled = false };

	public static StringResItem Separator() => new();

	public static StringResItem Empty() => new(Util.GetString("StatusEmpty"), "empty") { IsEnabled = false };

	public static StringResItem OpenLocalization() => new(Util.GetString("MenuResOption"), "openLocalization");

	private StringResItem() {
		IsSeparator = true;
		mHeader = "";
		mTag = "";
	}

	private readonly bool IsSeparator;

	public Visibility TextBlockVisibility => IsSeparator ? Visibility.Collapsed : Visibility.Visible;
	public Visibility SeparatorVisibility => IsSeparator ? Visibility.Visible : Visibility.Collapsed;

	private bool mIsEnabled = true;
	public bool IsEnabled {
		get => mIsEnabled;
		set {
			mIsEnabled = value;
			InvokeChange();
		}
	}

	private string mHeader;

	public string Header {
		get => mHeader;
		set {
			mHeader = value;
			InvokeChange();
		}
	}

	private string mTag;
	public string Tag {
		get => mTag;
		set {
			mTag = value;
			InvokeChange();
		}
	}

	private string? mTooltip;

	public string? Tooltip {
		get => mTooltip;
		set {
			mTooltip = value;
			InvokeChange();
		}
	}
}
