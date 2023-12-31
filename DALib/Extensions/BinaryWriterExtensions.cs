using System.IO;
using SkiaSharp;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for BinaryWriter
/// </summary>
public static class BinaryWriterExtensions
{
    /// <summary>
    ///     Writes an RGB888 color encoded as RGB555 to the BinaryWriter.
    /// </summary>
    /// <param name="writer">The BinaryWriter to write to.</param>
    /// <param name="color">An SKColor (RGB888)</param>
    public static void WriteRgb555Color(this BinaryWriter writer, SKColor color)
    {
        var r = color.Red >> 3;
        var g = color.Green >> 3;
        var b = color.Blue >> 3;

        var rgb555 = (ushort)((r << 10) | (g << 5) | b);

        writer.Write(rgb555);
    }

    /// <summary>
    ///     Writes an RGB888 color encoded as RGB565 to the BinaryWriter.
    /// </summary>
    /// <param name="writer">The BinaryWriter to write to.</param>
    /// <param name="color">An SKColor (RGB888)</param>
    public static void WriteRgb565Color(this BinaryWriter writer, SKColor color)
    {
        var r = color.Red >> 3;
        var g = color.Green >> 2;
        var b = color.Blue >> 3;

        var rgb565 = (ushort)((r << 11) | (g << 5) | b);

        writer.Write(rgb565);
    }
}