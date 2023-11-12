namespace DALib.Drawing;

public sealed class EpfFrame
{
    public int Bottom { get; init; }
    public required byte[] Data { get; init; }
    public int Left { get; init; }
    public int Right { get; init; }
    public int Top { get; init; }
    public int Height => Bottom - Top;
    public int Width => Right - Left;
}