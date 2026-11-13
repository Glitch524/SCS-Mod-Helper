using Microsoft.Win32;
using SCS_Mod_Helper.Class;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Language;
using SCS_Mod_Helper.Setting;
using SCS_Mod_Helper.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace SCS_Mod_Helper.ConverterPix; 
public partial class ConverterPixBinding(Func<IBaseVisual> visualFunc, Action<PIXFile> listBoxScrollToTop): WindowBinding(visualFunc) {
	private readonly Action<PIXFile> ScrollToFile = listBoxScrollToTop;

	private void ScrollToTop() => ScrollToFile(CurrentPack!.FileList[0]);

	public bool UseCustomDestPath {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;

	private string? mCustomExportPath = null;
	public string CustomExportPath {
		get {
			if (mCustomExportPath == null) {
				if (string.IsNullOrEmpty(Instances.HistoryPixExportPath))
					mCustomExportPath = Instances.ProjectLocation;
				else
					mCustomExportPath = Instances.HistoryPixExportPath;
			}
			return mCustomExportPath;
		}
		set {
			mCustomExportPath = value;
			Instances.HistoryPixExportPath = value;
			InvokeChange();
		}
	}

	public string ExportPath => UseCustomDestPath ? CustomExportPath : Instances.ProjectLocation;

	public ICommand ChooseExportPathCommand => new RelayCommand<object>(_ => ChooseCustomExportPath());
	private void ChooseCustomExportPath() {
		OpenFolderDialog openFolderDialog = new() {
			Multiselect = false,
			InitialDirectory = CustomExportPath,

		};
		if (openFolderDialog.ShowDialog() == true) {
			CustomExportPath = openFolderDialog.FolderName;
		}
	}

	public ObservableCollection<PixPackage> PackList { get; } = [new()];

	public PixPackage? CurrentPack {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = null;

	public ICommand AddPackCommand => new RelayCommand<object>(_ => AddPack());
	public void AddPack() {
		PixPackage newPack = new();
		PackList.Add(newPack);
		CurrentPack = newPack;
	}

	public ICommand ClosePackCommand => new RelayCommand<PixPackage>(ClosePack);
	public void ClosePack(PixPackage? pack) {
		if (pack == null)
			return;
		PackList.Remove(pack);
		if (PackList.Count == 0) {
			AddPack();
		}
	}

	private ObservableCollection<PackDir> CurrentPathList => CurrentPack!.PathList;
	private ObservableCollection<List<PackDir>> CurrentHistoryUndoList => CurrentPack!.HistoryUndoList;
	private ObservableCollection<PIXFile> CurrentFileList => CurrentPack!.FileList;
	private PIXFile? CurrentSelectedFile { 
		get => CurrentPack?.SelectedFile;
		set => CurrentPack?.SelectedFile = value;
	}
	private string CurrentPackPath => CurrentPack!.PackPath;
	private string CurrentPackDir => CurrentPack!.PackDir;

	public ICommand ChoosePackCommand => new RelayCommand<object>(_ => ChoosePack());

	private void ChoosePack() {
		string defaultDir;
		if (!string.IsNullOrEmpty(Instances.ETS2GamePath))
			defaultDir = Instances.ETS2GamePath;
		else if (!string.IsNullOrEmpty(Instances.ATSGamePath))
			defaultDir = Instances.ATSGamePath;
		else
			defaultDir = Instances.CommonModPath;

		string initialDir;
		if (string.IsNullOrEmpty(History.Default.PixPackPath))
			initialDir = defaultDir;
		else
			initialDir = History.Default.PixPackPath;
		OpenFileDialog openFileDialog = new() {
			Filter = Resource.GetFilter("DialogFilterMod"),
			Multiselect = false,
			DefaultDirectory = defaultDir,
			InitialDirectory = initialDir
		};
		if (openFileDialog.ShowDialog() == true) {
			if (openFileDialog.FileName.StartsWith(Instances.ETS2GamePath) || openFileDialog.FileName.StartsWith(Instances.ATSGamePath)) {
				if (openFileDialog.SafeFileName == "def.scs" && History.Default.PixOpenDefNotice) {
					NotShowableMessageBox messageBox = new(GetString("MessageOpenDef"), MessageBoxButton.OK, true) {
						Owner = BindingWindow
					};
					if (messageBox.ShowDialog() == true) {
						if (messageBox.DontShowAgain) {
							History.Default.PixOpenDefNotice = false;//下面也会保存history所以这里没有写
						}
					}
				}
			}
			CurrentPack!.PackPath = openFileDialog.FileName;
			CurrentPack!.PackName = openFileDialog.SafeFileName;
			History.Default.PixPackPath = Directory.GetParent(CurrentPack.PackPath)!.FullName;
			History.Default.Save();
			DirStart(openFileDialog.SafeFileName);
		}
	}



	public ICommand OpenFileCommand => new RelayCommand<object>(DirOpen);
	public void OpenSelectedDir() => DirOpen(CurrentSelectedFile);
	public void DirOpen(object? parameter) {
		if (parameter == null)
			return;
		if (parameter is PIXFile pixFile) {
			var filename = pixFile.Filename;
			if (pixFile.IsDir) {
				HistoryAdd();
				CurrentPathList.Add(new PackDir(filename));
				_ = ReadDir();
			}
		}
	}

	public ICommand FolderBackCommand => new RelayCommand<object>(_ => FolderBack());
	public void FolderBack() {
		if (CurrentPathList.Count > 1) {
			FolderBack(CurrentPathList.Count - 2);
		}
	}

	public void FolderBack(int index) {
		HistoryAdd();
		string focusFile = CurrentPathList[index + 1].DirName;
		while (CurrentPathList.Count > index + 1) {
			CurrentPathList.RemoveAt(index + 1);
		}
		_ = ReadDir(focusFile);
	}

	private void UpdateDirList(string linkPath) {
		if (CurrentPathList.Count > 1) {
			var baseDir = CurrentPathList[0];
			CurrentPathList.Clear();
			CurrentPathList.Add(baseDir);
		}
		while ((linkPath = linkPath[..linkPath.LastIndexOf('/')]).Length > 0) {
			CurrentPathList.Insert(1, new(linkPath));
		}
	}


	private void HistoryAdd() {
		CurrentHistoryUndoList.Add([.. CurrentPathList]);
	}
	private void HistoryClear() {
		CurrentHistoryUndoList.Clear();
	}
	public ICommand UndoCommand => new RelayCommand<object>(_ => Undo());
	public void Undo() {//返回时直接将dir放进dirlist里，然后readdir
		var last = CurrentHistoryUndoList.Last();
		CurrentHistoryUndoList.RemoveAt(CurrentHistoryUndoList.Count - 1);
		CurrentPathList.Clear();
		foreach (var l in last) {
			CurrentPathList.Add(l);
		}
		_ = ReadDir();
	}

	public void DirStart(string packName) {
		CurrentPathList.Clear();
		HistoryClear();
		CurrentPathList.Add(new("/", packName));
		_ = ReadDir();
	}

	public async Task ReadDir(string? focusPath = null) {
		string args = $"-b \"{CurrentPackPath}\" -listdir \"{CurrentPackDir}\"";

		using Process p = CallPix(args);
		p.ErrorDataReceived += (s, e) => {
			if (e.Data != null)
				Debug.WriteLine("[err] " + e.Data);
		};
		p.Start();
		p.BeginErrorReadLine();

		var tcs = new TaskCompletionSource<(bool success, string? reason)>();
		PIXFile? focusFile = null;
		var batchList = new List<PIXFile>(50);

		await Task.Run(async () => {
			p.Start();
			try {
				string? line;
				while ((line = await p.StandardOutput.ReadLineAsync()) != null) {
					if (line == null || line.Trim().StartsWith('*') || line.Length == 0)
						continue;
					if (line.StartsWith("<error>")) {
						var reason = line[(line.IndexOf(':') + 1)..];
						tcs.TrySetResult((false, reason));
						break;
					}
					if (line.Contains("Done"))
						break;
					if (line.StartsWith("[D]")) {
						string path = line[4..];
						var file = new PIXFile(path, true);
						if (focusPath == path)
							focusFile = file;
						batchList.Add(file);
					} else if (line.StartsWith("[F]")) {
						batchList.Add(new PIXFile(line[4..], false));
					}
				}
			} finally {
				if (!p.HasExited) {
					try {
						p.Kill();
					} catch {

					}
				}
				await p.WaitForExitAsync();
			}
			tcs.TrySetResult((true, null));
		});

		var (success, reason) = await tcs.Task;
		if (success) {
			CurrentFileList.Clear();
			foreach (var f in batchList) {
				CurrentFileList.Add(f);
			}
			CurrentSelectedFile = focusFile;
			if (focusFile == null)
				ScrollToTop();
			else
				ScrollToFile(focusFile);
		} else {
			MessageBox.Show(GetString("MessageReadFileFailed") + ": " + reason);
		}
	}




	private static Process CallPix(string args) {
		Process p = new();
		p.StartInfo.FileName = Instances.ConverterPixExe!;
		p.StartInfo.Arguments = args;
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.RedirectStandardInput = true;
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.RedirectStandardError = true;
		p.StartInfo.CreateNoWindow = true;
		return p;
	}

	public ICommand ExtractCommand => new RelayCommand<PIXFile>(pixFile => _ = ExtractFile(pixFile ?? CurrentSelectedFile));
	public async Task ExtractFile(PIXFile? pixFile) {
		if (pixFile == null)
			return;
		StringBuilder args = new($"-b \"{CurrentPackPath}\"");
		if (MountPackages && PackList.Count > 1) {
			foreach (var pack in PackList) {
				if (pack == CurrentPack || pack.PackPath.Length == 0)
					continue;
				args.Append($" -b \"{pack.PackPath}\"");
				
			}
		}
		args.Append($" -e \"{ExportPath}\"");
		int type = 0;
		if (pixFile.IsDir) {
			args.Append($" -extract_d \"{pixFile.Filename}\"");
		} else {
			switch (pixFile.Extension.ToLower()) {
				case "pmd":
				case "pmg":
				case "pma"://可能出现没有pmd的情况，需要找pmd
				case "pmc":
					type = 1;
					var name = pixFile.Filename;
					name = name[..^4];
					args.Append($" -m \"{name}\"");
					if (SearchAnim) {
						var animList = SearchAnimExe(name);
						if (animList.Count > 0)
							foreach (string anim in animList) {
								args.Append($" \"{anim}\"");
								break;
							}
					}
					break;
				case "tobj":
					type = 2;
					args.Append($" -t \"{pixFile.Filename}\"");
					break;
				default:
					args.Append($" -extract_f \"{pixFile.Filename}\"");
					break;
			}
		}
		Process p = CallPix(args.ToString());
		p.Start();

		string? line;
		string errorHeader = "<error>", warningHeader = "<warning>";
		bool success = true;
		List<string> errors = [], warnings = [];
		while ((line = await p.StandardOutput.ReadLineAsync()) != null) {
			Debug.WriteLine(line);

			if (line.Trim().StartsWith('*') || line.Length == 0)
				continue;
			if (line.StartsWith(errorHeader)) {
				success = false;
				line = line[errorHeader.Length..];
				errors.Add(line);
			} else if (line.StartsWith(warningHeader)) {
				line = line[warningHeader.Length..].Trim();
				warnings.Add(line);
			}
		}
		await p.WaitForExitAsync();
		p.Close();
		if (success) {
			string osFilename = pixFile.Filename.Replace('/', '\\');
			string filePath = ExportPath + osFilename;
			if (OpenExtracted) {
				if (type == 1)
					filePath = filePath[..^4] + ".pim";
				if (File.Exists(filePath)) {
					Process.Start(new ProcessStartInfo {
						FileName = "explorer.exe",
						Arguments = $"/select,\"{filePath}\"",
						UseShellExecute = true
					});
					return;
				}
			}

			string key = type switch {
				1 => "MessageExtractedModel",
				2 => "MessageExtractedTobj",
				_ => "MessageExtractedFile",
			};
			StringBuilder resultString = new();
			resultString.AppendLine(GetString(key));
			resultString.Append(filePath);
			if (warnings.Count > 0) {
				List<string> failFiles = [];
				string wStart = "[tobj] ", wEnd = ": Unable to load!";
				foreach (var warning in warnings) {
					if (warning.StartsWith(wStart) && warning.EndsWith(wEnd)) {
						string file = warning.Substring(wStart.Length, warning.Length - wStart.Length - wEnd.Length);
						if (!failFiles.Contains(file))
							failFiles.Add(file);
					}
				}
				resultString.Append("\n\n");
				resultString.AppendLine(GetQuantityString("MessageExtractWarning1", failFiles.Count));
				foreach(var file in failFiles) {
					resultString.AppendLine(file);
				}
				resultString.AppendLine(GetQuantityString("MessageExtractWarning2", failFiles.Count));
			}
			MessageBox.Show(BindingWindow, resultString.ToString(), GetString("TitleExtractResult"));
		} else {
			string resultMessage = $"{GetString("MessageExtractFail")}\n{string.Join(Environment.NewLine, errors)}";
			MessageBox.Show(BindingWindow, resultMessage, GetString("TitleExtractResult"));
		}
	}

	public bool SearchAnim {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;

	public bool OpenExtracted {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;

	public List<string> SearchAnimExe(string modelPath) {
		string args = $"-b \"{CurrentPackPath}\" --find-model-animations \"{modelPath}\"";

		Process p = CallPix(args);

		p.Start();
		List<string> anims = [];
		string? line;
		while ((line = p.StandardOutput.ReadLine()) != null) {
			if (line.StartsWith('*') || line.Length == 0)
				continue;
			if (line.EndsWith(".pma")) {
				anims.Add(line[..^4]);
			}
		}
		Debug.WriteLine(p.StandardOutput.ReadToEnd());
		p.WaitForExit();
		p.Close();
		return anims;
	}

	public bool MountPackages {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = false;

	public ICommand ExtractAllCommand => new RelayCommand<object>(_ => ExtractAll());

	private void ExtractAll() {
		var extractTask = new ExtractAllTask(CurrentPackPath, ExportPath) {
			Owner = BindingWindow
		};
		extractTask.ShowDialog();
	}
}

public class PackDir(string dirName, string? title): BaseBinding {

	public PackDir(string dirName) : this(dirName, null) {
	}

	private string mDirName = dirName;
	public string DirName {
		get => mDirName;
		set {
			mDirName = value;
			InvokeChange();
		}
	}

	public string Title {
		get {
			if (title != null)
				return title;
			int lastSlash = mDirName.LastIndexOf('/');
			if (lastSlash >= 0)
				return mDirName[(lastSlash + 1)..];
			return mDirName;
		}
	}

	public override string ToString() => $"PackDir Dir={DirName}";
}

public class PixPackage: BaseBinding {
	public string PackPath {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = string.Empty;

	public string PackName {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = GetString("TitlePackName");

	private readonly ObservableCollection<PackDir> mPathList = [];

	public ObservableCollection<PackDir> PathList => mPathList;
	public string PackDir => PathList.LastOrDefault()?.DirName ?? "/";

	public ObservableCollection<List<PackDir>> HistoryUndoList { get; } = [];//记录切换位置之前的所有dir

	private readonly ObservableCollection<PIXFile> mFileList = [];

	public ObservableCollection<PIXFile> FileList => mFileList;

	public PIXFile? SelectedFile {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = null;
}