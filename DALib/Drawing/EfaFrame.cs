namespace DALib.Drawing;

public class EfaFrame
{
    public int ByteCount { get; init; }
    public int ByteWidth { get; init; }
    public required byte[] Data { get; init; }
    public short FrameHeight { get; init; }
    public short FrameWidth { get; init; }
    public short ImageHeight { get; init; }
    public short ImageWidth { get; init; }
    public int Offset { get; set; }
    public int OriginFlags { get; init; }
    public short OriginX { get; init; }
    public short OriginY { get; init; }
    public int Pad1Flags { get; init; }
    public int Pad2Flags { get; init; }
    public int RawSize { get; init; }
    public int Size { get; set; }
    public int Unknown1 { get; init; }
    public int Unknown2 { get; init; }
    public int Unknown3 { get; init; }
    public int Unknown4 { get; init; }
    public int Unknown5 { get; init; }
}