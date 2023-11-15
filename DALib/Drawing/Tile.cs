namespace DALib.Drawing;

public sealed class Tile
{
    public required byte[] Data { get; set; }
    public int Height { get; set; }
    public int Id { get; set; }
    public int Width { get; set; }
}