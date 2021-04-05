using System.Drawing;
using System.IO;
using FluentAssertions;
using JPEG.Classes;
using NUnit.Framework;

namespace JpegTests
{
    public class JpegTests  //TODO add tests for earth.bmp
    {
        private JpegCompressor _compressor;

        [SetUp]
        public void SetUp()
        {
            _compressor = new JpegCompressor();
        }
        
        [TestCaseSource(nameof(_imageFilePaths))]
        public void Compress_CorrectCompressImage(string imageFilePath)
        {
            CompressTest(imageFilePath);
        }

        [TestCaseSource(nameof(_imageFilePaths))]
        public void Uncompress_CorrectUncompressImage(string imageFilePath)
        {
            UncompressTest(imageFilePath);
        }
        
        [TestCaseSource(nameof(_compressionQualities))]
        public void Compress_WithDifferentCompressionQuality_CorrectCompressImage(int compressionQuality)
        {
            CompressTest(@"TestData\sample.bmp", compressionQuality);
        }

        [TestCaseSource(nameof(_compressionQualities))]
        public void Uncompress_WithDifferentCompressionQuality_CorrectUncompressImage(int compressionQuality)
        {
            UncompressTest(@"TestData\sample.bmp", compressionQuality);
        }
        
        private void CompressTest(string imageFilePath, int compressionQuality = 70)
        {
            var compressedFilePath = GetCompressedFilePath(imageFilePath, compressionQuality);

            var compressionResult = _compressor.Compress(imageFilePath, compressionQuality);
            compressionResult.Save(compressedFilePath + ".test");  //TODO add teardown to delete file
            
            var comressedImageBytes = File.ReadAllBytes(compressedFilePath + ".test");
            var expectedResult = File.ReadAllBytes(compressedFilePath);
            comressedImageBytes.Should().BeEquivalentTo(
                expectedResult,
                config => config.WithStrictOrdering());
        }

        private void UncompressTest(string imageFilePath, int compressionQuality = 70)
        {
            var compressedFilePath = GetCompressedFilePath(imageFilePath, compressionQuality);
            
            var resultBmp = (Bitmap) _compressor.Uncompress(compressedFilePath);

            var expectedResult = (Bitmap) Image.FromFile(
                GetUncompressedFilePath(imageFilePath, compressionQuality));
            BitmapEquals(resultBmp, expectedResult).Should().BeTrue();
        }

        private static string GetCompressedFilePath(string filePath, int compressionQuality)
        {
            return filePath + ".compressed." + compressionQuality;
        }

        private static string GetUncompressedFilePath(string filePath, int compressionQuality)
        {
            return filePath + ".uncompressed." + compressionQuality + ".bmp";
        }

        private static bool BitmapEquals(Bitmap first, Bitmap second)
        {
            if (first.Width != second.Width || first.Height != second.Height)
                return false;

            for (int x = 0; x < first.Width; x++)
            for (int y = 0; y < first.Height; y++)
            {
                if (first.GetPixel(x, y) != second.GetPixel(x, y))
                    return false;
            }

            return true;
        }

        private static object[] _imageFilePaths =  //todo use TestCases
        {
            @"TestData\sample.bmp",
            @"TestData\MARBLES.BMP"
        };
        
        private static object[] _compressionQualities =  //todo use TestCases
        {
            10,
            50,
            75
        };
    }
}