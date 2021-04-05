using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using BenchmarkDotNet.Attributes;
using JPEG;
using JPEG.Classes;

namespace JpegBenchmarks
{
    [MemoryDiagnoser]
    public class JpegBenchmark
    {
        //todo add params
        private const string ImagePath = @"BenchmarkImages\MARBLES.BMP";
        private const int CompressionQuality = 70;

        private JpegCompressor _compressor;
        private string _compressedFileName;
        private string _uncompressedFileName;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _compressor = new JpegCompressor();
            
            var fileName = Path.GetFileName(ImagePath);
            _compressedFileName = fileName + ".compressed." + CompressionQuality;
            _uncompressedFileName = fileName + ".uncompressed." + CompressionQuality + ".bmp";
        }

        [Benchmark]
        public void LoadAndCompressImage()
        {
            var compressionResult = _compressor.Compress(ImagePath, CompressionQuality);
            compressionResult.Save(_compressedFileName);
        }

        [Benchmark]
        public void LoadAndUncompressImage()
        {
            var uncompressedImage = _compressor.Uncompress(_compressedFileName);
            var resultBmp = (Bitmap) uncompressedImage;
            resultBmp.Save(_uncompressedFileName, ImageFormat.Bmp);
        }
    }
}