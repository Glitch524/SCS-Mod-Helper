using SCS_Mod_Helper.Class;
using SCS_Mod_Helper.Class.Base;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace SCS_Mod_Helper.ConverterPix;

/// <summary>
/// TextReaderWindow.xaml 的交互逻辑
/// </summary>
public partial class TextReaderWindow: BaseWindow {
	private readonly ConverterPixBinding Binding;
	private readonly PIXFile ReadingFile;

	public TextReaderWindow(ConverterPixBinding binding, PIXFile readingFile) {
		InitializeComponent();
		Binding = binding;
		ReadingFile = readingFile;
		GridMain.DataContext = Binding;
		Closing += TextReaderWindow_Closing;
	}

	protected override void OnContentRendered(EventArgs e) {
		base.OnContentRendered(e);
		Binding.CurrentReadingFile = ReadingFile;
	}



	private void TextReaderWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
		Binding.ReaderClose();
	}

	private void OnPathClicked(object sender, RoutedEventArgs e) {
		if (sender is HyperlinkTextBlock hyperlinkTextBlock) {
			var inlines = hyperlinkTextBlock.Inlines;
			string? link = null;
			StringBuilder line = new();
			foreach (var inline in inlines) {
				switch (inline) {
					case Run run:
						line.Append(run.Text);
						break;
					case Hyperlink hyperlink:
						link = hyperlink.Tag?.ToString();
						line.Append(link);
						break;
				}
			}
			Binding.LinkClicked(line.ToString(), link);
		}
		//将converterpix窗口跳转到提供的路径
		//如果路径不存在，就提示文件不存在，可能在其他包里
		//路径一般是文件，获取文件所在的文件夹，打开文件夹，并focus到文件
		//图标文件路径是从material\ui\accessory开始的
		//支持的路径类型为interior_model interior_model_uk exterior_model exterior_model_uk model paint_job_mask icon
		//所有的路径都是使用的‘/’，converterpix也是使用‘/’，所以可以直接用
		//部分路径会以‘/’开头，converterpix也是以‘/’开头，所以可以直接用
		//icon和include没有以‘/’开头，include可能是以当前文件夹为起点
	}

	public void ResetColumnWidth(bool showSubText) {
		GridColumnText.Width = new(1, GridUnitType.Star);
		GridColumnSubText.Width = showSubText ? new(1, GridUnitType.Star) : new(0);
	}
	//读取可读取的文件，如sii。sii等出现路径的，设置链接跳转
}

