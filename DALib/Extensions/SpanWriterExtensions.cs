using DALib.Memory;
using SkiaSharp;

namespace DALib.Extensions;

public static class SpanWriterExtensions
{
    public static void WriteRgb555Color(ref this SpanWriter writer, SKColor color)
    {
        var r = color.Red >> 3;
        var g = color.Green >> 3;
        var b = color.Blue >> 3;

        var rgb555 = (ushort)((r << 10) | (g << 5) | b);

        writer.WriteUInt16(rgb555);
    }

    public static void WriteRgb565Color(ref this SpanWriter writer, SKColor color)
    {
        var r = color.Red >> 3;
        var g = color.Green >> 2;
        var b = color.Blue >> 3;

        var rgb565 = (ushort)((r << 11) | (g << 5) | b);

        writer.WriteUInt16(rgb565);
    }
}