namespace DALib.Drawing;

public sealed class EpfFrame
{
    public short Bottom { get; init; }
    public required byte[] Data { get; init; }
    public short Left { get; init; }
    public short Right { get; init; }
    public short Top { get; init; }
}