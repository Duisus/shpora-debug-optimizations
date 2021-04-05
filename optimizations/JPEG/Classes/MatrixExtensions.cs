using System;
using JPEG.Images;

namespace JPEG.Classes
{
    public static class MatrixExtensions
    {
        public static void InsertSubMatrixIn(this Matrix matrix,
            double[,] subMatrixForInsert, int yOffset, int xOffset, Func<Pixel, double> componentSelector)
        {
            var height = subMatrixForInsert.GetLength(0);
            var width = subMatrixForInsert.GetLength(1);

            for (var j = 0; j < height; j++)
            for (var i = 0; i < width; i++)
                subMatrixForInsert[j, i] = componentSelector(matrix.Pixels[yOffset + j, xOffset + i]);
        }
    }
}