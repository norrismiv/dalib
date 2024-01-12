using System.Linq;
using SkiaSharp;

namespace DALib.Drawing;

/// <summary>
///     Represents an entry in a color table.
/// </summary>
public sealed class ColorTableEntry
{
    /// <summary>
    ///     The color index as used by the client. (e.g. default purple = 0)
    /// </summary>
    public byte ColorIndex { get; set; }

    /// <summary>
    ///     The colors associated with the index
    /// </summary>
    public required SKColor[] Colors { get; set; }

    /// <summary>
    ///     Gets an "empty" instance of a ColorTableEntry that contains 6 white-transparent colors
    /// </summary>
    public static ColorTableEntry Empty
        => new()
        {
            ColorIndex = 0,
            Colors = Enumerable.Repeat(SKColors.Transparent, 6)
                               .ToArray()
        };
}