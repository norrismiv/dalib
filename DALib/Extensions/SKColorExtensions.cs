using System;
using DALib.Definitions;
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
    /// <param name="color">
    ///     The color whose luminance is being calculated.
    /// </param>
    /// <param name="coefficient">
    ///     The coefficient to multiply the luminance by. Default value is 1.0f.
    /// </param>
    public static float GetLuminance(this SKColor color, float coefficient = 1.0f)
        => (0.299f * color.Red + 0.587f * color.Green + 0.114f * color.Blue) * coefficient;

    /// <summary>
    ///     Checks if a color is close to black
    /// </summary>
    /// <param name="color">
    ///     The color to check
    /// </param>
    public static bool IsNearBlack(this SKColor color)
        => color is
        {
            Alpha: 255,
            Red: <= CONSTANTS.RGB555_COLOR_LOSS_FACTOR,
            Green: <= CONSTANTS.RGB555_COLOR_LOSS_FACTOR,
            Blue: <= CONSTANTS.RGB555_COLOR_LOSS_FACTOR
        };

    /// <summary>
    ///     Returns a new SKColor with the alpha set based on the luminance of the color
    /// </summary>
    /// <param name="color">
    ///     An SKColor
    /// </param>
    /// <param name="coefficient">
    ///     The coefficient to multiply the luminance alpha by. Default value is 1.0f.
    /// </param>
    /// <returns>
    ///     A new SKColor with the alpha set.
    /// </returns>
    public static SKColor WithLuminanceAlpha(this SKColor color, float coefficient = 1.0f)
    {
        var luminance = color.GetLuminance(coefficient);

        return color.WithAlpha((byte)Math.Clamp(luminance, 0, byte.MaxValue));
    }
}