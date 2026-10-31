using SCS_Mod_Helper.Class.Base;
using System.Windows.Media;
using Wpf.Ui.Extensions;

namespace SCS_Mod_Helper.Modding.PaintJob {
	public class ColorVariant: BaseBinding {
		private Color? mColorBase = null;
		public Color? ColorBase {
			get => mColorBase;
			set {
				mColorBase = value;
				InvokeChange();
				InvokeChange(nameof(ColorBaseBrush));
			}
		}

		public SolidColorBrush? ColorBaseBrush => ColorBase?.ToBrush();

		private Color? mColor1 = null;
		public Color? Color1 {
			get => mColor1;
			set {
				mColor1 = value;
				InvokeChange();
				InvokeChange(nameof(Color1Brush));
			}
		}
		public SolidColorBrush? Color1Brush => Color1?.ToBrush();

		private Color? mColor2 = null;
		public Color? Color2 {
			get => mColor2;
			set {
				mColor2 = value;
				InvokeChange();
				InvokeChange(nameof(Color2Brush));
			}
		}
		public SolidColorBrush? Color2Brush => Color2?.ToBrush();

		private Color? mColor3 = null;
		public Color? Color3 {
			get => mColor3;
			set {
				mColor3 = value;
				InvokeChange();
				InvokeChange(nameof(Color3Brush));
			}
		}
		public SolidColorBrush? Color3Brush => Color3?.ToBrush();
	}
}
