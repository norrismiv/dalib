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
}