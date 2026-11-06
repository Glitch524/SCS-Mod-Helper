using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SCS_Mod_Helper.ConverterPix;
public partial class ConverterPixBinding {
	private TextReaderWindow? TextReaderWindow = null;

	public void ReaderClose() {
		TextReaderWindow = null;
		TextLines.Clear();
	}

	public ICommand ReadFileCommand => new RelayCommand<PIXFile>(pixFile => ReadFile(pixFile ?? SelectedFile));
	public void ReadFile(PIXFile? pixFile) {
		if (pixFile == null || pixFile.IsDir)
			return;
		if (TextReaderWindow == null) {
			TextReaderWindow = new(this, pixFile) {
				Owner = BindingWindow
			};
			TextReaderWindow.Show();
		} else {
			CurrentReadingFile = pixFile;
		}
	}


	private PIXFile? mCurrentReadingFile = null;
	public PIXFile? CurrentReadingFile {
		get => mCurrentReadingFile;
		set {
			mCurrentReadingFile = value;
			InvokeChange();
			InvokeChange(nameof(TextFilename));
			if (value != null) {
				Task.Run(async () => {
					LoadingFile = true;
					await ReadTextFile(value);
					LoadingFile = false;
				});
			}
		}
	}
	private bool mLoadingFile = true;
	public bool LoadingFile {
		get => mLoadingFile;
		set {
			mLoadingFile = value;
			InvokeChange();
		}
	}

	public string TextFilename => CurrentReadingFile?.Filename ?? "";
	private readonly ObservableCollection<string> mTextLines = [];
	public ObservableCollection<string> TextLines => mTextLines;


	private bool mShowSubText = false;
	public bool ShowSubText {
		get => mShowSubText;
		set {
			mShowSubText = value;
			TextReaderWindow?.ResetColumnWidth(value);
			InvokeChange();
		}
	}

	private string mSubTextFilename = "";
	public string SubTextFilename {
		get => mSubTextFilename;
		set {
			mSubTextFilename = value;
			InvokeChange();
		} 
	}

	private readonly ObservableCollection<string> mSubTextLines = [];
	public ObservableCollection<string> SubTextLines => mSubTextLines;

	public async Task ReadTextFile(PIXFile? pixFile, bool subText = false) {
		await Application.Current.Dispatcher.InvokeAsync(() => {
			SubTextLines.Clear();
			if (subText) {
				ShowSubText = true;
			} else {
				ShowSubText = false;
				TextLines.Clear();
			}
		});
		if (pixFile == null)
			return;
		string args = $"-b \"{PackPath}\" -show_f \"{pixFile.Filename}\"";

		var startInfo = CallPixInfo(args);

		int span = 50;
		List<string> batch = new(span);
		bool get = false;

		using var p = new Process { StartInfo = startInfo };
		p.OutputDataReceived += async (sender, e) => {
			if (e.Data == null)
				return;
			var line = e.Data;
			//Debug.WriteLine(line);

			if (line.Trim().StartsWith('*') || line.Length == 0)
				return;
			if (line.StartsWith("Unable to open file to read")) {
				await Application.Current.Dispatcher.InvokeAsync(() => {
					MessageBox.Show(TextReaderWindow, GetString("MessageReadFileFailed"), pixFile.Filename);
				});
				return;
			}
			if (line.Trim().Contains("--")) {
				if (get)
					return;
				get = true;
				return;
			}
			if (!get)
				return;

			var trim = line.Trim();
			if (trim.StartsWith("interior_model") ||
			trim.StartsWith("interior_model_uk") ||
			trim.StartsWith("exterior_model") ||
			trim.StartsWith("exterior_model_uk") ||
			trim.StartsWith("paint_job_mask") ||
			trim.StartsWith("icon") ||
			trim.StartsWith("model") ||
			trim.StartsWith("coll") ||
			trim.StartsWith("rope_material") || 
			trim.StartsWith("animation") ||
			trim.StartsWith("phys_model") ||
			trim.StartsWith("phys_model_coll") ||
			trim.StartsWith("material") ||
			trim.StartsWith("@include") ||
			trim.StartsWith("detail_model") ||
			trim.StartsWith("lods[]") ||
			trim.StartsWith("trailer_brace_model") || 
			trim.StartsWith("trailer_brace_anim") ||
			trim.StartsWith("extended_shadow_texture") ||
			trim.StartsWith("ui_shadow")) {
				int firstQuoteIndex = line.IndexOf('"');
				if (firstQuoteIndex >= 0) {
					int secondQuoteIndex = line.IndexOf('"', firstQuoteIndex + 1);
					if (secondQuoteIndex > firstQuoteIndex) {
						line = line.Insert(secondQuoteIndex, "[/link]");
						line = line.Insert(firstQuoteIndex + 1, "[link]");
					}
				}
			}
			batch.Add(line);
			if (batch.Count >= span) {
				var snapshot = batch.ToList();
				batch.Clear();
				await Application.Current.Dispatcher.InvokeAsync(() => {
					foreach (var data in snapshot) {
						if (subText)
							SubTextLines.Add(data);
						else
							TextLines.Add(data);
					}
				}, System.Windows.Threading.DispatcherPriority.Background);
			}
		};

		var errorString = new StringBuilder();
		p.ErrorDataReceived += (sender, e) => {
			if (e.Data != null)
				errorString.AppendLine(e.Data);
		};

		p.Start();
		p.BeginOutputReadLine();
		p.BeginErrorReadLine();

		await p.WaitForExitAsync();
		if (batch.Count > 0) {
			var snapshot = batch.ToList();
			batch.Clear();
			await Application.Current.Dispatcher.InvokeAsync(() => {
				foreach (var data in snapshot) {
					if (subText)
						SubTextLines.Add(data);
					else
						TextLines.Add(data);
				}
			}, System.Windows.Threading.DispatcherPriority.Background);
		}
	}

	public void LinkClicked(string line, string? link) {
		if (link == null)
			return;
		string trim = line.Trim();
		string path;
		if (trim.StartsWith("@include")) {
			path = PackDir + '/' + link;
			SubTextFilename = trim;
			ReadSubFile(new PIXFile(path, false));
			return;
		} else if (trim.StartsWith("icon")) {
			path = "/material/ui/accessory/" + link;
		} else 
			path = link;
		int lastS = path.LastIndexOf('/');
		if (lastS == -1)
			return;
		string dir = path[..lastS];
		HistoryAdd();
		Task.Run(() => {
			_ = GotoLink(path);
		});
	}

	private void ReadSubFile(PIXFile subFile) {
		Task.Run(async () => {
			LoadingFile = true;
			await ReadTextFile(subFile, true);
			LoadingFile = false;
		});
	}
	public async Task GotoLink(string linkPath) {
		int lastS = linkPath.LastIndexOf('/');
		if (lastS == -1)
			return;
		string dir = linkPath[..lastS];
		if (PackDir == dir)//如果当前dir与reader点击的链接所在dir相同，那就没有必要重新读取
			return;

		string args = $"-b \"{PackPath}\" -listdir \"{dir}\"";

		var startInfo = CallPixInfo(args);

		PIXFile? linkFile = null;
		bool error = false;
		List<PIXFile> linkFiles = [];

		using var p = new Process { StartInfo = startInfo };
		p.OutputDataReceived += async (sender, e) => {
			var line = e.Data;
			if (line == null || line.Trim().StartsWith('*') || line.Length == 0)
				return;
			if (line.StartsWith("<error>")) {
				var reason = line[(line.IndexOf(':') + 1)..];
				await Application.Current.Dispatcher.InvokeAsync(() => {
					if (reason == "readDir returned null!") {
						MessageBox.Show(GetString("MessageLinkFileNotExist"));
					} else
						MessageBox.Show(GetString("MessageReadFileFailed") + ": " + reason);
				});
				error = true;
				p.CancelOutputRead();
				return;
			}
			if (line.Contains("Done")) 
				return;
			if (line.StartsWith("[D]")) {
				string path = line[4..];
				var file = new PIXFile(path, true);
				if (linkPath == path)
					linkFile = file;
				linkFiles.Add(file);
			} else if (line.StartsWith("[F]")) {
				string path = line[4..];
				var file = new PIXFile(path, false);
				if (linkPath == path)
					linkFile = file;
				linkFiles.Add(file);
			}
		};
		p.Start();
		p.BeginOutputReadLine();

		await p.WaitForExitAsync();
		if (!error) {
			await Application.Current.Dispatcher.InvokeAsync(() => {
				if (linkFile == null)//如果没有linkfile说明这个文件不在这个包里
					MessageBox.Show(GetString("MessageLinkFileNotExist"));
				else {
					FileList.Clear();
					foreach (var f in linkFiles) {
						FileList.Add(f);
					}
					ScrollToFile(linkFile);
					SelectedFile = linkFile;

					foreach(var pix in Application.Current.Windows.OfType<ConverterPixWindow>()) {
						pix.Focus();
					}
				}
			});
		}
	}
}
