using DALib.Memory;
using SkiaSharp;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for SpanWriters
/// </summary>
public static class SpanWriterExtensions
{
    /// <summary>
    ///     Writes the given SKColor as a 16-bit RGB555 encoded color to the SpanWriter.
    /// </summary>
    /// <param name="writer">
    ///     The SpanWriter to write the color to.
    /// </param>
    /// <param name="color">
    ///     The RGB888 color to encode and write
    /// </param>
    public static void WriteRgb555Color(ref this SpanWriter writer, SKColor color)
    {
        var r = color.Red >> 3;
        var g = color.Green >> 3;
        var b = color.Blue >> 3;

        var rgb555 = (ushort)((r << 10) | (g << 5) | b);

        writer.WriteUInt16(rgb555);
    }

    /// <summary>
    ///     Writes the given SKColor as a 16-bit RGB565 encoded color to the SpanWriter.
    /// </summary>
    /// <param name="writer">
    ///     The SpanWriter to write the color to.
    /// </param>
    /// <param name="color">
    ///     The RGB888 color to encode and write
    /// </param>
    public static void WriteRgb565Color(ref this SpanWriter writer, SKColor color)
    {
        var r = color.Red >> 3;
        var g = color.Green >> 2;
        var b = color.Blue >> 3;

        var rgb565 = (ushort)((r << 11) | (g << 5) | b);

        writer.WriteUInt16(rgb565);
    }
}