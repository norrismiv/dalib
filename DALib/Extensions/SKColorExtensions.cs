using System;
using SkiaSharp;

namespace DALib.Extensions;

public static class SKColorExtensions
{
    public static float GetLuminance(this SKColor color, float coefficient = 1.0f)
        => (0.299f * color.Red + 0.587f * color.Green + 0.114f * color.Blue) * coefficient;

    public static SKColor WithLuminanceAlpha(this SKColor color, float coefficient = 1.0f)
    {
        var luminance = color.GetLuminance(coefficient);

        return color.WithAlpha((byte)Math.Clamp(luminance, 0, byte.MaxValue));
    }
}