using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using JPEG.Classes;

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
                Console.WriteLine("Compression: " + sw.ElapsedMilliseconds);
                sw.Restart();
                
                var uncompressedImage = compressor.Uncompress(compressedFileName);
                var resultBmp = (Bitmap) uncompressedImage;
                resultBmp.Save(uncompressedFileName, ImageFormat.Bmp);

                Console.WriteLine("Decompression: " + sw.ElapsedMilliseconds);

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