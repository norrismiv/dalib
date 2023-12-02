namespace DALib.Drawing;

public sealed class MpfFrame
{
    public short Bottom { get; set; }
    public required byte[] Data { get; set; }
    public short Left { get; set; }
    public short Right { get; set; }
    public int StartAddress { get; set; }
    public short Top { get; set; }
    public short XOffset { get; set; }
    public short YOffset { get; set; }
    public int Height => Bottom - Top;
    public int Width => Right - Left;
}