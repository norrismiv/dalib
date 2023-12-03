namespace DALib.Drawing;

public sealed class MpfFrame
{
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

    public required byte[] Data { get; set; }
    public short Left { get; set; }

    public short Right { get; set; }
    public int StartAddress { get; set; }
    public short Top { get; set; }
    public int Height => Bottom - Top;
    public int Width => Right - Left;
}