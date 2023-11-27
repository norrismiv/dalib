using System.Linq;
using DALib.Drawing;
using KGySoft.Drawing.SkiaSharp;
using KGPalette = KGySoft.Drawing.Imaging.Palette;

namespace DALib.Extensions;

public static class KGPaletteExtensions
{
    public static Palette ToDALibPalette(this KGPalette palette, bool isDyableType = false)
    {
        var orderedColors = palette.GetEntries()
                                   .Select(c => c.ToSKColor())
                                   .OrderBy(c => c.GetLuminance());

        return new Palette(orderedColors);
    }
}