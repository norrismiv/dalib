using DALib.Definitions;

namespace DALib.Drawing;

/// <summary>
///     Represents a ground tile
/// </summary>
public sealed class Tile
{
    /// <summary>
    ///     The pixel data of the tile encoded as palette indexes
    /// </summary>
    public required byte[] Data { get; set; }

    /// <summary>
    ///     The pixel height of the tile
    /// </summary>
    public int PixelHeight => CONSTANTS.TILE_HEIGHT;

    /// <summary>
    ///     The pixel height of the tile
    /// </summary>
    public int PixelWidth => CONSTANTS.TILE_WIDTH;
}