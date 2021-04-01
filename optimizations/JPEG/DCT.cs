using System;
using JPEG.Utilities;

namespace JPEG
{
	public class DCT
	{
		public static double[,] DCT2D(double[,] input)
		{
			var height = input.GetLength(0);
			var width = input.GetLength(1);
			var coeffs = new double[width, height];
			var beta = Beta(height, width);

			for (int i = 0; i < width; i++)
			for (int j = 0; j < height; j++)
			{
				double sum = 0;
				for (int x = 0; x < width; x++)
				for (int y = 0; y < height; y++)
				{
					sum += BasisFunction(input[x, y], i, j, x, y, height, width);
				}

				coeffs[i, j] = sum * beta * Alpha(i) * Alpha(j);  // TODO optimize Alpha
			}

			return coeffs;
		}

		public static void IDCT2D(double[,] coeffs, double[,] output)
		{
			var height = coeffs.GetLength(0);
			var width = coeffs.GetLength(1);
			var beta = Beta(height, width);
			
			for(var x = 0; x < width; x++)
			{
				for(var y = 0; y < height; y++)
				{
					double sum = 0;
					for (int i = 0; i < width; i++)
					for (int j = 0; j < height; j++)
					{
						sum += BasisFunction(coeffs[i, j], i, j, x, y, height, width)
						       * Alpha(i) * Alpha(j);  // TODO optimize alpha
					}
					
					output[x, y] = sum * beta;
				}
			}
		}

		public static double BasisFunction(double a, double u, double v, double x, double y, int height, int width)
		{
			var b = Math.Cos(((2d * x + 1d) * u * Math.PI) / (2 * width));
			var c = Math.Cos(((2d * y + 1d) * v * Math.PI) / (2 * height));

			return a * b * c;
		}

		private static double Alpha(int u)
		{
			if(u == 0)
				return 1 / Math.Sqrt(2);
			return 1;
		}

		private static double Beta(int height, int width)
		{
			return 1d / width + 1d / height;
		}
	}
}