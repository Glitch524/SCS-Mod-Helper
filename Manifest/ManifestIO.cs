using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SCS_Mod_Helper.Manifest {
	public class ManifestIO(): SCSIO {
		private const string NameMFHeader = "mod_package : .package_name";
		private const string NameMFPackageVersion = "package_version";
		private const string NameMFDisplayName = "display_name";
		private const string NameMFAuthor = "author";
		private const string NameMFCategory = "category";
		private const string NameMFIcon = "icon";
		private const string NameMFDescriptionFile = "description_file";
		private const string NameMFMPOptional = "mp_mod_optional";

		protected override bool HasQuote(string name) => name switch {
			NameMFPackageVersion or
			NameMFDisplayName or
			NameMFAuthor or
			NameMFCategory or
			NameMFIcon or
			NameMFDescriptionFile => true,
			_ => false
		};

		protected override bool IsArray(string name) => name.Equals(NameMFCategory);


		protected void WriteData(StreamWriter sw, Action data) {
			sw.Write(new string('\t', TabCount));
			sw.WriteLine(NameMFHeader);
			BraceIn(sw);
			data();
			BraceOut(sw);
		}

		public static void SaveManifest(Window window, ManifestBinding binding) {
			new ManifestIO().SaveManifestInternal(window, binding);
		}

		private void SaveManifestInternal(Window window, ManifestBinding binding) {
			if (Util.IsEmpty(binding.ProjectLocation, binding.ModDisplayName, binding.DescriptionName)) {
				MessageBox.Show(window, Util.GetString("MessageManifestNotFilled"));
				return;
			}
			var saveLocation = binding.ProjectLocation;
			if (!binding.IconName.EndsWith(".jpg"))
				binding.IconName += ".jpg";

			if (binding.OldIconName != null) {
				var oldIconFile = Path.Combine(binding.ProjectLocation, binding.OldIconName);
				if (File.Exists(oldIconFile))
					File.Delete(oldIconFile);
			}
			var iconFile = Path.Combine(saveLocation, binding.IconName);
			if (binding.ModIcon == null) {
				if (File.Exists(iconFile))
					File.Delete(iconFile);
				var bitmap = new System.Drawing.Bitmap(276, 162);
				bitmap.Save(iconFile, ImageFormat.Jpeg);
			} else if (binding.NewIcon || !File.Exists(iconFile)) {
				JpegBitmapEncoder encoder = new();
				encoder.Frames.Add(BitmapFrame.Create(binding.ModIcon));
				if (File.Exists(iconFile))
					File.Delete(iconFile);
				using FileStream fs = new(iconFile, FileMode.CreateNew, FileAccess.ReadWrite);
				encoder.Save(fs);
			}

			if (binding.OldDescriptionName != null && binding.DescriptionName != binding.OldDescriptionName) {
				var descDeExt = binding.OldDescriptionName[..^4];
				foreach (var file in new DirectoryInfo(binding.ProjectLocation).GetFiles()) {
					var name = file.Name;
					if (name.StartsWith(descDeExt) && name.EndsWith(".txt")) {
						file.Delete();
					}
				}
			}
			var deExt = binding.DescriptionName[..^4];
			foreach (var locale in binding.Locales) {
				if (locale.HasDesc) {
					var isUniversal = locale.LocaleValue.Equals(Locale.Localization.Locale.LocaleValueUni);
					var dFile = Path.Combine(saveLocation, deExt);
					if (!isUniversal)
						dFile += $".{locale.LocaleValue}";
					dFile += ".txt";
					using StreamWriter dWriter = new(dFile);
					dWriter.Write(locale.DescContent);
				}
			}

			TabCount = 0;
			var manifestFile = Paths.ManifestFile(saveLocation);
			using StreamWriter sw = new(manifestFile);
			WriteFileStructure(sw, () => {
				WriteData(sw, () => {
					WriteLine(sw, NameMFPackageVersion, binding.Version);
					WriteLine(sw, NameMFDisplayName, binding.ModDisplayName);
					WriteLine(sw, NameMFAuthor, binding.Author);

					foreach (var cat in binding.SelectedCategories) {
						WriteLine(sw, NameMFCategory, cat);
					}
					WriteLine(sw, NameMFIcon, binding.IconName);
					WriteLine(sw, NameMFDescriptionFile, binding.DescriptionName);
					WriteLine(sw, NameMFMPOptional, binding.MPOptional.ToString().ToLower());
				});
			});
		}

		public static void LoadManifest(ManifestBinding binding) {
			var manifest = Paths.ManifestFile(binding.ProjectLocation);
			if (!File.Exists(manifest))
				return;
			try {
				using StreamReader sr = new(manifest);
				string? line = sr.ReadLine()?.Trim();
				if (line == null || !line.StartsWith(FileHeader))
					throw new(Util.GetString("MessageLoadManifestErrNotManifest"));
				while ((line = sr.ReadLine()?.Trim()) != null) {
					if (line.Length == 0 || line == "{" || line == "}" || line.StartsWith('#') || line.StartsWith(NameMFHeader))
						continue;
					int colonIndex = line.IndexOf(':');
					var name = line[..colonIndex].Trim();
					if (name.EndsWith("[]"))
						name = name[..^2];
					var value = ClipValue(line[(colonIndex + 1)..]);
					switch (name) {
						case NameMFPackageVersion:
							binding.Version = value;
							break;
						case NameMFDisplayName:
							binding.ModDisplayName = value;
							break;
						case NameMFAuthor:
							binding.Author = value;
							break;
						case NameMFCategory:
							binding.SetCategory(true, value);
							//binding.SelectedCategories.Add(value);
							break;
						case NameMFIcon:
							binding.IconName = value;
							binding.OldIconName = value;
							var iconFile = Path.Combine(binding.ProjectLocation, binding.IconName);
							if (File.Exists(iconFile))
								binding.ModIcon = Util.LoadImage(iconFile);
							break;
						case NameMFDescriptionFile:
							binding.DescriptionName = value;
							binding.OldDescriptionName = value;
							LoadDescription(binding);
							break;
						case NameMFMPOptional:
							binding.MPOptional = bool.Parse(value);
							break;
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(Util.GetString("MessageLoadManifestErrFail") + "\n" + ex.Message);
			}
		}

		private static void LoadDescription(ManifestBinding binding) {
			var descriptionFile = new FileInfo(Path.Combine(binding.ProjectLocation, binding.DescriptionName));
			var ext = descriptionFile.Extension;
			var deExt = binding.DescriptionName[..^(ext.Length - 1)];
			DirectoryInfo project = new(binding.ProjectLocation);
			foreach (var file in project.GetFiles()) {
				var filename = file.Name;
				var fileExt = file.Extension;
				var fileDeExt = filename[..^(fileExt.Length - 1)];
				if (fileDeExt.Length >= deExt.Length && fileDeExt.StartsWith(deExt)) {
					using StreamReader sr = new(file.FullName);
					StringBuilder sb = new();
					string? line = null;
					while ((line = sr.ReadLine()) != null) {
						sb.AppendLine(line);
					}
					string locale;
					if (fileDeExt.Length == deExt.Length)
						locale = Locale.Localization.Locale.LocaleValueUni;
					else
						locale = filename[deExt.Length..^fileExt.Length];
					var descLocale = binding.LocaleDict[locale];
					descLocale.DescContent = sb.ToString();
				}
			}
			binding.CurrentLocale = binding.LocaleDict[Locale.Localization.Locale.LocaleValueUni];
		}
	}
}
