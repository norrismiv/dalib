namespace DALib.Drawing;

public class EfaFrame
{
    public int ByteCount { get; init; }
    public int ByteWidth { get; init; }
    public required byte[] Data { get; init; }
    public int Offset { get; init; }
    public int OriginFlags { get; init; }
    public int OriginX { get; init; }
    public int OriginY { get; init; }
    public int Pad1Flags { get; init; }
    public short Pad1X { get; init; }
    public short Pad1Y { get; init; }
    public int Pad2Flags { get; init; }
    public short Pad2X { get; init; }
    public short Pad2Y { get; init; }
    public int RawSize { get; init; }
    public int Size { get; init; }
    public int Unknown1 { get; init; }
    public int Unknown2 { get; init; }
    public int Unknown3 { get; init; }
    public int Unknown4 { get; init; }
    public int Unknown5 { get; init; }
}