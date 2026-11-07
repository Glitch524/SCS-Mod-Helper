using Microsoft.Win32;
using SCS_Mod_Helper.Class.Base;
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

	private void ScrollToTop() => ScrollToFile(FileList[0]);

	public string PackPath {
		get;
		set {
			field = value;
			InvokeChange();
		}
	} = string.Empty;

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
			Filter = Util.GetFilter("DialogFilterMod"),
			Multiselect = false,
			DefaultDirectory = defaultDir,
			InitialDirectory = initialDir
		};
		if (openFileDialog.ShowDialog() == true) {
			PackPath = openFileDialog.FileName;
			History.Default.PixPackPath = Directory.GetParent(PackPath)!.FullName;
			History.Default.Save();
			DirStart(openFileDialog.SafeFileName);
		}
	}

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


	private readonly List<List<PackDir>> HistoryUndoList = [];//记录切换位置之前的所有dir
	public int UndoCount => HistoryUndoList.Count;

	private void HistoryAdd() {
		HistoryUndoList.Add([.. DirList]);
		InvokeChange(nameof(UndoCount));
	}
	private void HistoryClear() {
		HistoryUndoList.Clear();
		InvokeChange(nameof(UndoCount));
	}


	public ICommand UndoCommand => new RelayCommand<object>(_ => Undo());
	public void Undo() {//返回时直接将dir放进dirlist里，然后readdir
		var last = HistoryUndoList.Last();
		HistoryUndoList.RemoveAt(HistoryUndoList.Count - 1);
		InvokeChange(nameof(UndoCount));
		DirList.Clear();
		foreach(var l in last) {
			DirList.Add(l);
		}
		_ = ReadDir();
	}

	private readonly ObservableCollection<PackDir> mDirList = [];

	public ObservableCollection<PackDir> DirList => mDirList;
	public string PackDir => DirList.LastOrDefault()?.DirName ?? "/";

	public void DirStart(string packName) {
		DirList.Clear();
		HistoryClear();
		DirList.Add(new("/", packName));
		_ = ReadDir();
	}

	public ICommand OpenFileCommand => new RelayCommand<object>(DirOpen);
	public void OpenSelectedDir() => DirOpen(SelectedFile);
	public void DirOpen(object? parameter) {
		if (parameter == null)
			return;
		if (parameter is PIXFile pixFile) {
			var filename = pixFile.Filename;
			if (pixFile.IsDir) {
				HistoryAdd();
				DirList.Add(new PackDir(filename));
				_ = ReadDir();
			}
		}
	}

	public ICommand FolderBackCommand => new RelayCommand<object>(_ => FolderBack());
	public void FolderBack() {
		if (DirList.Count > 1) {
			DirBack(DirList.Count - 2);
		}
	}

	public void DirBack(int index) {
		HistoryAdd();
		string focusFile = DirList[index + 1].DirName;
		while (DirList.Count > index + 1) {
			DirList.RemoveAt(index + 1);
		}
		_ = ReadDir(focusFile);
	}

	public async Task ReadDir(string? focusPath = null) {
		string args = $"-b \"{PackPath}\" -listdir \"{PackDir}\"";

		var startInfo = CallPixInfo(args);

		var tcs = new TaskCompletionSource<(bool success, string? reason)>();
		PIXFile? focusFile = null;
		var batchList = new List<PIXFile>(50);

		await Task.Run(async () => {
			using var p = new Process { StartInfo = startInfo };
			p.Start();
			try {
				string? line;
				while ((line = p.StandardOutput.ReadLine()) != null) {
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
		if(success) {
			FileList.Clear();
			foreach (var f in batchList) {
				FileList.Add(f);
			}
			SelectedFile = focusFile;
			if (focusFile == null)
				ScrollToTop();
			else
				ScrollToFile(focusFile);
		} else {
			MessageBox.Show(GetString("MessageReadFileFailed") + ": " + reason);
		}
	}

	private void UpdateDirList(string linkPath) {
		if (DirList.Count > 1) {
			var baseDir = DirList[0];
			DirList.Clear();
			DirList.Add(baseDir);
		}
		while ((linkPath = linkPath[..linkPath.LastIndexOf('/')]).Length > 0) {
			DirList.Insert(1, new(linkPath));
		}
	}

	private readonly ObservableCollection<PIXFile> mFileList = [];

	public ObservableCollection<PIXFile> FileList => mFileList;

	private PIXFile? mSelectedFile = null;
	public PIXFile? SelectedFile {
		get => mSelectedFile;
		set {
			mSelectedFile = value;
			InvokeChange();
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
	private static ProcessStartInfo CallPixInfo(string args) => new() {
		FileName = Instances.ConverterPixExe!,
		Arguments = args.ToString(),
		UseShellExecute = false,
		CreateNoWindow = true,
		RedirectStandardInput = true,
		RedirectStandardOutput = true,
		RedirectStandardError = true,
		StandardOutputEncoding = Encoding.UTF8,
		StandardErrorEncoding = Encoding.UTF8
	};

	public ICommand ExtractCommand => new RelayCommand<PIXFile>(pixFile => ExtractFile(pixFile ?? SelectedFile));
	public void ExtractFile(PIXFile? pixFile) {
		if (pixFile == null)
			return;
		StringBuilder args = new($"-b \"{PackPath}\" -e \"{ExportPath}\"");
		int type = 0;
		if (pixFile.IsDir) {
			args.Append(" -extract_d \"").Append(pixFile.Filename).Append('"');
		} else {
			switch (pixFile.Extension.ToLower()) {
				case "pmd":
				case "pmg":
				case "pma"://可能出现没有pmd的情况，需要找pmd
				case "pmc":
					type = 1;
					var name = pixFile.Filename;
					name = name[..^4];
					args.Append(" -m \"").Append(name).Append('"');
					if (SearchAnim) {
						var anim = SearchAnimExe(name);
						if (anim.Count > 0)
							foreach (string a in anim) {
								args.Append(" \"").Append(a).Append('"');
								break;
							}
					}
					break;
				case "tobj":
					type = 2;
					args.Append(" -t \"").Append(pixFile.Filename).Append('"');
					break;
				default:
					args.Append(" -extract_f \"").Append(pixFile.Filename).Append('"');
					break;
			}
		}
		Process p = CallPix(args.ToString());
		p.Start();

		string? line;
		bool success = true;
		List<string> errors = [];
		while ((line = p.StandardOutput.ReadLine()) != null) {
			Debug.WriteLine(line);

			if (line.Trim().StartsWith('*') || line.Length == 0)
				continue;
			if (line.StartsWith("<error>")) {
				success = false;
				line = line[7..];
				errors.Add(line);
			}
		}
		p.WaitForExit();
		p.Close();
		if (success) {
			string osFilename = pixFile.Filename.Replace('/', '\\');
			string filePath = ExportPath + osFilename;
			if (OpenExtracted) {
				if (type == 1) {
					filePath = filePath[..^4] + ".pim";
				}
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
			string resultMessage = $"{GetString(key)} {filePath}";
			MessageBox.Show(BindingWindow, resultMessage, "Result");
		} else {
			string resultMessage = $"{GetString("MessageExtractFail")}\n{string.Join(Environment.NewLine, errors)}";
			MessageBox.Show(BindingWindow, resultMessage, "Result");
		}
	}


	private bool mSearchAnim = false;
	public bool SearchAnim {
		get => mSearchAnim;
		set {
			mSearchAnim = value;
			InvokeChange();
		}
	}

	private bool mOpenExtracted = false;
	public bool OpenExtracted {
		get => mOpenExtracted;
		set {
			mOpenExtracted = value;
			InvokeChange();
		}
	}

	public List<string> SearchAnimExe(string modelPath) {
		string args = $"-b \"{PackPath}\" --find-model-animations \"{modelPath}\"";

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

	public void TestExtractFD(PIXFile pixFile) {
		StringBuilder args = new($"-b \"{PackPath}\" -e \"{ExportPath}\"");
		if (pixFile.IsDir) {
			args.Append(" -extract_d \"").Append(pixFile.Filename).Append('"');
		} else {
			args.Append(" -extract_f \"").Append(pixFile.Filename).Append('"');
		}
		Process p = CallPix(args.ToString());

		p.Start();
		Debug.WriteLine(p.StandardOutput.ReadToEnd());
		p.WaitForExit();
		p.Close();
	}

	public ICommand ExtractAllCommand => new RelayCommand<object>(_ => ExtractAll());

	private void ExtractAll() {
		var extractTask = new ExtractAllTask(PackPath, ExportPath) {
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