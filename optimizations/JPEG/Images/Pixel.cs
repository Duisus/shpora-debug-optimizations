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
            
            _firstComponent = firstComponent;
            _secondComponent = secondComponent;
            _thirdComponent = thirdComponent;
        }

        private readonly byte _firstComponent;
        private readonly byte _secondComponent;
        private readonly byte _thirdComponent;

        public double R =>
            Format == PixelFormat.RGB 
                ? _firstComponent
                : (298.082 * _firstComponent + 408.583 * _thirdComponent) / 256.0 - 222.921;
        public double G =>
            Format == PixelFormat.RGB 
                ? _secondComponent 
                : (298.082 * _firstComponent - 100.291 * _secondComponent - 208.120 * _thirdComponent) / 256.0 + 135.576;
        public double B => 
            Format == PixelFormat.RGB 
                ? _thirdComponent 
                : (298.082 * _firstComponent + 516.412 * _secondComponent) / 256.0 - 276.836;
        public double Y => 
            Format == PixelFormat.YCbCr 
                ? _firstComponent 
                : 16.0 + (65.738 * _firstComponent + 129.057 * _secondComponent + 24.064 * _thirdComponent) / 256.0;
        public double Cb => 
            Format == PixelFormat.YCbCr 
                ? _secondComponent 
                : 128.0 + (-37.945 * _firstComponent - 74.494 * _secondComponent + 112.439 * _thirdComponent) / 256.0;
        public double Cr => 
            Format == PixelFormat.YCbCr 
                ? _thirdComponent 
                : 128.0 + (112.439 * _firstComponent - 94.154 * _secondComponent - 18.285 * _thirdComponent) / 256.0;
    }
}