using System;
using System.Linq;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class ColorTableEntry
{
    public byte ColorIndex { get; set; }
    public required SKColor[] Colors { get; set; }

    public static ColorTableEntry Empty
        => new()
        {
            ColorIndex = 0,
            Colors = Enumerable.Repeat(SKColors.Transparent, 6)
                               .ToArray()
        };
}