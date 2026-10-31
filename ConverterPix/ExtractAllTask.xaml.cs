using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace SCS_Mod_Helper.ConverterPix {
	/// <summary>
	/// ExtractAllTask.xaml 的交互逻辑
	/// </summary>
	public partial class ExtractAllTask: BaseWindow {
		Task ExtractTask;
		private readonly string PackPath;
		private readonly string DestPath;
		public CancellationToken Token => TokenSource.Token;
		public ExtractAllTask(string packPath, string destPath) {
			InitializeComponent();

			reporter = new Progress<string>(UpdateCall);

			PackPath = packPath;
			DestPath = destPath;

			ExtractTask = new(Extraction(reporter), Token);
			ExtractTask.Start();
		}

		readonly IProgress<string> reporter;

		private void UpdateCall(string call) {
			switch (call) {
				case "prepareMode":
					ProgressStep.IsIndeterminate = PrepareMode;
					PanelFile.Visibility = PrepareMode ? Visibility.Collapsed : Visibility.Visible;
					ProgressFile.IsIndeterminate = PrepareMode;
					break;
				case "startStep":
					ProgressStep.Value = Step;
					TextMessage.Text = Message;
					ProgressFile.Value = ProgressCurrent;
					ProgressFile.Maximum = ProgressMax;
					TextCurrentFileType.Text = CurrentFileType;
					break;
				case "Progress":
					ProgressFile.Value = ProgressCurrent;
					TextCurrentFile.Text = CurrentFile;
					break;
				case "Finish":
					MessageBox.Show(this, "Task Finished");
					Close();
					break;
			}
		}

		private void ButtonCancelClick(object sender, RoutedEventArgs e) {
			TokenSource.Cancel();
			Close();
		}

		bool PrepareMode = true;
		int Step = 0;
		string Message = "";
		string CurrentFileType = "";

		private int ProgressCurrent = 0;
		private int ProgressMax = 0;
		private string CurrentFile = "";


		public CancellationTokenSource TokenSource = new();

		public Action ExtractionTest(IProgress<string> reporter) => () => {

			//读取文件列表
			Thread.Sleep(500);

			int fileCount = 20;
			int fileInterval = 100;

			PrepareMode = false;
			Step = 1;
			Message = GetString("MessageExtractPMD");
			ProgressCurrent = 0;
			ProgressMax = fileCount;
			CurrentFileType = GetString("CurrentFilePMD");

			reporter.Report("prepareMode");
			reporter.Report("startStep");

			for (int i = 0; i < fileCount; i++) {
				EnsureActive();
				ProgressCurrent = i + 1;
				CurrentFile = " " + i;
				reporter.Report("Progress");
				Thread.Sleep(fileInterval);
			}

			Step = 2;
			Message = GetString("MessageExtractTOBJ");
			ProgressCurrent = 0;
			ProgressMax = fileCount;
			CurrentFileType = GetString("CurrentFileTOBJ");

			reporter.Report("startStep");
			for (int i = 0; i < fileCount; i++) {
				EnsureActive();
				ProgressCurrent = i + 1;
				CurrentFile = " " + i;
				reporter.Report("Progress");
				Thread.Sleep(fileInterval);
			}

			Step = 3;
			Message = GetString("MessageExtractDDS");
			ProgressCurrent = 0;
			ProgressMax = fileCount;
			CurrentFileType = GetString("CurrentFileDDS");

			reporter.Report("startStep");
			for (int i = 0; i < fileCount; i++) {
				EnsureActive();
				ProgressCurrent = i + 1;
				CurrentFile = " " + i;
				reporter.Report("Progress");
				Thread.Sleep(fileInterval);
			}

			Step = 4;
			Message = GetString("MessageExtractOther");
			ProgressCurrent = 0;
			ProgressMax = fileCount;
			CurrentFileType = GetString("CurrentFileOther");

			reporter.Report("startStep");
			for (int i = 0; i < fileCount; i++) {
				EnsureActive();
				ProgressCurrent = i + 1;
				CurrentFile = " " + i;
				reporter.Report("Progress");
				Thread.Sleep(fileInterval);
			}

			Step = 5;
			Message = GetString("MessageExtractDir");
			ProgressCurrent = 0;
			ProgressMax = fileCount;
			CurrentFileType = GetString("CurrentFileDir");

			reporter.Report("startStep");
			for (int i = 0; i < fileCount; i++) {
				EnsureActive();
				ProgressCurrent = i + 1;
				CurrentFile = " " + i;
				reporter.Report("Progress");
				Thread.Sleep(fileInterval);
			}

			reporter.Report("Finish");
		};

		public Action Extraction(IProgress<string> reporter) => () => {
			ListFiles(out List<string> pmdFiles,
			 out List<string> tobjFiles,
			 out List<string> ddsFiles,
			 out List<string> defFiles,
			 out List<string> matFiles,
			 out List<string> otherFiles,
			 out List<string> dirs);
			EnsureActive();

			PrepareMode = false;
			Step = 1;
			Message = GetString("MessageExtractPMD");
			ProgressCurrent = 0;
			ProgressMax = pmdFiles.Count;
			CurrentFileType = GetString("CurrentFilePMD");

			reporter.Report("prepareMode");
			reporter.Report("startStep");

			for (int i = 0; i < pmdFiles.Count; i++) {
				EnsureActive();
				var pmd = pmdFiles[i];
				ProgressCurrent = i + 1;
				CurrentFile = pmd;
				reporter.Report("Progress");

				if (IsFileExist(pmd))
					continue;
				ExtractPMDSimple(pmd);
			}

			EnsureActive();
			Step = 2;
			Message = GetString("MessageExtractTOBJ");
			ProgressCurrent = 0;
			ProgressMax = tobjFiles.Count;
			CurrentFileType = GetString("CurrentFileTOBJ");

			reporter.Report("startStep");
			for (int i = 0; i < tobjFiles.Count; i++) {
				EnsureActive();
				var tobj = tobjFiles[i];
				ProgressCurrent = i + 1;
				CurrentFile = tobj;
				reporter.Report("Progress");

				if (IsFileExist(tobj))
					continue;
				ExtractTOBJSimple(tobj);
			}

			EnsureActive();
			Step = 3;
			Message = GetString("MessageExtractDDS");
			ProgressCurrent = 0;
			ProgressMax = ddsFiles.Count;
			CurrentFileType = GetString("CurrentFileDDS");

			reporter.Report("startStep");
			for (int i = 0; i < ddsFiles.Count; i++) {
				EnsureActive();
				var dds = ddsFiles[i];
				ProgressCurrent = i + 1;
				CurrentFile = dds;
				reporter.Report("Progress");

				if (IsFileExist(dds))
					continue;
				ExtractDDSSimple(dds);
			}

			EnsureActive();
			Step = 4;
			Message = GetString("MessageExtractDEF");
			ProgressCurrent = 0;
			ProgressMax = defFiles.Count;
			CurrentFileType = GetString("CurrentFileDEF");

			reporter.Report("startStep");
			for (int i = 0; i < defFiles.Count; i++) {
				EnsureActive();
				var def = defFiles[i];
				ProgressCurrent = i + 1;
				CurrentFile = def;
				reporter.Report("Progress");

				if (IsFileExist(def))
					continue;
				ExtractOtherSimple(def);
			}

			EnsureActive();
			Step = 5;
			Message = GetString("MessageExtractMAT");
			ProgressCurrent = 0;
			ProgressMax = matFiles.Count;
			CurrentFileType = GetString("CurrentFileMAT");

			reporter.Report("startStep");
			for (int i = 0; i < matFiles.Count; i++) {
				EnsureActive();
				var mat = matFiles[i];
				ProgressCurrent = i + 1;
				CurrentFile = mat;
				reporter.Report("Progress");

				if (IsFileExist(mat))
					continue;
				ExtractOtherSimple(mat);
			}

			EnsureActive();
			Step = 6;
			Message = GetString("MessageExtractOther");
			ProgressCurrent = 0;
			ProgressMax = otherFiles.Count;
			CurrentFileType = GetString("CurrentFileOther");

			reporter.Report("startStep");
			for (int i = 0; i < otherFiles.Count; i++) {
				EnsureActive();
				var other = otherFiles[i];
				ProgressCurrent = i + 1;
				CurrentFile = other;
				reporter.Report("Progress");

				if (IsFileExist(other))
					continue;
				ExtractOtherSimple(other);
			}

			EnsureActive();
			Step = 7;
			Message = GetString("MessageExtractDir");
			ProgressCurrent = 0;
			ProgressMax = dirs.Count;
			CurrentFileType = GetString("CurrentFileDir");

			reporter.Report("startStep");
			for (int i = 0; i < dirs.Count; i++) {
				EnsureActive();
				var dirList = dirs[i];
				ProgressCurrent = i + 1;
				CurrentFile = dirList;
				reporter.Report("Progress");

				CreateDirs(dirList);
			}

			reporter.Report("Finish");
		};

		private void EnsureActive() => TokenSource.Token.ThrowIfCancellationRequested();

		public void ListFiles(
						out List<string> pmdFiles,
						out List<string> tobjFiles,
						out List<string> ddsFiles,
						out List<string> defFiles,
						out List<string> matFiles,
						out List<string> otherFiles,
						out List<string> dirs) {
			string args = $"-b \"{PackPath}\" --list-directory-recursive \"/\"";
			Process p = CallPix(args);
			p.Start();
			dirs = [];
			pmdFiles = [];
			tobjFiles = [];
			ddsFiles = [];
			defFiles = [];
			matFiles = [];
			otherFiles = [];

			string? line;
			while ((line = p.StandardOutput.ReadLine()) != null) {
				EnsureActive();
				Debug.WriteLine(line);
				if (line.StartsWith("<error>")) {
					var reason = line[(line.IndexOf(':') + 1)..];
					MessageBox.Show($"Unable to read the file: {reason}");
					break;
				}
				if (line.Contains("Done"))
					break;
				if (line.StartsWith("[F]")) {
					line = line[4..];
					if (line.StartsWith("/automat"))
						continue;
					if (line.EndsWith(".pmd") || line.EndsWith(".pmg"))
						pmdFiles.Add(line);
					else if (line.EndsWith(".tobj"))
						tobjFiles.Add(line);
					else if (line.EndsWith(".dds"))
						ddsFiles.Add(line);
					else if (line.EndsWith(".sii") || line.EndsWith(".sui"))
						defFiles.Add(line);
					else if (line.EndsWith(".mat"))
						matFiles.Add(line);
					else
						otherFiles.Add(line);
				} else if (line.StartsWith("[D]")) {
					line = line[4..];
					if (line.StartsWith("/automat"))
						continue;
					dirs.Add(line);
				}
			}
			p.WaitForExit();
			p.Close();
		}

		private bool IsFileExist(string filename) {
			if (filename.EndsWith(".pmd") || filename.EndsWith(".pmg"))
				filename = filename[..^4] + ".pim";
			else if (filename.EndsWith(".pma"))
				filename = filename[..^4] + ".pia";
			filename = filename.Replace('/', '\\');
			string osFilename = DestPath + filename;
			return File.Exists(osFilename);
		}

		private static Process CallPix(string args) {
			Process p = new();
			p.StartInfo.FileName = Instances.ConverterPixPath;
			p.StartInfo.Arguments = args;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardInput = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.CreateNoWindow = true;
			return p;
		}
		public void ExtractPMDSimple(string pmdFile) {
			StringBuilder args = new($"-b \"{PackPath}\" -e \"{DestPath}\"");
			var name = pmdFile;
			name = name[..^4];
			args.Append(" -m \"").Append(name).Append('"');
			var anim = SearchAnimExe(name);
			if (anim.Count > 0) {
				foreach (string a in anim) {
					args.Append(" \"").Append(a).Append('"');
					break;
				}
			}
			Process p = CallPix(args.ToString());
			p.Start();
			string? line;
			while ((line = p.StandardOutput.ReadLine()) != null) {
				if (line.Trim().StartsWith('*') || line.Length == 0)
					continue;
				if (line.StartsWith("<error>")) {
					//line = line[7..];
				}
			}
			p.WaitForExit();
			p.Close();
		}
		public List<string> SearchAnimExe(string modelPath) {
			string args = $"-b \"{PackPath}\" --find-model-animations \"{modelPath}\"";

			Process p = CallPix(args);

			p.Start();
			p.WaitForExit();
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
			p.Close();
			return anims;
		}

		public void ExtractTOBJSimple(string tobjFile) {
			string args = $"-b \"{PackPath}\" -e \"{DestPath}\" -t \"{tobjFile}\"";
			Process p = CallPix(args.ToString());
			p.Start();
			string? line;
			List<string> errors = [];
			while ((line = p.StandardOutput.ReadLine()) != null) {
				if (line.Trim().StartsWith('*') || line.Length == 0)
					continue;
				if (line.StartsWith("<error>")) {
					line = line[7..];
					errors.Add(line);
				}
			}
			p.WaitForExit();
			p.Close();
		}

		public void ExtractDDSSimple(string ddsFile) {
			string args = $"-b \"{PackPath}\" -e \"{DestPath}\" -d \"{ddsFile}\"";
			Process p = CallPix(args.ToString());
			p.Start();
			string? line;
			List<string> errors = [];
			while ((line = p.StandardOutput.ReadLine()) != null) {
				if (line.Trim().StartsWith('*') || line.Length == 0)
					continue;
				if (line.StartsWith("<error>")) {
					line = line[7..];
					errors.Add(line);
				}
			}
			p.WaitForExit();
			p.Close();
		}

		public void ExtractOtherSimple(string otherFile) {
			string args = $"-b \"{PackPath}\" -e \"{DestPath}\" -extract_f \"{otherFile}\"";
			Process p = CallPix(args.ToString());
			p.Start();
			string? line;
			List<string> errors = [];
			while ((line = p.StandardOutput.ReadLine()) != null) {
				if (line.Trim().StartsWith('*') || line.Length == 0)
					continue;
				if (line.StartsWith("<error>")) {
					line = line[7..];
					errors.Add(line);
				}
			}
			p.WaitForExit();
			p.Close();
		}
		private void CreateDirs(string dirName) {
			dirName = dirName.Replace('/', '\\');
			string osFilename = DestPath + dirName;
			if (File.Exists(osFilename) || Directory.Exists(osFilename))
				return;
			Directory.CreateDirectory(osFilename);
		}
	}
}
