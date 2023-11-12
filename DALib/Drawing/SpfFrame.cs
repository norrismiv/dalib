namespace DALib.Drawing;

public sealed class SpfFrame
{
    public uint ByteCount { get; init; }
    public uint ByteWidth { get; init; }
    public required byte[] Data { get; init; }
    public ushort PadHeight { get; init; }
    public ushort PadWidth { get; init; }
    public ushort PixelHeight { get; init; }
    public ushort PixelWidth { get; init; }
    public uint Reserved { get; init; }
    public uint SemiByteCount { get; init; }
    public uint StartAddress { get; set; }
    public static uint Unknown => 0xCCCCCCCC; // Every SPF has this value associated with it
}