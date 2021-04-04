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
        }
        
        public static unsafe explicit operator Matrix(Bitmap bmp)  // TODO refactor
        {
            var height = bmp.Height - bmp.Height % 8;
            var width = bmp.Width - bmp.Width % 8;
            var matrix = new Matrix(height, width);

            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                bmp.PixelFormat);
            var bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            var heightInPixels = height;
            var widthInBytes = width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*) bmpData.Scan0;

            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = ptrFirstPixel + (y * bmpData.Stride);
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    var blue = currentLine[x];
                    var green = currentLine[x + 1];
                    var red = currentLine[x + 2];

                    matrix.Pixels[y, x / bytesPerPixel] = new Pixel(
                        red, green, blue, PixelFormat.RGB);
                }
            }
            bmp.UnlockBits(bmpData);

            return matrix;
        }

        public static unsafe explicit operator Bitmap(Matrix matrix)  // TODO refactor
        {
            var bmp = new Bitmap(matrix.Width, matrix.Height);

            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly,
                bmp.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int heightInPixels = bmpData.Height;
            int widthInBytes = bmpData.Width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*) bmpData.Scan0;

            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = ptrFirstPixel + (y * bmpData.Stride);
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    var pixel = matrix.Pixels[y, x / bytesPerPixel];
                    currentLine[x] = ToByte(pixel.B);  // todo refactor
                    currentLine[x + 1] = ToByte(pixel.G);
                    currentLine[x + 2] = ToByte(pixel.R);
                    currentLine[x + 3] = (byte) 255;
                }
            }

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static byte ToByte(double d)
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