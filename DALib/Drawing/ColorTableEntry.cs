using SkiaSharp;

namespace DALib.Drawing;

public sealed class ColorTableEntry
{
    public byte ColorIndex { get; }
    public SKColor[] Colors { get; }

    public ColorTableEntry(byte colorIndex, SKColor[] colors)
    {
        ColorIndex = colorIndex;
        Colors = colors;
    }
}