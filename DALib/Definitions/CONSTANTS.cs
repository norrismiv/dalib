using SkiaSharp;

namespace DALib.Definitions;

/// <summary>
///     This class contains constant values used in the project.
/// </summary>
public static class CONSTANTS
{
    /// <summary>
    ///     The length of a data archive entry name
    /// </summary>
    public const int DATA_ARCHIVE_ENTRY_NAME_LENGTH = 13;

    /// <summary>
    ///     The starting index for the dye colors in a palette
    /// </summary>
    public const int PALETTE_DYE_INDEX_START = 98;

    /// <summary>
    ///     The maximum number of colors that can be stored in a palette
    /// </summary>
    public const int COLORS_PER_PALETTE = 256;

    /// <summary>
    ///     The width of a tile in pixels
    /// </summary>
    public const int TILE_WIDTH = 56;

    /// <summary>
    ///     The height of a tile in pixels
    /// </summary>
    public const int TILE_HEIGHT = 27;

    /// <summary>
    ///     The width of a foreground tile in pixels
    /// </summary>
    public const int HPF_TILE_WIDTH = 28;

    /// <summary>
    ///     Half of the width of a tile in pixels
    /// </summary>
    public const int HALF_TILE_WIDTH = 28;

    /// <summary>
    ///     Half of the height of a tile in pixels (rounded up)
    /// </summary>
    public const int HALF_TILE_HEIGHT = 14;

    /// <summary>
    ///     The area of a tile in pixels
    /// </summary>
    public const int TILE_SIZE = TILE_WIDTH * TILE_HEIGHT;

    /// <summary>
    ///     A byte with 5 of it's 8 bits set to 1
    /// </summary>
    public const byte FIVE_BIT_MASK = 0b11111;

    /// <summary>
    ///     A byte with 6 of it's 8 bits set to 1
    /// </summary>
    public const byte SIX_BIT_MASK = 0b111111;

    /// <summary>
    ///     Transparent black, commonly used to represent transparency in DarkAges image formats
    /// </summary>
    public static readonly SKColor Transparent = SKColors.Black.WithAlpha(0);
}