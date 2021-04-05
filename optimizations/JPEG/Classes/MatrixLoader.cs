using System.Drawing;
using System.IO;
using JPEG.Images;

namespace JPEG.Classes
{
    public static class MatrixLoader
    {
        public static Matrix FromImage(string imagePath)
        {
            using (var fileStream = File.OpenRead(imagePath)) 
            using (var bmp = (Bitmap) Image.FromStream(fileStream, false, false))
            {
                return (Matrix) bmp;
            }
        }
    }
}