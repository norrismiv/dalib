namespace DALib.Drawing;

/// <summary>
///     Represents a frame in an MpfFile
/// </summary>
public sealed class MpfFrame
{
    /// <summary>
    ///     The highest Y coordinate of the frame. Bottom - Top = Height
    /// </summary>
    public short Bottom { get; set; }

    /// <summary>
    ///     The X coordinate of the image that lines up with the center of the draw area. The image will be reflected on this X
    ///     coordinate when facing left or right
    /// </summary>
    /// <remarks>
    ///     This value is generally exactly(or very close to) ImageWidth / 2
    /// </remarks>
    public short CenterX { get; set; }

    /// <summary>
    ///     The Y coordinate of the image that lines up with the center of the draw area. Generally, the bottom of the image
    ///     sits on the center of the draw area
    /// </summary>
    /// <remarks>
    ///     This value is generally close to ImageHeight. Depending on how much space was left at the bottom of the frame vs
    ///     the total height of the image, you may need to subtract a small number of pixels (1-15)
    /// </remarks>
    public short CenterY { get; set; }

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
    ///     The address within the data segment of the file where the image data for this frame can be found
    /// </summary>
    public int StartAddress { get; set; }

    /// <summary>
    ///     The lowest Y coordinate of the frame. Bottom - Top = Height
    /// </summary>
    public short Top { get; set; }

    /// <summary>
    ///     The pixel height of the frame
    /// </summary>
    public int PixelHeight => Bottom - Top;

    /// <summary>
    ///     The pixel width of the frame
    /// </summary>
    public int PixelWidth => Right - Left;
}