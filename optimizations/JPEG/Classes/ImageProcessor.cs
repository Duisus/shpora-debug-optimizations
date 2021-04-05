using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using JPEG.Images;

namespace JPEG.Classes
{
    public static class ImageProcessor
    {
        public unsafe delegate void ProcessPixel(
            byte* firstByteOfPixel, Matrix matrix, int y, int x, int bytesPerPixel);
        
        public static unsafe void UnsafeProcess(
            Bitmap bmp, Matrix matrix, ImageLockMode lockMode, ProcessPixel processPixel)
        {
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                lockMode,
                bmp.PixelFormat);
            
            int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int heightInPixels = matrix.Height;
            int widthInBytes = matrix.Width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*) bmpData.Scan0;

            Parallel.For(0, heightInPixels, y =>
            {
                byte* currentLine = ptrFirstPixel + (y * bmpData.Stride);
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    processPixel(currentLine, matrix, y, x, bytesPerPixel);   
                }
            });

            bmp.UnlockBits(bmpData);
        }
    }
}