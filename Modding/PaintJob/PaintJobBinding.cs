using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Modding.Accessories;
using SCS_Mod_Helper.Modding.PaintJob.ColorMask;
using SCS_Mod_Helper.Utils;
using SCS_Mod_Helper.Vehicles;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Modding.PaintJob {
	public partial class PaintJobBinding: WindowBinding {
		public Window? PreviewWindow;
		public bool IsPreviewOpen => PreviewWindow != null;

		public void OpenPaintJobPreview() {
			if (PreviewWindow == null) {
				PreviewWindow = new PaintJobTexPreviewWindow(this) {
					Owner = BindingWindow
				};
				PreviewWindow.Show();
			} else
				PreviewWindow.Focus();
			PaintJobData.CurrentVehicle?.GetCabinUV();
		}


		private readonly AccPaintJobData mPaintJobData;
		public AccPaintJobData PaintJobData => mPaintJobData;
		public PaintJobBinding(Func<IBaseVisual> visualFunc): base(visualFunc) {
			mPaintJobData = new(() => ShowCabinUV);
		}

		public bool SyncOverNetwork {
			get => PaintJobData.SyncOverNetwork;
			set {
				PaintJobData.SyncOverNetwork = value;
				InvokeChange();
			}
		}

		public string ModelName {
			get => PaintJobData.ModelName;
			set {
				PaintJobData.ModelName = value;
				InvokeChange();

				InvokeChange(nameof(NameOver12));
			}
		}

		public bool NameOver12 => ModelName.Length > 12;

		public void ChooseIcon() => PaintJobData.ChooseIcon(BindingWindow);

		//todo suitablefor conflictwith


		public string PaintJobTex => PaintJobData.PaintJobTex;

		public ICommand AddOvrCommand => new RelayCommand(PaintJobData.AddOvr);
		public ICommand RemoveOvrCommand => new RelayCommand<PaintJobOverrideData>(PaintJobData.RemoveOvr);

		public void AccessoryChecked(Accessory accessory) => PaintJobData.AccessoryChecked(accessory);
		public void AccessoryUnchecked(Accessory accessory) => PaintJobData.AccessoryUnchecked(accessory);


		public void CreatePaintJobSii() {
			if (ModelName.Length == 0
				|| PaintJobData.CurrentVehicle == null
				|| PaintJobData.DisplayName.Length == 0)
				return;
			new PaintJobSCSIO().WritePaintJobData(this);
			MessageBox.Show(BindingWindow, GetString("MessagePaintJobCreateSuccess"));
		}


		public void FileDrop(string tag, string filePath) {
			Debug.WriteLine(tag + " " + filePath);
			if (tag == "IconName") {
				PaintJobData.CheckIcon(BindingWindow, filePath);
			} else {
				CheckTexPath(tag, filePath);
			}
		}

		public const string TEX_PAINT_JOB = "PaintJobTex";
		public const string TEX_BASE_TEX_OVR = "BaseTexOverride";
		public const string TEX_FLAKE_NOISE = "FlakeNoise";
		public const string TEX_ACC_TEX = "AccTex";
		public void ChooseTex(string type) {
			string title;
			string filter = GetFilter("DialogFilterTextureFile");
			switch (type) {
				case TEX_PAINT_JOB:
				case TEX_ACC_TEX:
					title = GetString("TitlePaintJobTexture");
					break;
				case TEX_BASE_TEX_OVR:
					title = GetString("TitleBaseTexOverride");
					break;
				case TEX_FLAKE_NOISE:
					title = GetString("TitleFlakeNoise");
					break;
				default:
					return;
			}

			var textureDialog = new OpenFileDialog {
				Title = title,
				Multiselect = false,
				DefaultDirectory = Instances.ProjectLocation,
				DefaultExt = "tobj",
				Filter = filter,
			};
			if (textureDialog.ShowDialog() != true)
				return;
			string path = textureDialog.FileName;
			CheckTexPath(type, path);
		}

		public void CheckTexPath(string type, string path) {//先确定path的类型是否为指定类型，因为filedrop放的不一定
			string projectLocation = Instances.ProjectLocation;
			if (path.StartsWith(projectLocation)) {
				string? imagePath;
				string tobjPath;
				if (path.EndsWith(".tobj")) {
					imagePath = GetPathFromTobj(path);
					tobjPath = path;
				} else if (path.EndsWith(".tga") || path.EndsWith(".dds")) {
					imagePath = path;
					tobjPath = path[..^4] + ".tobj";
					if (!File.Exists(tobjPath)) {
						var result = MessageBox.Show(BindingWindow, GetString("MessageAskCreateTobj"), GetString("MessageTitleNotice"), MessageBoxButton.YesNoCancel);
						switch(result) {
							case MessageBoxResult.Yes:
								AccessoryDataUtil.CreateTobjFile(path);
								break;
							case MessageBoxResult.No:
								break;
							case MessageBoxResult.Cancel:
								return;
						}
					}
				} else {
					MessageBox.Show(BindingWindow, GetString("MessageFileTypeNotSupported"));
					return;
				}
				string inProjectpath = tobjPath[projectLocation.Length..];
				inProjectpath = inProjectpath.Replace("\\", "/");
				if (!inProjectpath.StartsWith('/'))
					inProjectpath = "/" + inProjectpath;
				switch (type) {
					case TEX_PAINT_JOB:
						PaintJobData.PaintJobTex = inProjectpath;
						PaintJobData.PaintJobTexRealPath = path;
						break;
					case TEX_BASE_TEX_OVR:
						PaintJobData.BaseTexOverride = inProjectpath;
						break;
					case TEX_FLAKE_NOISE:
						PaintJobData.FlakeNoise = inProjectpath;
						break;
					case TEX_ACC_TEX:
						PaintJobData.AccTex = inProjectpath;
						break;
					default:
						return;
				}
				if (imagePath != null) {
					if (type == TEX_PAINT_JOB || type == TEX_ACC_TEX) {
						imagePath = imagePath.ToLower();
						if (imagePath.EndsWith(".dds") || imagePath.EndsWith(".tga")) {
							if (type == TEX_PAINT_JOB) {
								LoadTexImage(imagePath);
							} else
								PaintJobData.AccTexImage = Util.LoadPfimImage(imagePath);
						} else {
							if (type == TEX_PAINT_JOB) {
								LoadTexImage(imagePath);
							} else
								PaintJobData.AccTexImage = Util.LoadImage(imagePath);
						}
					}
				}
			} else {
				MessageBox.Show(BindingWindow, GetString("DialogResultFileOutsideProject"));
			}
		}

		public void LoadTexImage(string imagePath) {
			PaintJobTexImage = null;
			MaskChannelR = null;
			MaskChannelG = null;
			MaskChannelB = null;
			if (IsPreviewOpen) {
				PaintJobTexImage = Util.LoadPfimImage(imagePath);
				LoadColorChannel();
			}
		}

		public void LoadColorChannel() {
			if (!PaintJobData.Airbrush && PaintJobTexImage != null && MaskChannelR == null) {
				ChannelExtraction.ExtractRGBChannels(PaintJobTexImage, out WriteableBitmap rBmp, out WriteableBitmap gBmp, out WriteableBitmap bBmp);
				MaskChannelR = rBmp;
				MaskChannelG = gBmp;
				MaskChannelB = bBmp;
			}
		}

		private static string? GetPathFromTobj(string tobjPath) {
			using StreamReader reader = new(tobjPath, Encoding.UTF8);
			string? line = reader.ReadLine();
			if (line == null)
				return null;
			string result;
			if (line.StartsWith('\u0001')) {
				line = reader.ReadLine();
				if (line == null)
					return null;
				result = '/' + line[(line.IndexOf('/') + 1)..];
			} else {
				result = deMap2D().Replace(line, "").Trim();
				if (result.Length == 0) {//路径有可能在下一行
					line = reader.ReadLine()?.Trim();
					if (line == null || line.StartsWith("addr"))
						return null;
					result = line;
				}
			}
			result = result.Replace('/', '\\');
			if (result.StartsWith('\\')) {
				result = Instances.ProjectLocation + result;
			} else {
				result = new DirectoryInfo(tobjPath).Parent + "\\" + result;
			}
			return result;
		}

		[GeneratedRegex(@"map[\s\t]2d")]
		private static partial Regex deMap2D();
	}
}
