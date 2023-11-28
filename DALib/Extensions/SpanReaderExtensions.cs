using DALib.Definitions;
using DALib.Memory;
using DALib.Utility;
using SkiaSharp;

namespace DALib.Extensions;

public static class SpanReaderExtensions
{
    public static SKColor ReadArgb1555Color(ref this SpanReader reader, bool scaleToArgb8888)
    {
        var color = reader.ReadUInt16();

        var a = (color & 0b1) == 0b1; //maybe?
        var r = (byte)((color >> 10) & CONSTANTS.FIVE_BIT_MASK);
        var g = (byte)((color >> 5) & CONSTANTS.FIVE_BIT_MASK);
        var b = (byte)(color & CONSTANTS.FIVE_BIT_MASK);

        if (scaleToArgb8888)
        {
            r = MathEx.ScaleRange<byte, byte>(
                r,
                0,
                CONSTANTS.FIVE_BIT_MASK,
                0,
                byte.MaxValue);

            g = MathEx.ScaleRange<byte, byte>(
                g,
                0,
                CONSTANTS.FIVE_BIT_MASK,
                0,
                byte.MaxValue);

            b = MathEx.ScaleRange<byte, byte>(
                b,
                0,
                CONSTANTS.FIVE_BIT_MASK,
                0,
                byte.MaxValue);
        }

        return new SKColor(
            r,
            g,
            b,
            a ? byte.MaxValue : byte.MinValue);
    }

    public static SKColor ReadRgb565Color(ref this SpanReader reader, bool scaleToRgb888)
    {
        var color = reader.ReadUInt16();

        var r = (byte)(color >> 11);
        var g = (byte)((color >> 5) & CONSTANTS.SIX_BIT_MASK);
        var b = (byte)(color & CONSTANTS.FIVE_BIT_MASK);

        if (scaleToRgb888)
        {
            r = MathEx.ScaleRange<byte, byte>(
                r,
                0,
                CONSTANTS.FIVE_BIT_MASK,
                0,
                byte.MaxValue);

            g = MathEx.ScaleRange<byte, byte>(
                g,
                0,
                CONSTANTS.SIX_BIT_MASK,
                0,
                byte.MaxValue);

            b = MathEx.ScaleRange<byte, byte>(
                b,
                0,
                CONSTANTS.FIVE_BIT_MASK,
                0,
                byte.MaxValue);
        }

        return new SKColor(r, g, b);
    }
}