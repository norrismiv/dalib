using DALib.Definitions;
using DALib.Utility;
using SkiaSharp;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for numbers.
/// </summary>
public static class NumberExtensions
{
    /// <summary>
    ///     Converts a 16-bit number to an RGB555 color scaled to RGB888.
    /// </summary>
    /// <param name="number">The 16-bit number representing the RGB555 color.</param>
    public static SKColor ToRgb555Color(this ushort number)
    {
        var r = (byte)((number >> 10) & CONSTANTS.FIVE_BIT_MASK);
        var g = (byte)((number >> 5) & CONSTANTS.FIVE_BIT_MASK);
        var b = (byte)(number & CONSTANTS.FIVE_BIT_MASK);

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

        return new SKColor(r, g, b);
    }

    /// <summary>
    ///     Converts a 16-bit number to an RGB565 color scaled to RGB888.
    /// </summary>
    /// <param name="number">The 16-bit number representing the RGB565 color.</param>
    public static SKColor ToRgb565Color(this ushort number)
    {
        var r = (byte)(number >> 11);
        var g = (byte)((number >> 5) & CONSTANTS.SIX_BIT_MASK);
        var b = (byte)(number & CONSTANTS.FIVE_BIT_MASK);

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

        return new SKColor(r, g, b);
    }
}