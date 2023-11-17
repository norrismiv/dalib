using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using DALib.Data;
using DALib.Definitions;
using DALib.Memory;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class Palette : Collection<SKColor>
{
    public Palette(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.Default, true);

        for (var i = 0; i < CONSTANTS.COLORS_PER_PALETTE; ++i)
            Add(new SKColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()));
    }

    public Palette(IEnumerable<SKColor> colors)
        : base(colors.ToList()) { }

    public Palette(Span<byte> buffer)
    {
        var reader = new SpanReader(Encoding.Default, buffer, Endianness.LittleEndian);

        for (var i = 0; i < CONSTANTS.COLORS_PER_PALETTE; ++i)
            Add(new SKColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()));
    }

    public Palette Dye(ColorTableEntry colorTableEntry)
    {
        var dyedPalette = new Palette(this);

        for (var i = 0; i < colorTableEntry.Colors.Length; ++i)
            dyedPalette[CONSTANTS.PALETTE_DYE_INDEX_START + i] = colorTableEntry.Colors[i];

        return dyedPalette;
    }

    public static Dictionary<int, Palette> FromArchive(string pattern, DataArchive archive)
    {
        var palettes = new Dictionary<int, Palette>();

        foreach (var entry in archive.GetEntries(pattern, ".pal"))
        {
            var paletteName = Path.GetFileNameWithoutExtension(entry.EntryName);
            var removePattern = paletteName.Replace(pattern, string.Empty);
            var paletteNum = int.Parse(removePattern);

            palettes.Add(paletteNum, FromEntry(entry));
        }

        return palettes;
    }

    public static Palette FromEntry(DataArchiveEntry entry) => new(entry.ToStreamSegment());

    public static Palette FromFile(string path)
    {
        using var stream = File.Open(
            path,
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new Palette(stream);
    }
}