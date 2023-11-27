using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using DALib.Abstractions;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class Palette : Collection<SKColor>, ISavable
{
    private Palette(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.Default, true);

        for (var i = 0; i < CONSTANTS.COLORS_PER_PALETTE; ++i)
            Add(new SKColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()));
    }

    internal Palette()
        : base(
            Enumerable.Repeat(SKColor.Empty, CONSTANTS.COLORS_PER_PALETTE)
                      .ToList()) { }

    internal Palette(IEnumerable<SKColor> colors)
        : this()
    {
        var index = 0;

        foreach (var color in colors)
            this[index++] = color;
    }

    public Palette Dye(ColorTableEntry colorTableEntry, int dyeIndexStart = CONSTANTS.PALETTE_DYE_INDEX_START)
    {
        var dyedPalette = new Palette(this);

        for (var i = 0; i < colorTableEntry.Colors.Length; ++i)
            dyedPalette[dyeIndexStart + i] = colorTableEntry.Colors[i];

        return dyedPalette;
    }

    #region SaveTo
    public void Save(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".pal"),
            new FileStreamOptions
            {
                Access = FileAccess.Write,
                Mode = FileMode.Create,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        Save(stream);
    }

    public void Save(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.Default, true);

        for (var i = 0; i < Count; ++i)
        {
            var color = this[i];

            writer.Write(color.Red);
            writer.Write(color.Green);
            writer.Write(color.Blue);
        }

        //pad the palette with black to 256 colors
        for (var i = Count; i < CONSTANTS.COLORS_PER_PALETTE; ++i)
        {
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
        }
    }
    #endregion

    #region LoadFrom
    public static Dictionary<int, Palette> FromArchive(string pattern, DataArchive archive)
    {
        var palettes = new Dictionary<int, Palette>();

        foreach (var entry in archive.GetEntries(pattern, ".pal"))
        {
            if (!entry.TryGetNumericIdentifier(out var identifier))
                continue;

            palettes.Add(identifier, FromEntry(entry));
        }

        return palettes;
    }

    public static Palette FromEntry(DataArchiveEntry entry)
    {
        using var segment = entry.ToStreamSegment();

        return new Palette(segment);
    }

    public static Palette FromFile(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".pal"),
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new Palette(stream);
    }
    #endregion
}