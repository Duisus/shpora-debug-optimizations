using System.Drawing;
using System.Drawing.Imaging;
using JPEG.Classes;

namespace JPEG.Images
{
    public class Matrix
    {
        public readonly Pixel[,] Pixels;
        public readonly int Height;
        public readonly int Width;

        public Matrix(int height, int width)
        {
            Height = height;
            Width = width;

            Pixels = new Pixel[height, width];
        }
        
        public static unsafe explicit operator Matrix(Bitmap bmp)  // TODO refactor
        {
            var height = bmp.Height - bmp.Height % 8;
            var width = bmp.Width - bmp.Width % 8;
            var matrix = new Matrix(height, width);
            
            ImageProcessor.UnsafeProcess(bmp, matrix, ImageLockMode.WriteOnly, 
                (firstByteOfPixel, _matrix, y, x, bytesPerPixel) =>
                {
                    var blue = firstByteOfPixel[x];
                    var green = firstByteOfPixel[x + 1];
                    var red = firstByteOfPixel[x + 2];

                    matrix.Pixels[y, x / bytesPerPixel] = new Pixel(
                        red, green, blue, PixelFormat.RGB);
                });

            return matrix;
        }

        public static unsafe explicit operator Bitmap(Matrix matrix)  // TODO refactor
        {
            var bmp = new Bitmap(matrix.Width, matrix.Height);

            ImageProcessor.UnsafeProcess(bmp, matrix, ImageLockMode.WriteOnly, 
                (firstByteOfPixel, _matrix, y, x, bytesPerPixel) =>
            {
                var pixel = matrix.Pixels[y, x / bytesPerPixel];
                firstByteOfPixel[x] = ToByte(pixel.B);
                firstByteOfPixel[x + 1] = ToByte(pixel.G);
                firstByteOfPixel[x + 2] = ToByte(pixel.R);
                firstByteOfPixel[x + 3] = (byte) 255;
            });

            return bmp;
        }
        
        public void SetPixels(double[,] a, double[,] b, double[,] c, PixelFormat format,
            int yOffset, int xOffset)
        {
            var height = a.GetLength(0);
            var width = a.GetLength(1);

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                Pixels[yOffset + y, xOffset + x] = new Pixel(
                    ToByte(a[y, x]), 
                    ToByte(b[y, x]), 
                    ToByte(c[y, x]), 
                    format);
        }

        private static byte ToByte(double d)
        {
            var val = (int) d;
            if (val > byte.MaxValue)
                return byte.MaxValue;
            if (val < byte.MinValue)
                return byte.MinValue;
            return (byte) val;
        }
    }
}