using System;
using System.Linq;
using DALib.Drawing;
using SkiaSharp;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for SKImages
/// </summary>
public static class SKImageExtensions
{
    /// <summary>
    ///     Retrieves the pixel data from an SKImage encoded as palette indexes pointing to the provided palette
    /// </summary>
    /// <param name="image">
    ///     The SKImage to retrieve the pixel data from.
    /// </param>
    /// <param name="palette">
    ///     A palette containing all of the colors in the image
    /// </param>
    public static byte[] GetPalettizedPixelData(this SKImage image, Palette palette)
    {
        var colorMap = palette.Select((c, i) => (c, i))
                              .DistinctBy(set => set.c)
                              .ToDictionary(set => set.c, c => (byte)c.i);
        var pixelData = new byte[image.Width * image.Height];
        using var pixels = image.PeekPixels();

        for (var y = 0; y < image.Height; ++y)
        {
            for (var x = 0; x < image.Width; ++x)
            {
                var pixelIndex = y * image.Width + x;

                var trueColor = pixels.GetPixelColor(x, y);
                var color = trueColor.WithAlpha(byte.MaxValue);

                if (!colorMap.TryGetValue(color, out var colorIndex))
                    throw new InvalidOperationException("Color not found in palette.");

                pixelData[pixelIndex] = colorIndex;
            }
        }

        return pixelData;
    }
}