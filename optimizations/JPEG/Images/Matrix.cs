using System.Drawing;
using System.Drawing.Imaging;

namespace JPEG.Images
{
    public class Matrix //TODO delete public
    {
        public readonly Pixel[,] Pixels;
        public readonly int Height;
        public readonly int Width;

        public Matrix(int height, int width)
        {
            Height = height;
            Width = width;

            Pixels = new Pixel[height, width];
            for (var i = 0; i < height; ++i)
            for (var j = 0; j < width; ++j)
                Pixels[i, j] = new Pixel(0, 0, 0, PixelFormat.RGB);
        }

        public static unsafe explicit operator Matrix(Bitmap bmp)  // TODO refactor
        {
            var height = bmp.Height - bmp.Height % 8;
            var width = bmp.Width - bmp.Width % 8;
            var matrix = new Matrix(height, width);

            var bitmapData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                bmp.PixelFormat);
            var bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            //var heightInPixels = bitmapData.Height;
            //var widthInBytes = bitmapData.Width * bytesPerPixel;
            var heightInPixels = height;
            var widthInBytes = width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*) bitmapData.Scan0;

            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    var blue = currentLine[x];
                    var green = currentLine[x + 1];
                    var red = currentLine[x + 2];

                    matrix.Pixels[y, x / bytesPerPixel] = new Pixel(
                        red, green, blue, PixelFormat.RGB);
                }
            }

            return matrix;
        }

        public static unsafe explicit operator Bitmap(Matrix matrix)  // TODO refactor
        {
            var bmp = new Bitmap(matrix.Width, matrix.Height);

            var bitmapData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly,
                bmp.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*) bitmapData.Scan0;

            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    var pixel = matrix.Pixels[y, x / bytesPerPixel];
                    currentLine[x] = (byte) ToByte(pixel.B);
                    currentLine[x + 1] = (byte) ToByte(pixel.G);
                    currentLine[x + 2] = (byte) ToByte(pixel.R);
                    currentLine[x + 3] = (byte)255;
                }
            }

            bmp.UnlockBits(bitmapData);

            return bmp;
        }

        public static int ToByte(double d)
        {
            var val = (int) d;
            if (val > byte.MaxValue)
                return byte.MaxValue;
            if (val < byte.MinValue)
                return byte.MinValue;
            return val;
        }
    }
}