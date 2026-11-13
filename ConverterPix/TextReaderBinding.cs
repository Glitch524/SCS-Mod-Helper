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

	public ICommand ReadFileCommand => new RelayCommand<PIXFile>(pixFile => ReadFile(pixFile ?? CurrentSelectedFile));
	public void ReadFile(PIXFile? pixFile) {
		if (pixFile == null || pixFile.IsDir)
			return;
		ReadingFilePackName = CurrentPack!.PackName;

		if (TextReaderWindow == null) {
			TextReaderWindow = new(this, pixFile) {
				Owner = BindingWindow
			};
			TextReaderWindow.Show();
		} else {
			CurrentReadingFile = pixFile;
		}
	}

	private string ReadingFilePackName = string.Empty;
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

	public string TextFilename => CurrentReadingFile == null ? "" : $"{ReadingFilePackName}:{CurrentReadingFile.Filename}";
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
		string args = $"-b \"{CurrentPackPath}\" -show_f \"{pixFile.Filename}\"";

		using Process p = CallPix(args);
		p.ErrorDataReceived += (s, e) => {
			if (e.Data != null)
				Debug.WriteLine("[err] " + e.Data);
		};
		p.Start();
		p.BeginErrorReadLine();

		int span = 50;
		List<string> batch = new(span);
		bool get = false;

		string? line;
		while((line = await p.StandardOutput.ReadLineAsync()) != null) {
			if (line.Trim().StartsWith('*') || line.Length == 0)
				continue;
			if (line.StartsWith("Unable to open file to read")) {
				await Application.Current.Dispatcher.InvokeAsync(() => {
					MessageBox.Show(TextReaderWindow, GetString("MessageReadFileFailed"), pixFile.Filename);
				});
				continue;
			}
			if (line.Trim().Contains("--")) {
				if (!get)
					get = true;
				continue;
			}
			if (!get)
				continue;
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
		}
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
			if (link.StartsWith('/'))//如果以/开头，就表示路径为绝对路径，从projectlocation开始
				path = link;
			else
				path = CurrentPackDir + '/' + link;
			SubTextFilename = trim;
			Task.Run(async () => {
				LoadingFile = true;
				await ReadTextFile(new PIXFile(path, false), true);
				LoadingFile = false;
			});
			return;
		}
		if (trim.StartsWith("icon")) {
			path = $"/material/ui/accessory/{link}.tobj";
		} else 
			path = link;
		int lastS = path.LastIndexOf('/');
		if (lastS == -1)
			return;
		string dir = path[..lastS];
		if (CurrentPackDir != dir)
			HistoryAdd();
		Task.Run(async () => {
			int lastS = path.LastIndexOf('/');
			if (lastS == -1)
				return;
			var pathLocation = path[..lastS];
			if (CurrentPackDir == dir) { //如果当前dir与reader点击的链接所在dir相同，那就没有必要重新读取
				foreach(var pix in CurrentFileList) {
					if (pix.Filename == path)
						return;
				}
			}
			var result = await GotoLink(CurrentPack!, pathLocation, path);//首先搜索当前的pack
			if (result)//result为true即成功找到并展示对应文件夹
				return;
			if (PackList.Count > 1) {//打开了多个pack的情况，使用对应pack来获取文件夹内容
				foreach (var pack in PackList) {
					if (pack == CurrentPack || pack.PackPath.Length == 0)
						continue;
					if (await GotoLink(pack, pathLocation, path))
						return;
				}
			}
			await Application.Current.Dispatcher.InvokeAsync(() => {
				MessageBox.Show(GetString("MessageLinkFileNotExist"));
			});
		});
	}


	public async Task<bool> GotoLink(PixPackage pixPack, string pathLocation, string linkPath) {//todo 如果path是def.scs，而且是def/vehicle，就转到basevehicle
		var packPath = pixPack.PackPath;
		string args = $"-b \"{packPath}\" -listdir \"{pathLocation}\"";
		using Process p = CallPix(args);
		p.ErrorDataReceived += (s, e) => {
			if (e.Data != null)
				Debug.WriteLine("[err] " + e.Data);
		};
		p.Start();
		p.BeginErrorReadLine();

		bool error = false;
		PIXFile? linkFile = null;
		List<PIXFile> linkFiles = [];

		string? line;
		while ((line = await p.StandardOutput.ReadLineAsync()) != null) {
			if (line == null || line.Trim().StartsWith('*') || line.Length == 0)
				continue;
			if (line.StartsWith("<error>")) {
				var reason = line[(line.IndexOf(':') + 1)..].Trim();
				await Application.Current.Dispatcher.InvokeAsync(() => {
					if (reason == "readDir returned null!") {
						MessageBox.Show(GetString("MessageLinkFileNotExist"));
					} else
						MessageBox.Show(GetString("MessageReadFileFailed") + ": " + reason);
				});
				error = true;
				continue;
			}
			if (line.Contains("Done"))
				continue;
			if (line.StartsWith("[D]") || line.StartsWith("[F]")) {
				string path = line[4..];
				var file = new PIXFile(path, line.StartsWith("[D]"));
				if (linkPath == path)
					linkFile = file;
				linkFiles.Add(file);
			}
		}
		await p.WaitForExitAsync();
		p.Close();
		if (!error) {
			return await Application.Current.Dispatcher.InvokeAsync(() => {
				if (linkFile == null) {//如果没有linkfile说明这个文件不在这个包里，由于需要返回click处继续查找下一个包，所以这里不能放messagebox提示，否则会发生跳到正确位置，但依旧提示找不到的问题
					return false;
				} else {
					if (pixPack != CurrentPack)//如果输入的pixPack不是当前pack说明是其他pack找到的路径
						CurrentPack = pixPack;//所以先切换当前选中pack
					UpdateDirList(linkPath);
					CurrentFileList.Clear();
					foreach (var f in linkFiles) {
						CurrentFileList.Add(f);
					}
					ScrollToFile(linkFile);
					CurrentSelectedFile = linkFile;

					foreach(var pix in Application.Current.Windows.OfType<ConverterPixWindow>()) {
						pix.Focus();
					}
					return true;
				}
			});
		}
		return false;
	}
}
