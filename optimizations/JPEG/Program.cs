using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using JPEG.Images;
using PixelFormat = JPEG.Images.PixelFormat;

namespace JPEG
{
    public class Program //TODO delete public
    {
        const int CompressionQuality = 70;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(IntPtr.Size == 8 ? "64-bit version" : "32-bit version");
                var sw = Stopwatch.StartNew();
                var fileName = @"BmpImages\\MARBLES.BMP";
                //var fileName = "Big_Black_River_Railroad_Bridge.bmp";
                var compressedFileName = fileName + ".compressed." + CompressionQuality;
                var uncompressedFileName = fileName + ".uncompressed." + CompressionQuality + ".bmp";

                var imageMatrix = LoadImageAsMatrix(fileName);
                var compressionResult = Compress(imageMatrix, CompressionQuality); //TODO create class Compressor
                compressionResult.Save(compressedFileName);

                sw.Stop();
                Console.WriteLine("Compression: " + sw.Elapsed);
                sw.Restart();

                var compressedImage = CompressedImage.Load(compressedFileName);
                var uncompressedImage = Uncompress(compressedImage);
                var resultBmp = (Bitmap) uncompressedImage;
                resultBmp.Save(uncompressedFileName, ImageFormat.Bmp);

                Console.WriteLine("Decompression: " + sw.Elapsed);

                Console.WriteLine($"Peak commit size: {MemoryMeter.PeakPrivateBytes() / (1024.0 * 1024):F2} MB");
                Console.WriteLine($"Peak working set: {MemoryMeter.PeakWorkingSet() / (1024.0 * 1024):F2} MB");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static Matrix LoadImageAsMatrix(string fileName) //TODO delete public
        {
            using (var fileStream = File.OpenRead(fileName))
            //using (var fileStream = new MemoryStream(File.ReadAllBytes(fileName))) todo
            using (var bmp = (Bitmap) Image.FromStream(fileStream, false, false))
            {
                Console.WriteLine($"{bmp.Width}x{bmp.Height} - {fileStream.Length / (1024.0 * 1024):F2} MB");
                return (Matrix) bmp;
            }
        }

        public static CompressedImage Compress(Matrix matrix, int quality = 50) 
        {
            var selectors = new Func<Pixel, double>[] {p => p.Y, p => p.Cb, p => p.Cr};
            var size = matrix.Height * matrix.Width * selectors.Length;
            var allQuantizedBytes = new byte[size];

            Parallel.For(0, matrix.Height / DCTSize, (lineNum) =>
            {
                var y = lineNum * DCTSize;
                var subMatrix = new double[DCTSize, DCTSize];
                var channelFreqs = new double[DCTSize, DCTSize];
                var quantizedFreqs = new byte[DCTSize, DCTSize];
                var quantizedBytes = new byte[DCTSize * DCTSize];
                
                for (int x = 0; x < matrix.Width; x += DCTSize)
                {
                    for (int i = 0; i < selectors.Length; i++)
                    {
                        InsertSubMatrixIn(subMatrix, matrix, y, x, selectors[i]);
                        ShiftMatrixValues(subMatrix, -128);  // TODO combine GetSubMatrix and ShiftMatrixValues ?
                        DCT.DCT2D(subMatrix, channelFreqs);
                        QuantizeIn(channelFreqs, quality, quantizedFreqs);
                        ZigZagScanIn(quantizedFreqs, quantizedBytes);
                        var index = matrix.Width * y * 3 + x * 8 * 3 + i * 64;
                        quantizedBytes.CopyTo(allQuantizedBytes, index); 
                    }
                }
            });

            QuantizationMatrix = null; // TODO refactor (create Quantizer class?)

            long bitsCount;
            Dictionary<BitsWithLength, byte> decodeTable;
            var compressedBytes = HuffmanCodec.Encode(allQuantizedBytes, out decodeTable, out bitsCount);

            return new CompressedImage
            {
                Quality = quality,
                CompressedBytes = compressedBytes,
                BitsCount = bitsCount,
                DecodeTable = decodeTable,
                Height = matrix.Height,
                Width = matrix.Width
            };
        }

        public static Matrix Uncompress(CompressedImage image) //TODO delete public
        {
            var result = new Matrix(image.Height, image.Width);
            using (var allQuantizedBytes =
                new MemoryStream(HuffmanCodec.Decode(image.CompressedBytes, image.DecodeTable, image.BitsCount)))  // TODO MemoryStream?
            {
                Parallel.For(0, image.Height / DCTSize, (lineNum) =>
                {
                    var _y = new double[DCTSize, DCTSize];
                    var cb = new double[DCTSize, DCTSize];
                    var cr = new double[DCTSize, DCTSize];
                    var channels = new[] {_y, cb, cr};
                    var quantizedBytes = new byte[DCTSize * DCTSize];
                    var y = lineNum * DCTSize;
                    var channelFreqs = new double[DCTSize, DCTSize];
                    var quantizedFreqs = new byte[DCTSize, DCTSize];
                    
                    for (var x = 0; x < image.Width; x += DCTSize)
                    {
                        for (int i = 0; i < channels.Length; i++)
                        {
                            lock (allQuantizedBytes)
                            {
                                var offset = image.Width * y * 3 + x * 8 * 3 + i * 64;
                                allQuantizedBytes.Position = offset;
                                allQuantizedBytes.Read(quantizedBytes, 0, quantizedBytes.Length);   
                            }
                            ZigZagUnScanIn(quantizedBytes, quantizedFreqs);
                            DeQuantizeIn(quantizedFreqs, image.Quality, channelFreqs);
                            DCT.IDCT2D(channelFreqs, channels[i]);
                            ShiftMatrixValues(channels[i], 128);
                        }

                        SetPixels(result, _y, cb, cr, PixelFormat.YCbCr, y, x);
                    }
                });
            }

            QuantizationMatrix = null; // TODO refactor (create Quantizer class?)

            return result;
        }

        private static void ShiftMatrixValues(double[,] subMatrix, int shiftValue)
        {
            var height = subMatrix.GetLength(0);
            var width = subMatrix.GetLength(1);

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                subMatrix[y, x] = subMatrix[y, x] + shiftValue;
        }

        private static void SetPixels(Matrix matrix, double[,] a, double[,] b, double[,] c, PixelFormat format,
            int yOffset, int xOffset)
        {
            var height = a.GetLength(0);
            var width = a.GetLength(1);

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                matrix.Pixels[yOffset + y, xOffset + x] = new Pixel(
                    Matrix.ToByte(a[y, x]),  //todo refactor
                    Matrix.ToByte(b[y, x]),
                    Matrix.ToByte(c[y, x]),
                    format);
        }

        private static void InsertSubMatrixIn(double[,] subMatrixForInsert, // todo delete
            Matrix matrix, int yOffset, int xOffset, Func<Pixel, double> componentSelector)
        {
            for (var j = 0; j < DCTSize; j++)
            for (var i = 0; i < DCTSize; i++)
                subMatrixForInsert[j, i] = componentSelector(matrix.Pixels[yOffset + j, xOffset + i]);
        }

        private static readonly int[] indexesX =
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
            
        private static readonly int[] indexesY =
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
        
        private static void ZigZagScanIn(byte[,] channelFreqs, byte[] output)
        {
            for (int i = 0; i < indexesX.Length; i++)
            {
                output[i] = channelFreqs[indexesY[i], indexesX[i]];
            }
        }

        private static int[,] indexesForUnScan =
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
        
        private static void ZigZagUnScanIn(IReadOnlyList<byte> quantizedBytes, byte[,] output)
        {
            for (int i = 0; i < DCTSize; i++)
            for (int j = 0; j < DCTSize; j++)
            {
                output[i, j] = quantizedBytes[indexesForUnScan[i, j]];
            }
        }

        private static void QuantizeIn(double[,] channelFreqs, int quality, byte[,] output)
        {
            var height = channelFreqs.GetLength(0);
            var width = channelFreqs.GetLength(1);
            var quantizationMatrix = QuantizationMatrix
                                     ?? (QuantizationMatrix = GetQuantizationMatrix(quality));  // TODO refactor

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    output[y, x] = (byte) (channelFreqs[y, x] / quantizationMatrix[y, x]);
                }
            }
        }
        
        private static void DeQuantizeIn(byte[,] quantizedBytes, int quality, double[,] output)
        {
            var height = quantizedBytes.GetLength(0);
            var width = quantizedBytes.GetLength(1);
            var quantizationMatrix = QuantizationMatrix
                                     ?? (QuantizationMatrix = GetQuantizationMatrix(quality)); // TODO refactor

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    output[y, x] =
                        ((sbyte) quantizedBytes[y, x]) *
                        quantizationMatrix[y, x]; //NOTE cast to sbyte not to loose negative numbers
                }
            }
        }

        private static int[,] QuantizationMatrix;

        private static int[,] GetQuantizationMatrix(int quality)
        {
            if (quality < 1 || quality > 99)
                throw new ArgumentException("quality must be in [1,99] interval");

            var multiplier = quality < 50 ? 5000 / quality : 200 - 2 * quality;

            var result = new[,]
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

            for (int y = 0; y < result.GetLength(0); y++) // TODO optimize GetLength ???
            {
                for (int x = 0; x < result.GetLength(1); x++) // TODO optimize GetLength ???
                {
                    result[y, x] = (multiplier * result[y, x] + 50) / 100;
                }
            }

            return result;
        }

        const int DCTSize = 8;
    }
}