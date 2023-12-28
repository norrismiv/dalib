using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;

namespace DALib.Drawing;

public class PaletteLookup
{
    public IDictionary<int, Palette> Palettes { get; set; }
    public PaletteTable Table { get; set; }

    private PaletteLookup(IDictionary<int, Palette> palettes, PaletteTable table)
    {
        Palettes = palettes;
        Table = table;
    }

    public virtual PaletteLookup Freeze() => new FrozenPaletteLookup(this);

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

    private sealed class FrozenPaletteLookup : PaletteLookup
    {
        public FrozenPaletteLookup(PaletteLookup paletteLookup)
            : base(paletteLookup.Palettes, paletteLookup.Table)
        {
            Palettes = Palettes.ToFrozenDictionary();
            Table = Table.Freeze();
        }

        public override PaletteLookup Freeze() => this;
    }

    #region LoadFrom
    public static PaletteLookup FromArchive(string pattern, DataArchive archive)
        => new(Palette.FromArchive(pattern, archive), PaletteTable.FromArchive(pattern, archive));

    public static PaletteLookup FromArchive(string tablePattern, string palettePattern, DataArchive archive)
        => new(Palette.FromArchive(palettePattern, archive), PaletteTable.FromArchive(tablePattern, archive));
    #endregion
}