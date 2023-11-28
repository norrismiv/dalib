using System;
using System.Linq;
using DALib.Definitions;
using DALib.Drawing;
using DALib.Utility;

namespace DALib.Extensions;

public static class PalettizedExtensions
{
    /// <summary>
    ///     For EPF images that are for a game type that supports dye colors, this method will arrange the colors in the
    ///     palette so that they line up with the starting dye index
    /// </summary>
    /// <param name="palettized">A palettized epf file</param>
    /// <param name="dyeColors">
    ///     The dye colors present in the image. Set this to null if the type is dyable, but the specific
    ///     image is not
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
    public static Palettized<EpfFile> ArrangeColorsForDyableType(this Palettized<EpfFile> palettized, ColorTableEntry? dyeColors = null)
    {
        (var epf, var palette) = palettized;

        var paletteWithoutDyeColors = palette.Distinct()
                                             .ToList();

        if (dyeColors is not null)
            paletteWithoutDyeColors = paletteWithoutDyeColors.Except(dyeColors.Colors)
                                                             .ToList();

        if ((paletteWithoutDyeColors.Count + 6) > CONSTANTS.COLORS_PER_PALETTE)
            throw new InvalidOperationException("Palette does not have enough space for dye colors.");

        //take colors up to the dye index start
        var newPalette = new Palette(paletteWithoutDyeColors.Take(CONSTANTS.PALETTE_DYE_INDEX_START));

        //copy dye colors into the palette if necessary
        if (dyeColors is null)
            for (var i = 0; i < dyeColors!.Colors.Length; i++)
                newPalette[CONSTANTS.PALETTE_DYE_INDEX_START + i] = dyeColors.Colors[i];

        //take remaining colors and insert after dye index end
        var index = 6;

        foreach (var color in paletteWithoutDyeColors.Skip(CONSTANTS.PALETTE_DYE_INDEX_START))
            newPalette[CONSTANTS.PALETTE_DYE_INDEX_START + index++] = color;

        //for each frame, render the image and remap the colors to the new palette
        foreach (var frame in epf)
        {
            using var image = Graphics.RenderImage(frame, palette);
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
}