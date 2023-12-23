using System;
using System.Linq;
using DALib.Drawing;
using SkiaSharp;

namespace DALib.Extensions;

public static class SKImageExtensions
{
    public static byte[] GetPalettizedPixelData(this SKImage image, Palette palette)
    {
        var colorMap = palette.Select((c, i) => (c, i))
                              .DistinctBy(set => set.c)
                              .ToDictionary(set => set.c, c => (byte)c.i);
        var pixelData = new byte[image.Width * image.Height];

        for (var y = 0; y < image.Height; ++y)
        {
            for (var x = 0; x < image.Width; ++x)
            {
                var pixelIndex = y * image.Width + x;
                using var pixels = image.PeekPixels();

                var color = pixels.GetPixelColor(x, y)
                                  .WithAlpha(byte.MaxValue);

                if (!colorMap.TryGetValue(color, out var colorIndex))
                    throw new InvalidOperationException("Color not found in palette.");

                pixelData[pixelIndex] = colorIndex;
            }
        }

        return pixelData;
    }
}