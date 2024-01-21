using System.Linq;
using DALib.Drawing;
using KGySoft.Drawing.SkiaSharp;
using KGPalette = KGySoft.Drawing.Imaging.Palette;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for KGPalettes
/// </summary>
public static class KGPaletteExtensions
{
    /// <summary>
    ///     Converts a KGPalette to a DALib.Palette
    /// </summary>
    /// <param name="palette">
    ///     The KGPalette to convert.
    /// </param>
    public static Palette ToDALibPalette(this KGPalette palette)
    {
        var orderedColors = palette.GetEntries()
                                   .Select(c => c.ToSKColor())
                                   .OrderBy(c => c.GetLuminance());

        return new Palette(orderedColors);
    }
}