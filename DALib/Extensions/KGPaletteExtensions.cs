using System.Linq;
using DALib.Drawing;
using KGySoft.Drawing.SkiaSharp;
using KGPalette = KGySoft.Drawing.Imaging.Palette;

namespace DALib.Extensions;

public static class KGPaletteExtensions
{
    public static Palette ToDALibPalette(this KGPalette palette) => new(palette.GetEntries().Select(c => c.ToSKColor()));
}