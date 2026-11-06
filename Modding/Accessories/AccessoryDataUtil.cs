using Microsoft.Win32;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Modding.Accessories;
public static class AccessoryDataUtil {

	public static string? ChooseModelPath(ChooseModelType chooseType, string? currentPath, VehicleType vehicleType = VehicleType.Truck) {
		var projectLocation = Instances.ProjectLocation;
		if (projectLocation.Length == 0)
			throw new(Util.GetString("MessageProjectLocationFirst"));
		string title, filter, defaultExt;
		switch (chooseType) {
			case ChooseModelType.Model:
			case ChooseModelType.ExtModel:
				title = Util.GetString("DialogTitleChooseModel");
				filter = Util.GetFilter("DialogFilterChooseModel");
				defaultExt = "pmd";
				break;
			case ChooseModelType.ModelUK:
			case ChooseModelType.ExtModelUK:
				title = Util.GetString("DialogTitleChooseModelUK");
				filter = Util.GetFilter("DialogFilterChooseModel");
				defaultExt = "pmd";
				break;
			case ChooseModelType.Coll:
				title = Util.GetString("DialogTitleChooseColl");
				filter = Util.GetFilter("DialogFilterChooseColl");
				defaultExt = "pmc";
				break;
			default:
				throw new ArgumentException("wrong type");
		}

		if (string.IsNullOrEmpty(Instances.HistoryModelPath) || !Instances.HistoryModelPath.StartsWith(Instances.ProjectLocation)) {
			Instances.HistoryModelPath = Paths.VehicleUpgradePath(vehicleType);
		}
		var defaultDir = Instances.HistoryModelPath;

		string initialPath;
		if (string.IsNullOrEmpty(currentPath)) {
			initialPath = defaultDir;
		} else {
			currentPath = currentPath.Replace('/', '\\');
			if (!currentPath.StartsWith('\\'))
				currentPath = '\\' + currentPath;
			currentPath = projectLocation + currentPath;
			DirectoryInfo currentDir = new(currentPath);
			currentDir = currentDir.Parent!;
			while (!currentDir.Exists && currentDir.FullName != projectLocation) {
				currentDir = currentDir.Parent!;
			}
			initialPath = currentDir.FullName;
		}

		var fileDialog = new OpenFileDialog {
			Multiselect = false,
			DefaultDirectory = defaultDir,
			DefaultExt = defaultExt,
			Title = title,
			Filter = filter,
			InitialDirectory = initialPath
		};
		if (fileDialog.ShowDialog() != true)
			return null;
		string path = fileDialog.FileName;
		if (!path.StartsWith(projectLocation))
			throw new(Util.GetString("MessageModelOutsideProject"));
		if (chooseType == ChooseModelType.Coll) {
			if (!path.EndsWith(".pic") && !path.EndsWith(".pmc"))
				throw new(Util.GetString("MessageInvalidExtColl"));
		} else if (!path.EndsWith(".pim") && !path.EndsWith(".pmd")) {
			throw new(Util.GetString("MessageInvalidExt"));
		}
		Instances.HistoryModelPath = Directory.GetParent(path)!.FullName;
		return path;
	}

	//图标
	public static string? ChooseIconPath(Window window) {
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
			return fileDialog.FileName;
		} catch (Exception ex) {
			MessageBox.Show(window, ex.Message);
		}
		return null;
	}
	public static string? CheckIconPath(Window window, string path, out string? iconPath) {
		iconPath = null;
		try {
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
		} catch (Exception e) {
			MessageBox.Show(window, e.Message);
		}
		return null;
	}

	/// <summary>
	/// 检查图标文件是否有对应的tobj和mat。
	/// 会从图标所在位置以及Accessory文件夹检查
	/// </summary>
	/// <returns>mat文件的绝对路径</returns>
	private static string? CheckIconFileExistence(Window window, string pathAcc, DirectoryInfo iconFile) {//如果图标不在accessory文件夹内，就只能选择accessory文件夹生成
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
		var iconPath = Paths.AccessoryIconDir(iconName);
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


	public static string? ChooseTexPath(Window window, ChooseTexType texType, string currentPath, VehicleType vehicleType = VehicleType.Truck) {//可选普通格式的图片，但是提示仅用于预览，后期需要自己转换
		try {
			var projectLocation = Instances.ProjectLocation;
			if (projectLocation.Length == 0)
				throw new(Util.GetString("MessageProjectLocationFirst"));
			string title;
			switch(texType) {
				case ChooseTexType.Rope:
					title = Util.GetString("DialogTitleChooseRope");
					break;
				case ChooseTexType.Patch:
					title = Util.GetString("DialogTitleChoosePatch");
					break;
				case ChooseTexType.Paintjob:
				case ChooseTexType.AccTex:
					title = Util.GetString("TitlePaintJobTexture");
					break;
				case ChooseTexType.BaseTexOvr:
					title = Util.GetString("TitleBaseTextureOverride");
					break;
				case ChooseTexType.FlakeNoise:
					title = Util.GetString("TitleFlakeNoise");
					break;
				default:
					return null;
			}
			string defaultDir, initialDir;
			if (texType == ChooseTexType.Rope) {
				defaultDir = projectLocation;
				initialDir = Paths.RopeDir();
			} else if (texType == ChooseTexType.FlakeNoise) {
				defaultDir = projectLocation;
				initialDir = projectLocation;
			} else {
				if (string.IsNullOrEmpty(Instances.HistoryTexturePath) || !Instances.HistoryTexturePath.StartsWith(Instances.ProjectLocation)) {
					Instances.HistoryTexturePath = Paths.VehicleUpgradePath(vehicleType);
				}
				defaultDir = Instances.HistoryTexturePath;

				if (string.IsNullOrEmpty(currentPath)) {
					initialDir = defaultDir;
				} else {
					currentPath = currentPath.Replace('/', '\\');
					if (!currentPath.StartsWith('\\'))
						currentPath = '\\' + currentPath;
					currentPath = projectLocation + currentPath;
					DirectoryInfo currentDir = new(currentPath);
					currentDir = currentDir.Parent!;
					while (!currentDir.Exists && currentDir.FullName != projectLocation) {
						currentDir = currentDir.Parent!;
					}
					initialDir = currentDir.FullName;
				}
			}
			var fileDialog = new OpenFileDialog {
				Multiselect = false,
				Title = title,
				DefaultDirectory = defaultDir,
				InitialDirectory = initialDir,
				DefaultExt = "tobj",
				Filter = Util.GetFilter("DialogFilterTextureFile"),
			};
			if (fileDialog.ShowDialog() != true)
				return null;
			if (texType == ChooseTexType.Paintjob
				|| texType == ChooseTexType.Patch
				|| texType == ChooseTexType.BaseTexOvr
				|| texType == ChooseTexType.AccTex) {
				Instances.HistoryTexturePath = fileDialog.FileName;
			}
			return fileDialog.FileName;
		} catch (Exception e) {
			MessageBox.Show(window, e.Message);
		}
		return null;
	}

	public static string? ChooseRope() => ChooseMaterial(Util.GetString("DialogTitleChooseRope"));
	public static string? ChoosePatch() => ChooseMaterial(Util.GetString("DialogTitleChoosePatch"));
	private static string? ChooseMaterial(string title) {//rope需要保留mat扩展名
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
			var matLocation = CheckMatExistence(iconFile);
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

	private static string? CheckMatExistence(DirectoryInfo iconFile) {
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

	public static void PreviewDragOver(object sender, DragEventArgs e, Func<ChooseFileType> fileDragOver) {
		if (e.Data.GetDataPresent(DataFormats.Text) && sender is TextBox) {
			e.Effects = DragDropEffects.Copy;
			e.Handled = true;
			return;
		}
		if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
			var files = (string[])e.Data.GetData(DataFormats.FileDrop);
			if (files.Length == 1) {
				var file = new FileInfo(files[0]);
				if (file.FullName.StartsWith(Instances.ProjectLocation)) {
					CheckDragFileType(e, fileDragOver());
					e.Handled = true;
					return;
				}
			}
		}
		e.Effects = DragDropEffects.None;
		e.Handled = true;
	}

	public static void CheckDragFileType(DragEventArgs e, ChooseFileType fileType) {
		var files = (string[])e.Data.GetData(DataFormats.FileDrop);
		if (files.Length == 1) {
			var file = new FileInfo(files[0]);
			switch (file.Extension.ToLower()) {
				case ".tobj":
				case ".dds":
				case ".tga":
				case ".jpg":
				case ".jpeg":
				case ".png":
					if (fileType == ChooseFileType.Texture)
					e.Effects = DragDropEffects.Copy;
					return;
				case ".pmd":
				case ".pim":
				case ".pmg":
				case ".pit":
					if (fileType == ChooseFileType.ModelFile)
						e.Effects = DragDropEffects.Copy;
					return;
				case ".pmc":
				case ".pic":
					if (fileType == ChooseFileType.CollisionFile)
						e.Effects = DragDropEffects.Copy;
					return;
				default:
					break;
			}
		}
		e.Effects = DragDropEffects.None;
	}
}

public enum ChooseModelType { Model, ExtModel, ModelUK, ExtModelUK, Coll }
public enum ChooseTexType { Rope, Patch, Paintjob, BaseTexOvr, FlakeNoise, AccTex }
public enum ChooseFileType { Texture, ModelFile, CollisionFile }