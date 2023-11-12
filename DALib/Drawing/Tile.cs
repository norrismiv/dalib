namespace DALib.Drawing;

public sealed class Tile
{
    public int PaletteId { get; set; }
    public required byte[] Data { get; set; }
    public int Height { get; set; }
    public int Id { get; set; }
    public int Width { get; set; }
}