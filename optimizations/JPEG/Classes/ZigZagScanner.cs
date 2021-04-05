using System.Collections.Generic;

namespace JPEG.Classes
{
    public static class ZigZagScanner
    {
        private static readonly int[] ScanIndexesX =  //TODO use ImmutableArray ???
        {
            0,1,0,0,1,2,3,2,
            1,0,0,1,2,3,4,5,
            4,3,2,1,0,0,1,2,
            3,4,5,6,7,6,5,4,
            3,2,1,0,1,2,3,4,
            5,6,7,7,6,5,4,3,
            2,3,4,5,6,7,7,6,
            5,4,5,6,7,7,6,7
        };
            
        private static readonly int[] ScanIndexesY =
        {
            0,0,1,2,1,0,0,1,
            2,3,4,3,2,1,0,0,
            1,2,3,4,5,6,5,4,
            3,2,1,0,0,1,2,3,
            4,5,6,7,7,6,5,4,
            3,2,1,2,3,4,5,6,
            7,7,6,5,4,3,4,5,
            6,7,7,6,5,6,7,7
        };

        private static readonly int[,] UnScanIndexes =
        {
            {0, 1, 5, 6, 14, 15, 27, 28},
            {2, 4, 7, 13, 16, 26, 29, 42},
            {3, 8, 12, 17, 25, 30, 41, 43},
            {9, 11, 18, 24, 31, 40, 44, 53},
            {10, 19, 23, 32, 39, 45, 52, 54},
            {20, 22, 33, 38, 46, 51, 55, 60},
            {21, 34, 37, 47, 50, 56, 59, 61},
            {35, 36, 48, 49, 57, 58, 62, 63}
        };

        public static void ScanIn(byte[,] channelFreqs, byte[] output)
        {
            for (int i = 0; i < ScanIndexesX.Length; i++)
            {
                output[i] = channelFreqs[ScanIndexesY[i], ScanIndexesX[i]];
            }
        }

        public static void UnScanIn(IReadOnlyList<byte> quantizedBytes, byte[,] output)
        {
            var height = UnScanIndexes.GetLength(0);
            var width = UnScanIndexes.GetLength(1);

            for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
            {
                output[i, j] = quantizedBytes[UnScanIndexes[i, j]];
            }
        }
    }
}