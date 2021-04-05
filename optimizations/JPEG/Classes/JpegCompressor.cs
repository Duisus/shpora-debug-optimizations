using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JPEG.Images;

namespace JPEG.Classes
{
    public class JpegCompressor
    {
        private const int DCTSize = 8;

        private readonly Func<Pixel, double>[] _selectors =
        {
            p => p.Y, 
            p => p.Cb, 
            p => p.Cr
        };
        
        public CompressedImage Compress(string imagePath, int quality = 50)
        {
            var imageMatrix = MatrixLoader.FromImage(imagePath);
            return Compress(imageMatrix, quality);
        }

        public CompressedImage Compress(Matrix matrix, int quality = 50) 
        {
            var size = matrix.Height * matrix.Width * _selectors.Length;
            var allQuantizedBytes = new byte[size];
            var quantizer = new Quantizer(quality);

            Parallel.For(0, matrix.Height / DCTSize, (lineNum) =>
                ProcessImageForCompress(allQuantizedBytes, matrix, quantizer, lineNum));
            
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

        public Matrix Uncompress(string compressedImagePath)
        {
            var compressedImage = CompressedImage.Load(compressedImagePath);
            return Uncompress(compressedImage);
        }

        public Matrix Uncompress(CompressedImage image)
        {
            var result = new Matrix(image.Height, image.Width);
            var quantizer = new Quantizer(image.Quality);
            
            using (var allQuantizedBytes = new MemoryStream(
                HuffmanCodec.Decode(image.CompressedBytes, image.DecodeTable, image.BitsCount)))
            {
                Parallel.For(0, image.Height / DCTSize, (lineNum) => 
                    ProcessImageForUncompress(allQuantizedBytes, result, quantizer, lineNum));
            }
            
            return result;
        }

        private void ProcessImageForCompress(
            byte[] allQuantizedBytes, Matrix matrix, Quantizer quantizer, int lineNum)
        {
            var y = lineNum * DCTSize;
            var subMatrix = new double[DCTSize, DCTSize];
            var channelFreqs = new double[DCTSize, DCTSize];
            var quantizedFreqs = new byte[DCTSize, DCTSize];
            var quantizedBytes = new byte[DCTSize * DCTSize];
                
            for (int x = 0; x < matrix.Width; x += DCTSize)
            for (int i = 0; i < _selectors.Length; i++)
            {
                matrix.InsertSubMatrixIn(subMatrix, y, x, _selectors[i]);
                ShiftMatrixValues(subMatrix, -128);
                DCT.DCT2D(subMatrix, channelFreqs);
                quantizer.QuantizeIn(channelFreqs, quantizedFreqs);
                ZigZagScanner.ScanIn(quantizedFreqs, quantizedBytes);
                
                quantizedBytes.CopyTo(allQuantizedBytes, CalcInsertIndex(matrix.Width, y, x, i)); 
            }
        }

        private void ProcessImageForUncompress(
            MemoryStream allQuantizedBytes, Matrix resultMatrix, Quantizer quantizer, int lineNum)
        {
            var channels = GetChannels();
            var quantizedBytes = new byte[DCTSize * DCTSize];
            var y = lineNum * DCTSize;
            var channelFreqs = new double[DCTSize, DCTSize];
            var quantizedFreqs = new byte[DCTSize, DCTSize];
                    
            for (var x = 0; x < resultMatrix.Width; x += DCTSize)
            {
                for (int i = 0; i < channels.Length; i++)
                {
                    lock (allQuantizedBytes)
                    {
                        allQuantizedBytes.Position = CalcInsertIndex(resultMatrix.Width, y, x, i);
                        allQuantizedBytes.Read(quantizedBytes, 0, quantizedBytes.Length);   
                    }
                    ZigZagScanner.UnScanIn(quantizedBytes, quantizedFreqs);
                    quantizer.DeQuantizeIn(quantizedFreqs, channelFreqs);
                    DCT.IDCT2D(channelFreqs, channels[i]);
                    ShiftMatrixValues(channels[i], 128);
                }
        
                resultMatrix.SetPixels(channels[0], channels[1], channels[2], PixelFormat.YCbCr, y, x);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalcInsertIndex(int imageWidth, int y, int x, int blockNum)
        {
            return imageWidth * y * 3 + x * 8 * 3 + blockNum * 64;
        }

        private double[][,] GetChannels()
        {
            var y = new double[DCTSize, DCTSize];
            var cb = new double[DCTSize, DCTSize];
            var cr = new double[DCTSize, DCTSize];
            return new[] {y, cb, cr};
        }

        private void ShiftMatrixValues(double[,] subMatrix, int shiftValue)  // TODO move to extension method of double[,]?
        {
            var height = subMatrix.GetLength(0);
            var width = subMatrix.GetLength(1);

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                subMatrix[y, x] = subMatrix[y, x] + shiftValue;
        }
    }
}