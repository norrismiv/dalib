namespace DALib.Drawing;

public sealed class EpfFrame
{
    public short Bottom { get; set; }
    public required byte[] Data { get; set; }
    public short Left { get; set; }
    public short Right { get; set; }
    public short Top { get; set; }
}