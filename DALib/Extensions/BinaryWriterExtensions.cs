using System.IO;
using DALib.Utility;
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
    /// <param name="writer">
    ///     The BinaryWriter to write to.
    /// </param>
    /// <param name="color">
    ///     An SKColor (RGB888)
    /// </param>
    public static void WriteRgb555Color(this BinaryWriter writer, SKColor color)
    {
        var encodedColor = ColorCodec.EncodeRgb555(color);

        writer.Write(encodedColor);
    }

    /// <summary>
    ///     Writes an RGB888 color encoded as RGB565 to the BinaryWriter.
    /// </summary>
    /// <param name="writer">
    ///     The BinaryWriter to write to.
    /// </param>
    /// <param name="color">
    ///     An SKColor (RGB888)
    /// </param>
    public static void WriteRgb565Color(this BinaryWriter writer, SKColor color)
    {
        var encodedColor = ColorCodec.EncodeRgb565(color);

        writer.Write(encodedColor);
    }
}