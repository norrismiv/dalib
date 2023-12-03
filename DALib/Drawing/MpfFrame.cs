namespace DALib.Drawing;

public sealed class MpfFrame
{
    public short Bottom { get; set; }
    public required byte[] Data { get; set; }
    public short Left { get; set; }

    /// <summary>
    ///     This controls the X coordinate of the image in which the image will be flipped on when facing left or right
    /// </summary>
    /// <remarks>
    ///     This value is generally exactly(or very close to) ImageWidth / 2
    /// </remarks>
    public short ReflectionX { get; set; }

    /// <summary>
    ///     This controls the Y coordinate of the image in which it might be reflected, but in practicality it represents the
    ///     bottom of the image.
    /// </summary>
    /// <remarks>
    ///     This value is generally close to ImageHeight. Depending on how much space was left at the bottom of the frame vs
    ///     the total height of the image, you may need to subtract a small number of pixels (1-15)
    /// </remarks>
    public short ReflectionY { get; set; }

    public short Right { get; set; }
    public int StartAddress { get; set; }
    public short Top { get; set; }
    public int Height => Bottom - Top;
    public int Width => Right - Left;
}