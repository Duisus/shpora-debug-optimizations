using System;

namespace JPEG.Classes
{
    public class Quantizer
    {
        public readonly int CompressionQuality;
        
        private readonly int[,] _quantizationMatrix;

        private static readonly int[,] BaseQuantizationMatrix =
        {
            {16, 11, 10, 16, 24, 40, 51, 61},
            {12, 12, 14, 19, 26, 58, 60, 55},
            {14, 13, 16, 24, 40, 57, 69, 56},
            {14, 17, 22, 29, 51, 87, 80, 62},
            {18, 22, 37, 56, 68, 109, 103, 77},
            {24, 35, 55, 64, 81, 104, 113, 92},
            {49, 64, 78, 87, 103, 121, 120, 101},
            {72, 92, 95, 98, 112, 100, 103, 99}
        };
        
        public Quantizer(int compressionQuality)
        {
            CompressionQuality = compressionQuality;
            _quantizationMatrix = GetQuantizationMatrix(compressionQuality);
        }
        
        public void QuantizeIn(double[,] channelFreqs, byte[,] output)
        {
            var height = channelFreqs.GetLength(0);
            var width = channelFreqs.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    output[y, x] = (byte) (channelFreqs[y, x] / _quantizationMatrix[y, x]);
                }
            }
        }
        
        public void DeQuantizeIn(byte[,] quantizedBytes, double[,] output)
        {
            var height = quantizedBytes.GetLength(0);
            var width = quantizedBytes.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //NOTE cast to sbyte not to loose negative numbers
                    output[y, x] = ((sbyte) quantizedBytes[y, x]) * _quantizationMatrix[y, x]; 
                }
            }
        }

        private static int[,] GetQuantizationMatrix(int quality)
        {
            if (quality < 1 || quality > 99)
                throw new ArgumentException("quality must be in [1,99] interval");

            var multiplier = quality < 50 ? 5000 / quality : 200 - 2 * quality;

            var result = new int[8, 8];

            var width = result.GetLength(1);
            var height = result.GetLength(0);
            
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                result[y, x] = (multiplier * BaseQuantizationMatrix[y, x] + 50) / 100;
            }

            return result;
        }
    }
}