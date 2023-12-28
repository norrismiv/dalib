using System;
using SkiaSharp;

namespace DALib.Extensions;

public static class SKColorExtensions
{
    public static float GetLuminance(this SKColor color, float coefficient = 1.0f)
        => (0.299f * color.Red + 0.587f * color.Green + 0.114f * color.Blue) * coefficient;

    public static ushort ToRgb555Number(this SKColor color)
    {
        var r = color.Red >> 3;
        var g = color.Green >> 3;
        var b = color.Blue >> 3;

        return (ushort)((r << 10) | (g << 5) | b);
    }

    public static ushort ToRgb565Number(this SKColor color)
    {
        var r = color.Red >> 3;
        var g = color.Green >> 2;
        var b = color.Blue >> 3;

        return (ushort)((r << 11) | (g << 5) | b);
    }

    public static SKColor WithLuminanceAlpha(this SKColor color, float coefficient = 1.0f)
    {
        var luminance = color.GetLuminance(coefficient);

        return color.WithAlpha((byte)Math.Clamp(luminance, 0, byte.MaxValue));
    }
}