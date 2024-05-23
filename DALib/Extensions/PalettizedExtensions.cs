using System;
using System.Collections.Frozen;
using System.Linq;
using DALib.Definitions;
using DALib.Drawing;
using DALib.Utility;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for Palettized objects
/// </summary>
public static class PalettizedExtensions
{
    /// <summary>
    ///     For EPF images that are for a game type that supports dye colors, this method will arrange the colors in the
    ///     palette so that dye colors line up with the starting dye index
    /// </summary>
    /// <param name="palettized">
    ///     A palettized epf file
    /// </param>
    /// <param name="defaultDyeColors">
    ///     The dye colors present in the image. Set this to null if the type is dyeable, but the specific image is not
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// </exception>
    /// <remarks>
    ///     If the type is dyeable, but the specific image is not intended to be dyeable, make sure you specify only 250 colors
    ///     during quantization, so that there is room for the 6 dyeable color slots to be empty
    /// </remarks>
    public static Palettized<EpfFile> ArrangeColorsForDyeableType(
        this Palettized<EpfFile> palettized,
        ColorTableEntry? defaultDyeColors = null)
    {
        (var epf, var palette) = palettized;

        defaultDyeColors ??= ColorTableEntry.Empty;

        var paletteWithoutDyeColors = palette.Distinct()
                                             .ToList();

        paletteWithoutDyeColors = paletteWithoutDyeColors.Except(defaultDyeColors.Colors)
                                                         .ToList();

        if ((paletteWithoutDyeColors.Count + 6) > CONSTANTS.COLORS_PER_PALETTE)
            throw new InvalidOperationException("Palette does not have enough space for dye colors.");

        //take colors up to the dye index start
        var newPalette = new Palette(paletteWithoutDyeColors.Take(CONSTANTS.PALETTE_DYE_INDEX_START));

        //copy dye colors into dyeable indexes
        for (var i = 0; i < defaultDyeColors.Colors.Length; i++)
            newPalette[CONSTANTS.PALETTE_DYE_INDEX_START + i] = defaultDyeColors.Colors[i];

        //take remaining colors and insert after dye index end
        var index = CONSTANTS.PALETTE_DYE_INDEX_START + 6;

        foreach (var color in paletteWithoutDyeColors.Skip(CONSTANTS.PALETTE_DYE_INDEX_START))
            newPalette[index++] = color;

        //for each frame, render the image and remap the colors to the new palette
        foreach (var frame in epf)
        {
            //render the image with the old palette
            using var image = Graphics.RenderImage(frame, palette);

            //remap the image to the new palette
            var newFrameData = image.GetPalettizedPixelData(newPalette);

            //set the remapped frame data for the frames
            frame.Data = newFrameData;
        }

        //return the epffile with the new palette
        return new Palettized<EpfFile>
        {
            Entity = epf,
            Palette = newPalette
        };
    }

    /// <summary>
    ///     Remaps the frame data of the given palettized epf file to use the indexes of the new palette, assuming the colors
    ///     are the same
    /// </summary>
    public static Palettized<EpfFile> RemapPalette(this Palettized<EpfFile> palettized, Palette newPalette)
    {
        (var epf, var palette) = palettized;

        //create a dictionary that maps the old color indexes to the new color indexes
        var colorIndexMap = palette.Select((c, i) => (c, i))
                                   .DistinctBy(set => set.c)
                                   .ToFrozenDictionary(set => (byte)set.i, set => (byte)newPalette.IndexOf(set.c));

        foreach (var frame in epf)
            for (var i = 0; i < frame.Data.Length; i++)
                frame.Data[i] = colorIndexMap[frame.Data[i]];

        //return the epffile with the new palette
        return new Palettized<EpfFile>
        {
            Entity = epf,
            Palette = newPalette
        };
    }
}