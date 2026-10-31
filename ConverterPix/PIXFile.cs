using SCS_Mod_Helper.Class.Base;

namespace SCS_Mod_Helper.ConverterPix {
	public class PIXFile(string filename, bool isDir): BaseBinding {

		private string mFilename = filename;
		public string Filename {
			get => mFilename;
			set {
				mFilename = value;
				InvokeChange();
			}
		}

		public string Extension {
			get {
				if (mIsDir)
					return "";
				int index = mFilename.LastIndexOf('.');
				if (index == -1)
					return "";
				return mFilename[(index + 1)..].ToUpper();
			}
		}

		private bool mIsDir = isDir;
		public bool IsDir {
			get => mIsDir;
			set {
				mIsDir = value;
				InvokeChange();
			}
		}

		public string Title {
			get {
				int lastSlash = mFilename.LastIndexOf('/');
				if (lastSlash >= 0)
					return mFilename[(lastSlash + 1)..];
				return mFilename;
			}
		}

		public string Subtitle {
			get => mIsDir ? GetString("SubtitleFolder") : GetString("SubtitleFile", Extension);
		}

		public override string ToString() {
			return $"PixFile: {Filename}";
		}
	}
}
