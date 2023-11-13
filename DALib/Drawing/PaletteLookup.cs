using System.Collections.Generic;
using DALib.Data;

namespace DALib.Drawing;

public sealed class PaletteLookup
{
    public required PaletteTable Table { get; set; }
    public required Dictionary<int, Palette> Palettes { get; set; }

    public Palette GetPaletteForId(int id)
    {
        var paletteNumber = Table.GetPaletteNumber(id);

        return Palettes[paletteNumber];
    }

    public static PaletteLookup FromArchive(string pattern, DataArchive archive) =>
        new()
        {
            Table = PaletteTable.FromArchive(pattern, archive),
            Palettes = Palette.FromArchive(pattern, archive)
        };
}