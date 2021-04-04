using System;
using System.Linq;

namespace JPEG.Images
{
    public struct Pixel
    {
        public readonly PixelFormat Format;
        private static readonly PixelFormat[] supportedFormats = {PixelFormat.RGB, PixelFormat.YCbCr};
        
        public Pixel(byte firstComponent, byte secondComponent, byte thirdComponent, PixelFormat pixelFormat)
        {
            if (!supportedFormats.Contains(pixelFormat))
                throw new FormatException("Unknown pixel format: " + pixelFormat);
            Format = pixelFormat;
            
            this.firstComponent = firstComponent;
            this.secondComponent = secondComponent;
            this.thirdComponent = thirdComponent;
        }

        private readonly byte firstComponent;
        private readonly byte secondComponent;
        private readonly byte thirdComponent;

        public double R =>
            Format == PixelFormat.RGB 
                ? firstComponent
                : (298.082 * firstComponent + 408.583 * thirdComponent) / 256.0 - 222.921;
        public double G =>
            Format == PixelFormat.RGB 
                ? secondComponent 
                : (298.082 * firstComponent - 100.291 * secondComponent - 208.120 * thirdComponent) / 256.0 + 135.576;
        public double B => 
            Format == PixelFormat.RGB 
                ? thirdComponent 
                : (298.082 * firstComponent + 516.412 * secondComponent) / 256.0 - 276.836;
        public double Y => 
            Format == PixelFormat.YCbCr 
                ? firstComponent 
                : 16.0 + (65.738 * firstComponent + 129.057 * secondComponent + 24.064 * thirdComponent) / 256.0;
        public double Cb => 
            Format == PixelFormat.YCbCr 
                ? secondComponent 
                : 128.0 + (-37.945 * firstComponent - 74.494 * secondComponent + 112.439 * thirdComponent) / 256.0;
        public double Cr => 
            Format == PixelFormat.YCbCr 
                ? thirdComponent 
                : 128.0 + (112.439 * firstComponent - 94.154 * secondComponent - 18.285 * thirdComponent) / 256.0;
    }
}