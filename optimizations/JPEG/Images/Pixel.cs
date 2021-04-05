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
            
            this.FirstComponent = firstComponent;
            this.SecondComponent = secondComponent;
            this.ThirdComponent = thirdComponent;
        }

        public readonly byte FirstComponent;
        public readonly byte SecondComponent;
        public readonly byte ThirdComponent;

        public double R =>
            Format == PixelFormat.RGB 
                ? FirstComponent
                : (298.082 * FirstComponent + 408.583 * ThirdComponent) / 256.0 - 222.921;
        public double G =>
            Format == PixelFormat.RGB 
                ? SecondComponent 
                : (298.082 * FirstComponent - 100.291 * SecondComponent - 208.120 * ThirdComponent) / 256.0 + 135.576;
        public double B => 
            Format == PixelFormat.RGB 
                ? ThirdComponent 
                : (298.082 * FirstComponent + 516.412 * SecondComponent) / 256.0 - 276.836;
        public double Y => 
            Format == PixelFormat.YCbCr 
                ? FirstComponent 
                : 16.0 + (65.738 * FirstComponent + 129.057 * SecondComponent + 24.064 * ThirdComponent) / 256.0;
        public double Cb => 
            Format == PixelFormat.YCbCr 
                ? SecondComponent 
                : 128.0 + (-37.945 * FirstComponent - 74.494 * SecondComponent + 112.439 * ThirdComponent) / 256.0;
        public double Cr => 
            Format == PixelFormat.YCbCr 
                ? ThirdComponent 
                : 128.0 + (112.439 * FirstComponent - 94.154 * SecondComponent - 18.285 * ThirdComponent) / 256.0;
    }
}