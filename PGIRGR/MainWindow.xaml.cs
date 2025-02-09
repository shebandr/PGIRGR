using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PGIRGR
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public string fileOpenPath;
		public string fileSavePath;
		public MainWindow()
		{
			InitializeComponent();
		}

		private void mainButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
			{
				fileOpenPath = openFileDialog.FileName;
			}

			List<byte> data = MainLib.GetBytesFromPCX(fileOpenPath);
			List<List<List<byte>>> colorsTC = MainLib.printPCXTC(data, inputImage);

			List<byte> data256 = MainLib.TCto256(data, colorsTC, 250000);
			MainLib.printPCX256(data, outputImage);


			SaveFileDialog saveFileDialog = new SaveFileDialog();
			if (saveFileDialog.ShowDialog() == true)
			{
				fileSavePath = saveFileDialog.FileName;
			}
			MainLib.SetBytesToPCX(fileSavePath, data256);

		}
	}
}
