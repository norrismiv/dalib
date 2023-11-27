using System.Collections.Generic;
using System.Linq;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;

namespace DALib.Drawing;

public sealed class PaletteLookup
{
    public required Dictionary<int, Palette> Palettes { get; init; }
    public required PaletteTable Table { get; init; }

    public int GetNextPaletteId() => Palettes.Keys.Max() + 1;

    public Palette GetPaletteForId(int id)
    {
        var paletteNumber = Table.GetPaletteNumber(id);
        var useLuminanceBlending = false;

        if (paletteNumber >= 1000)
        {
            paletteNumber -= 1000;
            useLuminanceBlending = true;
        }

        var palette = Palettes[paletteNumber];

        if (useLuminanceBlending)
        {
            var blendedPalette = new Palette();

            for (var i = 0; i < CONSTANTS.COLORS_PER_PALETTE; i++)
            {
                var color = palette[i];

                blendedPalette[i] = color.WithLuminanceAlpha();
            }

            return blendedPalette;
        }

        return Palettes[paletteNumber];
    }

    #region LoadFrom
    public static PaletteLookup FromArchive(string pattern, DataArchive archive)
        => new()
        {
            Table = PaletteTable.FromArchive(pattern, archive),
            Palettes = Palette.FromArchive(pattern, archive)
        };

    public static PaletteLookup FromArchive(string tablePattern, string palettePattern, DataArchive archive)
        => new()
        {
            Table = PaletteTable.FromArchive(tablePattern, archive),
            Palettes = Palette.FromArchive(palettePattern, archive)
        };
    #endregion
}