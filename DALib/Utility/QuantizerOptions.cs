using DALib.Definitions;
using KGySoft.Drawing.Imaging;
using SkiaSharp;

namespace DALib.Utility;

public sealed class QuantizerOptions
{
    /// <summary>
    ///     Do not change this value unless you know what you're doing
    /// </summary>
    public SKColorType ColorType { get; set; } = SKColorType.Rgba8888;

    /// <summary>
    ///     The default ditherer is null, but here are some possible options:
    ///     <br />
    ///     "ErrorDiffusionDitherer.FloydSteinberg" <br />
    ///     "new InterleavedGradientNoiseDitherer(AutoStrengthMode.Default)" <br />
    ///     "OrderedDitherer.Bayer2x2", 3x3, or 4x4, <br />
    ///     "ErrorDiffusionDitherer.Atkinson" <br />
    /// </summary>
    /// <remarks>
    ///     The larger the matrix, the larger the spread of the error diffusion. This means the patterns used to dither the
    ///     images are larger, so the size of the matrix you want is largely dependent on the size and complexity of the image.
    ///     Due to the small size of DarkAges graphics, you generally want a small matrix (2x2-4x4). You can try out some of
    ///     the 5x* ditherers if you're working with an image that might cover most of the screen, like a mundane illustration
    /// </remarks>
    public IDitherer? Ditherer { get; set; }

    /// <summary>
    ///     If the image you are quantizing is a dyeable type (like an item), but you do not want that image to actually be
    ///     dyeable, make sure you set this value to 250 instead of 256, so that there is room to fill the 6 dyeable color
    ///     indexes with empty colors
    /// </summary>
    /// <remarks>
    ///     For dyeable images, you may want to quantize the image with 250 colors first, then edit the image with the default
    ///     dyeable colors (the 6 purple colors), then re-run that image through the process with the MaxColors set to 256. The
    ///     quantizer will not run again because the image already has less than the max colors.
    /// </remarks>
    public int MaxColors { get; set; } = CONSTANTS.COLORS_PER_PALETTE;

    public static QuantizerOptions Default { get; } = new();

    /// <summary>
    ///     The default ditherer is Floyd-Steinberg, but you can try out some of the others. Here are some other options:
    ///     <br />
    ///     "new InterleavedGradientNoiseDitherer(AutoStrengthMode.Default)" <br />
    ///     "OrderedDitherer.Bayer2x2", 3x3, or 4x4, <br />
    ///     "ErrorDiffusionDitherer.Atkinson" <br />
    /// </summary>
    /// <remarks>
    ///     The larger the matrix, the larger the spread of the error diffusion. This means the patterns used to dither the
    ///     images are larger, so the size of the matrix you want is largely dependent on the size and complexity of the image.
    ///     Due to the small size of DarkAges graphics, you generally want a small matrix (2x2-4x4). You can try out some of
    ///     the 5x* ditherers if you're working with an image that might cover most of the screen, like a mundane illustration
    /// </remarks>
    public static QuantizerOptions DefaultWithDithering { get; } = new()
    {
        Ditherer = ErrorDiffusionDitherer.FloydSteinberg
    };
}