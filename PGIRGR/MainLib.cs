using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
			Console.WriteLine(dataDecoded.Count);
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

		public static List<byte> TCto256(List<byte> data, List<List<List<byte>>> colorsFull, int delta)
		{



			int width = BitConverter.ToInt16(data.GetRange(8, 2).ToArray(), 0) + 1;
			int height = BitConverter.ToInt16(data.GetRange(10, 2).ToArray(), 0) + 1;
			List<byte> result = new List<byte>(data.GetRange(0, 128));
			Dictionary<int, int> colorsCountsPairs = new Dictionary<int, int>();//цвет в виде инт и число
			/*			for(int i = 0; i < result.Count; i++)
						{
							Console.WriteLine( i + ") " + result[i]);
						}*/
			//result[4] = 8;
			result[65] = 1;
			//result[68] = 1;
			//что вообще с заголовком делать...


			//урезание цветов в 2**12 раз
			Console.WriteLine("начало урезания цветов");
			List<List<List<byte>>> colors = new List<List<List<byte>>>(colorsFull);

			for (int i = 0; i < height; i++)
			{
				for(int j = 0; j<width; j++)
				{
					for(int q = 0; q<3; q++)
					{
						colors[i][j][q] &= 0xF0;
					}
				}
			}


			Console.WriteLine("сортировка цветов");

			//сортировка цветов
			for (int i = 0; i<height; i++)
			{
				for(int j = 0; j<width; j++)
				{
					List<byte> tempListColor = new List<byte>(colors[i][j]);
					tempListColor.Add(0);
					int curCol = BitConverter.ToInt32(tempListColor.ToArray(), 0);
					if (colorsCountsPairs.ContainsKey(curCol))
					{
						colorsCountsPairs[curCol]++;
					} else
					{
						colorsCountsPairs.Add(curCol, 1);
					}
			    }
			}

            Dictionary<int, int> colorPalettSorted = colorsCountsPairs.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			int z = 0;


            KeyValuePair<int, int> nb = new KeyValuePair<int, int>(0, 0);
            foreach (var a in colorPalettSorted)
			{
				if (a.Value != nb.Value)
				{
                    List<byte> tempCol = BitConverter.GetBytes(a.Key).ToList();
                    //Console.WriteLine(z + ") " + tempCol[0] + " " + tempCol[1] + " " + tempCol[2] + " | " + a.Value);

                }
				nb = a;
                z++;
            }
			//получение 256 самых популярных цветов с разницей в дельту

			Console.WriteLine("получение разных цветов");
			int TEMP = 0;
			while (true)
			{
				Console.WriteLine(TEMP);
				TEMP++;
				bool flag = true;
				int w = 0;
				int q = 0;
				foreach (var a in colorPalettSorted)
				{
					w = 0;
					foreach (var b in colorPalettSorted)
					{
						if (w == 0)
						{
							w++;
							continue;
						}
						List<byte> aList = BitConverter.GetBytes(a.Key).ToList();
						List<byte> bList = BitConverter.GetBytes(b.Key).ToList();
						if ((aList[0] - bList[0]) * (aList[0] - bList[0]) + (aList[1] - bList[1])* (aList[1] - bList[1]) + (aList[2] - bList[2]) * (aList[2] - bList[2]) < delta && !(aList[0] == bList[0] && aList[1] == bList[1] && aList[2] == bList[2]))
						{
							colorPalettSorted.Remove(a.Key);
							//Console.WriteLine(aList[0] + " " + aList[1] + " " + aList[2] + " " + a.Value + " | " + bList[0] + " " + bList[1] + " " + bList[2] + " " + b.Value);
							flag = false;
							break;
						}
						w++;
						if (w >= q)
						{
							break;
						}
					}

					q++;
					if (q == 255 || !flag)
					{
						break;
					}

				}

				if (flag)
				{
					break;
				}
				else
				{
					/*Console.Write(q + " " + w + " | ");*/
				}
			}

			Console.WriteLine("замена всех цветов");
			z = 0;
			List<List<byte>> palett256 = new List<List<byte>>();
			foreach (var a in colorPalettSorted.Take(256))
            {
                List<byte> tempCol = BitConverter.GetBytes(a.Key).ToList();
               // Console.WriteLine(z + ") " + tempCol[0] + " " + tempCol[1] + " " + tempCol[2] + " | " + a.Value);
                palett256.Add(new List<byte> {tempCol[0], tempCol[1], tempCol[2]});
				z++;
            }
			
			//приведение всех цветов к ближайшим к 256

			List<List<byte>> colors256 = new List<List<byte>>();

			for (int i = 0; i < height; i++)
			{
				colors256.Add(new List<byte>());
				for (int j = 0; j < width; j++)
				{
					List<byte> tempColor = colors[i][j];
					int minDelta = int.MaxValue;
					int tempIndex = 0;
					for (int q = 0; q < 256; q++)
					{
						int tDelta = (int)((colors[i][j][0] - palett256[q][0]) * (colors[i][j][0] - palett256[q][0]) + (colors[i][j][1] - palett256[q][1]) * (colors[i][j][1] - palett256[q][1]) + (colors[i][j][2] - palett256[q][2])* (colors[i][j][2] - palett256[q][2]));
						if(tDelta< minDelta)
						{
							tempColor = palett256[q];
							tempIndex = q;
							minDelta = tDelta;
						}

					}
					colors256[i].Add((byte)tempIndex);

				}

			}
			List<byte> dataTemp = new List<byte>();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {

					dataTemp.Add(colors256[i][j]);
					
                }

            }

			// RLE сжатие

			Console.WriteLine("RLE сжатие");
			List<byte> dataRLE = new List<byte>();
			for (int i = 0; i < dataTemp.Count; i++)
			{

				if (i < dataTemp.Count - 1)
				{

					if (dataTemp[i] >= 193)
					{
						if (dataTemp[i] != dataTemp[i + 1])
						{
							dataRLE.Add(193);
							dataRLE.Add((byte)dataTemp[i]);
						}
						else
						{
							int t = 0;
							while (i + t + 1 < dataTemp.Count && dataTemp[i] == dataTemp[i + t] && t<63)
							{
								t++;

							}
							dataRLE.Add((byte)(192 + t));
							dataRLE.Add(((byte)dataTemp[i]));
							i += t-1;
						}

					}
					else
					{
						dataRLE.Add((byte)dataTemp[i]);
					}

				}
				else
				{
					if (dataTemp[i] >= 193)
					{
						dataRLE.Add(193);
						dataRLE.Add((byte)dataTemp[i]);
					}
					else
					{
						dataRLE.Add((byte)dataTemp[i]);
					}
				}

			}
				

			//палитра под запись
			List<byte> palettPCX = new List<byte>();
			for(int i = 0; i<palett256.Count; i++)
			{
				for(int j = 0; j<3; j++)
				{
					palettPCX.Add(palett256[i][j]);
				}
			}
			//запись под формат файла PCX
			result.AddRange(dataRLE);
			result.Add(12);
			result.AddRange(palettPCX);

			return result;
		}

	}
}
