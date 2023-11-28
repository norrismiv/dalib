using DALib.Definitions;
using KGySoft.Drawing.Imaging;
using SkiaSharp;

namespace DALib.Utility;

public sealed class QuantizerOptions
{
    public SKColorType ColorType { get; set; } = SKColorType.Rgba8888;

    public IDitherer? Ditherer { get; set; }
    public int MaxColors { get; set; } = CONSTANTS.COLORS_PER_PALETTE;
    public static QuantizerOptions Default { get; } = new();
}