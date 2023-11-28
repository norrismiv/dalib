using SkiaSharp;

namespace DALib.Drawing;

public sealed class ColorTableEntry
{
    public byte ColorIndex { get; init; }
    public required SKColor[] Colors { get; init; }
}