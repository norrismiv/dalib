using SkiaSharp;

namespace DALib.Drawing;

public sealed class SpfFrame
{
    public uint ByteCount { get; set; }
    public uint ByteWidth { get; set; }
    public SKColor[]? ColorData { get; set; }
    public byte[]? Data { get; set; }
    public ushort PadHeight { get; set; }
    public ushort PadWidth { get; set; }
    public ushort PixelHeight { get; set; }
    public ushort PixelWidth { get; set; }
    public uint Reserved { get; set; }
    public uint SemiByteCount { get; set; }
    public uint StartAddress { get; set; }
    public static uint Unknown => 0xCCCCCCCC; // Every SPF has this value associated with it
}