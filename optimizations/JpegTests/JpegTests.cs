using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using FluentAssertions;
using JPEG;
using NUnit.Framework;

namespace JpegTests
{
    public class JpegTests  //TODO add tests for earth.bmp
    {
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
        
        private static void CompressTest(string imageFilePath, int compressionQuality = 70)
        {
            var compressedFilePath = GetCompressedFilePath(imageFilePath, compressionQuality);
            var imageMatrix = JPEG.Program.LoadImageAsMatrix(imageFilePath);
            
            var compressionResult = JPEG.Program.Compress(imageMatrix, compressionQuality);
            compressionResult.Save(compressedFilePath + ".test");  //TODO add teardown to delete file
            var comressedImageBytes = File.ReadAllBytes(compressedFilePath + ".test");
            
            var expectedResult = File.ReadAllBytes(compressedFilePath);
            comressedImageBytes.Should().BeEquivalentTo(
                expectedResult,
                config => config.WithStrictOrdering());
        }

        private static void UncompressTest(string imageFilePath, int compressionQuality = 70)
        {
            var compressedImage = CompressedImage.Load(
                GetCompressedFilePath(imageFilePath, compressionQuality));

            var uncompressedImage = JPEG.Program.Uncompress(compressedImage);
            var resultBmp = (Bitmap) uncompressedImage;
            
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

        private static bool BitmapEquals(Bitmap first, Bitmap second)  //TODO find other ready method
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

        private static object[] _imageFilePaths =
        {
            @"TestData\sample.bmp",  //TODO separate origin and compressed images
            @"TestData\MARBLES.BMP"
        };
        
        private static object[] _compressionQualities =
        {
            10,
            50,
            75
        };
    }
}