namespace DALib.Drawing;

public sealed class EfaFrame
{
    public int ByteCount { get; set; }
    public int ByteWidth { get; set; }
    public required byte[] Data { get; set; }
    public short FrameHeight { get; set; }
    public short FrameWidth { get; set; }
    public short ImageHeight { get; set; }
    public short ImageWidth { get; set; }
    public int Offset { get; set; }
    public int OriginFlags { get; set; }
    public short OriginX { get; set; }
    public short OriginY { get; set; }
    public int Pad1Flags { get; set; }
    public int Pad2Flags { get; set; }
    public int RawSize { get; set; }
    public int Size { get; set; }
    public int Unknown1 { get; set; }
    public int Unknown2 { get; set; }
    public int Unknown3 { get; set; }
    public int Unknown4 { get; set; }
    public int Unknown5 { get; set; }
}