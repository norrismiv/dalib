namespace DALib.Drawing;

/// <summary>
///     Represents a frame in an EpfFile
/// </summary>
public sealed class EpfFrame
{
    /// <summary>
    ///     The highest Y coordinate of the frame. Bottom - Top = Height
    /// </summary>
    public short Bottom { get; set; }

    /// <summary>
    ///     The pixel data of the frame encoded as palette indexes
    /// </summary>
    public required byte[] Data { get; set; }

    /// <summary>
    ///     The lowest X coordinate of the frame. Right - Left = Width
    /// </summary>
    public short Left { get; set; }

    /// <summary>
    ///     The highest X coordinate of the frame. Right - Left = Width
    /// </summary>
    public short Right { get; set; }

    /// <summary>
    ///     The lowest Y coordinate of the frame. Bottom - Top = Height
    /// </summary>
    public short Top { get; set; }
}