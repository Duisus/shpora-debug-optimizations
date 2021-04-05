using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using JPEG.Classes;
using JPEG.Images;
using PixelFormat = JPEG.Images.PixelFormat;

namespace JPEG
{
    class Program
    {
        const int CompressionQuality = 70;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(IntPtr.Size == 8 ? "64-bit version" : "32-bit version");
                var sw = Stopwatch.StartNew();
                var fileName = @"BmpImages\\MARBLES.BMP";
                var compressedFileName = fileName + ".compressed." + CompressionQuality;
                var uncompressedFileName = fileName + ".uncompressed." + CompressionQuality + ".bmp";
                var compressor = new JpegCompressor();
                    
                var compressionResult = compressor.Compress(fileName, CompressionQuality);
                compressionResult.Save(compressedFileName);

                sw.Stop();
                Console.WriteLine("Compression: " + sw.Elapsed);
                sw.Restart();
                
                var uncompressedImage = compressor.Uncompress(compressedFileName);
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
    }
}