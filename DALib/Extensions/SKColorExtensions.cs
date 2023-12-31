using System;
using SkiaSharp;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="SKColor" /> class.
/// </summary>
public static class SKColorExtensions
{
    /// <summary>
    ///     Calculates the luminance of a color using the provided coefficient.
    /// </summary>
    /// <param name="color">The color whose luminance is being calculated.</param>
    /// <param name="coefficient">The coefficient to multiply the luminance by. Default value is 1.0f.</param>
    public static float GetLuminance(this SKColor color, float coefficient = 1.0f)
        => (0.299f * color.Red + 0.587f * color.Green + 0.114f * color.Blue) * coefficient;

    /// <summary>
    ///     Converts the given SKColor to a RGB555 number.
    /// </summary>
    /// <param name="color">The SKColor to convert.</param>
    public static ushort ToRgb555Number(this SKColor color)
    {
        var r = color.Red >> 3;
        var g = color.Green >> 3;
        var b = color.Blue >> 3;

        return (ushort)((r << 10) | (g << 5) | b);
    }

    /// <summary>
    ///     Converts the given SKColor to a RGB565 number.
    /// </summary>
    /// <param name="color">The SKColor to convert.</param>
    public static ushort ToRgb565Number(this SKColor color)
    {
        var r = color.Red >> 3;
        var g = color.Green >> 2;
        var b = color.Blue >> 3;

        return (ushort)((r << 11) | (g << 5) | b);
    }

    /// <summary>
    ///     Returns a new SKColor with the alpha set based on the luminance of the color
    /// </summary>
    /// <param name="color">An SKColor</param>
    /// <param name="coefficient">The coefficient to multiply the luminance alpha by. Default value is 1.0f.</param>
    /// <returns>A new SKColor with the alpha set.</returns>
    public static SKColor WithLuminanceAlpha(this SKColor color, float coefficient = 1.0f)
    {
        var luminance = color.GetLuminance(coefficient);

        return color.WithAlpha((byte)Math.Clamp(luminance, 0, byte.MaxValue));
    }
}