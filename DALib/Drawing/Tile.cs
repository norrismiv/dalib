using DALib.Definitions;

namespace DALib.Drawing;

public sealed class Tile
{
    public required byte[] Data { get; set; }
    public int Height => CONSTANTS.TILE_HEIGHT;
    public int Width => CONSTANTS.TILE_WIDTH;
}