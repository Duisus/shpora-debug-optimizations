using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using BenchmarkDotNet.Attributes;
using JPEG;

namespace JpegBenchmarks
{
    public class JpegBenchmark
    {
        private const string ImagePath = 
            @"C:\Users\denis\Desktop\shpora-debug-optimizations\optimizations\JPEG\sample.bmp";
        private const int CompressionQuality = 70;
        
        private string _compressedFileName;
        private string _uncompressedFileName;

        [GlobalSetup]
        public void Setup()
        {
            var fileName = Path.GetFileName(ImagePath);
            _compressedFileName = fileName + ".compressed." + CompressionQuality;
            _uncompressedFileName = fileName + ".uncompressed." + CompressionQuality + ".bmp";
        }

        [Benchmark]
        public void LoadAndCompressImage()
        {
            var imageMatrix = JPEG.Program.LoadImageAsMatrix(ImagePath);
            var compressionResult = JPEG.Program.Compress(imageMatrix, CompressionQuality);
            compressionResult.Save(_compressedFileName);
        }

        [Benchmark]
        public void LoadAndUncompressImage()
        {
            var compressedImage = CompressedImage.Load(_compressedFileName);
            var uncompressedImage = JPEG.Program.Uncompress(compressedImage);
            var resultBmp = (Bitmap) uncompressedImage;
            resultBmp.Save(_uncompressedFileName, ImageFormat.Bmp);
        }
    }
}