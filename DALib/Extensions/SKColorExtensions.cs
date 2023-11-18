using System;
using SkiaSharp;

namespace DALib.Extensions;

public static class SKColorExtensions
{
    public static SKColor WithLuminanceAlpha(this SKColor color, float coefficient = 1.0f)
    {
        var luminance = 0.299f * color.Red + 0.587f * color.Green + 0.114f * color.Blue;

        return color.WithAlpha((byte)Math.Clamp(coefficient * luminance, 0, byte.MaxValue));
    }
}