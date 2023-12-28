using DALib.Memory;
using SkiaSharp;

namespace DALib.Extensions;

public static class SpanReaderExtensions
{
    public static SKColor ReadRgb555Color(ref this SpanReader reader)
        => reader.ReadUInt16()
                 .ToRgb555Color();

    public static SKColor ReadRgb565Color(ref this SpanReader reader)
        => reader.ReadUInt16()
                 .ToRgb565Color();
}