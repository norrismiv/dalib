using SkiaSharp;

namespace DALib.Drawing;

public sealed class ColorTableEntry
{
    public byte ColorIndex { get; set; }
    public required SKColor[] Colors { get; set; }
}