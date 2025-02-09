using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace PGIRGR
{
	internal class MainLib
	{
		public static List<byte> GetBytesFromPCX(string path)
		{
			List<byte> bytes = new List<byte>();

			try
			{
				bytes = File.ReadAllBytes(path).ToList();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Ошибка при чтении файла: " + ex.Message);
				return null;
			}


			return bytes;
		}

		public static void SetBytesToPCX(string path, List<byte> bytes)
		{
			try
			{
				// Записываем массив байтов в файл
				File.WriteAllBytes(path, bytes.ToArray());
				Console.WriteLine("Файл успешно записан " + bytes.Count + " байт");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при записи файла: {ex.Message}");
			}
		}

		public static List<List<List<byte>>> printPCXTC(List<byte> data, Image outputImage)
		{
			List<List<List<byte>>> pixelsDecoded = new List<List<List<byte>>>();

			List<byte> dataDecoded = new List<byte>();
			int currentIndex = 128;
			List<List<byte>> colors = new List<List<byte>>();

			int width = BitConverter.ToInt16(data.GetRange(8, 2).ToArray(), 0) + 1;
			int height = BitConverter.ToInt16(data.GetRange(10, 2).ToArray(), 0) + 1;

			//дешифрование RLE шифра
			for (int i = currentIndex; i < (data.Count); i++)
			{
				if (data[i] < 193)
				{
					dataDecoded.Add(data[i]);
				}
				else if (data[i] == 193)
				{
					i++;
					dataDecoded.Add(data[i]);
				}
				else if (data[i] > 193)
				{
					int temp = data[i] - 192;
					i++;
					for (int q = 0; q < temp; q++)
					{
						dataDecoded.Add(data[i]);
					}
				}
			}

			for (int i = 0; i < height*3; i+=3)
			{
				pixelsDecoded.Add(new List<List<byte>>());
				for (int j = 0; j < width; j++)
				{
					pixelsDecoded[i/3].Add(new List<byte> { dataDecoded[(width*i) + j], dataDecoded[(width * (i+1)) + j], dataDecoded[(width * (i + 2)) + j] });


				}
			}

			Console.WriteLine(data.Count + " | " + dataDecoded.Count + " | " + width + " | " + height);


			WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
			outputImage.Source = bitmap;
			bitmap.Lock();
			unsafe
			{
				IntPtr pBackBuffer = bitmap.BackBuffer;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{

						IntPtr pPixel = pBackBuffer + i * bitmap.BackBufferStride + j * 4;

						byte r = pixelsDecoded[i][j][0];
						byte g = pixelsDecoded[i][j][1];
						byte b = pixelsDecoded[i][j][2];
						*((int*)pPixel) = (255 << 24) | (r << 16) | (g << 8) | b;

					}
				}
			}
			bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
			bitmap.Unlock();

			return pixelsDecoded;

		}
		public static void printPCX256(List<byte> data, Image outputImage)
		{
			List<List<List<byte>>> pixelsDecoded = new List<List<List<byte>>>();

			List<byte> dataDecoded = new List<byte>();
			int currentIndex = 128;
			int colorByte = data[4];
			int colorCount = 256;
			List<List<byte>> colors = new List<List<byte>>();
			if (colorByte == 8)
			{
				colorCount = 256;
			}
			int width = BitConverter.ToInt16(data.GetRange(8, 2).ToArray(), 0) + 1;
			int height = BitConverter.ToInt16(data.GetRange(10, 2).ToArray(), 0) + 1;

			//дешифрование RLE шифра
			for (int i = currentIndex; i < (data.Count - colorCount * 3); i++)
			{
				if (data[i] < 193)
				{
					dataDecoded.Add(data[i]);
				}
				else if (data[i] == 193)
				{
					i++;
					dataDecoded.Add(data[i]);
				}
				else if (data[i] > 193)
				{
					int temp = data[i] - 192;
					i++;
					for (int q = 0; q < temp; q++)
					{
						dataDecoded.Add(data[i]);
					}
				}
			}
			List<List<byte>> pixelsMap = new List<List<byte>>();
			for (int i = 0; i < height; i++)
			{
				pixelsMap.Add(new List<byte>());
				for (int q = 0; q < width; q++)
				{
					pixelsMap[i].Add(dataDecoded[(i * width) + q]);
				}
			}
			int tempIndex = 0;

			for (int i = data.Count - (colorCount * 3); i < data.Count; i += 3)
			{
				colors.Add(new List<byte>());
				for (int q = 0; q < 3; q++)
				{

					colors[tempIndex].Add(data[i + q]);

				}
/*				Console.WriteLine(colors[tempIndex][0] + " | " + colors[tempIndex][1] + " | " + colors[tempIndex][2]);*/
				tempIndex++;
			}
			int index = 0;
			for (int i = 0; i < height; i++)
			{
				pixelsDecoded.Add(new List<List<byte>>());
				for (int j = 0; j < width; j++)
				{
					pixelsDecoded[i].Add(new List<byte>());

					pixelsDecoded[i][j].Add(colors[dataDecoded[index]][0]);
					pixelsDecoded[i][j].Add(colors[dataDecoded[index]][1]);
					pixelsDecoded[i][j].Add(colors[dataDecoded[index]][2]);
					index++;

				}
			}

	/*		Console.WriteLine(data.Count + " | " + dataDecoded.Count + " | " + width + " | " + height);*/


			WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
			outputImage.Source = bitmap;
			bitmap.Lock();
			unsafe
			{
				IntPtr pBackBuffer = bitmap.BackBuffer;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{

						IntPtr pPixel = pBackBuffer + i * bitmap.BackBufferStride + j * 4;

						byte r = colors[pixelsMap[i][j]][0];
						byte g = colors[pixelsMap[i][j]][1];
						byte b = colors[pixelsMap[i][j]][2];
						*((int*)pPixel) = (255 << 24) | (r << 16) | (g << 8) | b;

					}
				}
			}
			bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
			bitmap.Unlock();
		}






		public static List<byte> TCto256(List<byte> data, List<List<List<byte>>> colors, int delta)
		{

			int width = BitConverter.ToInt16(data.GetRange(8, 2).ToArray(), 0) + 1;
			int height = BitConverter.ToInt16(data.GetRange(10, 2).ToArray(), 0) + 1;
			List<byte> result = new List<byte>(data.GetRange(0, 128));

			//сокращение и создание цветовой палитры
			Dictionary<int, List<byte>> trueColorPalett = new Dictionary<int, List<byte>>();

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					List<byte> tempColor = new List<byte> { colors[i][j][0], colors[i][j][1], colors[i][j][2],0 };
					int colorIntId = BitConverter.ToInt32(tempColor.ToArray(), 0);
					if (!trueColorPalett.ContainsKey(colorIntId))
					{
						trueColorPalett.Add(colorIntId, colors[i][j]);
					}
				}
			}
			List<List<int>> colorsID = new List<List<int>>();
			for (int i = 0; i < height; i++)
			{
				colorsID.Add(new List<int>());
				for (int j = 0; j < width; j++)
				{
					List<byte> tempColor = new List<byte> { colors[i][j][0], colors[i][j][1], colors[i][j][2], 0 };
					int colorIntId = BitConverter.ToInt32(tempColor.ToArray(), 0);
					colorsID[i].Add(colorIntId);
				}
			}

			Dictionary<int, int> colorsIDCount = new Dictionary<int, int>();
			for (int i = 0; i < height; i++)
			{
				colorsID.Add(new List<int>());
				for (int j = 0; j < width; j++)
				{
					if (!colorsIDCount.ContainsKey(colorsID[i][j]))
					{
						colorsIDCount.Add(colorsID[i][j], 1);
					} else
					{
						colorsIDCount[colorsID[i][j]]++;
					}
				}
			}



			


			Dictionary<int, int> colorPalettSorted = colorsIDCount.OrderByDescending(kvp => kvp.Value).Take(500).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);






			int q = 0;

			Dictionary<int, int> colors256 = new Dictionary<int, int>();
			foreach (var a in colorPalettSorted)
			{

				if (q == 500)
				{
					break;
				}
				bool flag = false;
				foreach (var b in colorPalettSorted)
				{
					if (Math.Pow((trueColorPalett[a.Key][0] - trueColorPalett[b.Key][0]), 2) + Math.Pow((trueColorPalett[a.Key][1] - trueColorPalett[b.Key][1]), 2) + Math.Pow((trueColorPalett[a.Key][2] - trueColorPalett[b.Key][2]), 2) < delta)
					{
						flag = true;
						/*Console.WriteLine(Math.Pow((trueColorPalett[a.Key][0] - trueColorPalett[b.Key][0]), 2) + Math.Pow((trueColorPalett[a.Key][1] - trueColorPalett[b.Key][1]), 2) + Math.Pow((trueColorPalett[a.Key][2] - trueColorPalett[b.Key][2]), 2));*/
						break; 
					}
				}
				if (flag == false)
				{
					colors256.Add(a.Key, a.Value);
				}
				
				q++;
			}

			Dictionary<int, int> color256PalettSorted = colors256.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			q = 0;
			foreach (var a in color256PalettSorted)
			{

				Console.WriteLine(q + ") " + trueColorPalett[a.Key][0] + " " + trueColorPalett[a.Key][1] + " " + trueColorPalett[a.Key][2] + " | " + a.Value);
				q++;
			}

			Console.WriteLine(colors256.Count + " " + color256PalettSorted.Count + " " + colorPalettSorted.Count);

			List<List<byte>> colorsList256 = new List<List<byte>>();
			q = 0;
			foreach (var a in trueColorPalett)
			{

				colorsList256.Add(trueColorPalett[a.Key]);
				q++;
				if (q == 256)
				{
					break;
				}
			}


			//приведение всех пикселей к существующим цветам
			List<byte> colorsPalett256 = new List<byte>(); 

			for(int i = 0;i<height; i++)
			{

				for(int j = 0; j<height; j++)
				{
					//нахождение минимальной дельты
					int deltaLocal = 10000000;
					byte deltaIndex = 0;
					for(int k = 0; k<256; k++)
					{
						if(deltaLocal < Math.Pow(colors[i][j][0] - colorsList256[k][0], 2) + Math.Pow(colors[i][j][1] - colorsList256[k][1], 2) + Math.Pow(colors[i][j][2] - colorsList256[k][2], 2))
						{
							deltaLocal = (int)(Math.Pow(colors[i][j][0] - colorsList256[k][0], 2) + Math.Pow(colors[i][j][1] - colorsList256[k][1], 2) + Math.Pow(colors[i][j][2] - colorsList256[k][2], 2));
							deltaIndex = (byte)k;
						}

					}
					colorsPalett256.Add(deltaIndex);
				}
			}
			// colorsPalett256 - переписанные под пиксели, но без шифрования
			//шифрование RLE

			List<byte> dataRLE = new List<byte>();
			for(int i = 0; i < colorsPalett256.Count; i++)
			{
				if (i < colorsPalett256.Count - 1 && colorsPalett256[i] != colorsPalett256[i + 1] && colorsPalett256[i] < 192)
				{
					dataRLE.Add(colorsPalett256[i]);
				}
				if(i < colorsPalett256.Count - 1 && colorsPalett256[i] != colorsPalett256[i + 1] && colorsPalett256[i] >= 192)
				{
					dataRLE.Add((byte)192);
					dataRLE.Add(colorsPalett256[i]);
				}
				if(i < colorsPalett256.Count - 1 && colorsPalett256[i] == colorsPalett256[i + 1])
				{
					int g = 1;
					while (colorsPalett256[i] == colorsPalett256[i + g])
					{
						g++;

						if (i + g == colorsPalett256.Count - 1)
						{
							break;
						}
					}
					dataRLE.Add((byte)(192 + g));
					dataRLE.Add(colorsPalett256[i]);
					i += g;
				}
				if( i == colorsPalett256.Count - 1)
				{
					dataRLE.Add(colorsPalett256[i]);
				}
			}

			result.AddRange(dataRLE);
			result.Add(12);
			foreach(var y in colorsList256)
			{
				result.AddRange(y);
			}



			Console.WriteLine(dataRLE.Count + " | " + colorsPalett256.Count + " | " + colorsList256.Count);


			//запись под формат 256 цветов


			return result;
		}

	}
}
