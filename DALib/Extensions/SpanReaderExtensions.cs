using DALib.Memory;
using SkiaSharp;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for SpanReaders
/// </summary>
public static class SpanReaderExtensions
{
    /// <summary>
    ///     Reads a 16-bit RGB555 color from the SpanReader and scales it to RGB888
    /// </summary>
    /// <param name="reader">The SpanReader instance.</param>
    public static SKColor ReadRgb555Color(ref this SpanReader reader)
        => reader.ReadUInt16()
                 .ToRgb555Color();

    /// <summary>
    ///     Reads a 16-bit RGB565 color from the SpanReader and scales it to RGB888
    /// </summary>
    /// <param name="reader">The SpanReader instance.</param>
    public static SKColor ReadRgb565Color(ref this SpanReader reader)
        => reader.ReadUInt16()
                 .ToRgb565Color();
}