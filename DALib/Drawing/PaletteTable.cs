using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DALib.Data;
using DALib.Definitions;
using DALib.Memory;

namespace DALib.Drawing;

/// <remarks>
///     As a palette table is populated, newer entries override older ones. This is intended behavior.
///     In my opinion this makes it meaningless to store and search through all of the entries.
///     You could search through them in reverse order and return the first one you find, but even still...
///     It should be faster this way, where each id is mapped to a palette number
/// </remarks>
public sealed class PaletteTable
{
    private readonly Dictionary<int, int> Entries = new();
    private readonly Dictionary<int, int> Overrides = new();

    public PaletteTable() { }

    public PaletteTable(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();

            if (string.IsNullOrEmpty(line))
                continue;

            var vals = line.Split(' ');

            if ((vals.Length < 2) || !int.TryParse(vals[0], out var min) || !int.TryParse(vals[1], out var paletteNumOrMax))
                continue;

            switch (vals.Length)
            {
                case 2:
                {
                    Overrides[min] = paletteNumOrMax;

                    break;
                }
                case 3 when int.TryParse(vals[2], out var paletteNumber):
                    for (var i = min; i <= paletteNumOrMax; ++i)
                        Entries[i] = paletteNumber;

                    break;
            }
        }
    }

    public PaletteTable(Span<byte> buffer)
    {
        var reader = new SpanReader(Encoding.UTF8, buffer, Endianness.LittleEndian);

        while (!reader.EndOfSpan)
        {
            var line = reader.ReadString();

            if (string.IsNullOrEmpty(line))
                continue;

            var vals = line.Split(' ');

            if ((vals.Length < 2) || !int.TryParse(vals[0], out var min) || !int.TryParse(vals[1], out var paletteNumOrMax))
                continue;

            switch (vals.Length)
            {
                case 2:
                {
                    Overrides[min] = paletteNumOrMax;

                    break;
                }
                case 3 when int.TryParse(vals[2], out var paletteNumber):
                    for (var i = min; i <= paletteNumOrMax; ++i)
                        Entries[i] = paletteNumber;

                    break;
            }
        }
    }

    public static PaletteTable FromArchive(string pattern, DataArchive archive)
    {
        var table = new PaletteTable();

        foreach (var entry in archive.GetEntries(pattern, ".tbl"))
        {
            var tablePart = FromEntry(entry);

            table.Merge(tablePart);
        }

        return table;
    }

    public static PaletteTable FromEntry(DataArchiveEntry entry)
    {
        using var segment = entry.ToStreamSegment();

        return new PaletteTable(segment);
    }

    public static PaletteTable FromFile(string path)
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

        return new PaletteTable(stream);
    }

    public int GetPaletteNumber(int tileNumber)
    {
        if (Overrides.TryGetValue(tileNumber, out var paletteNumber))
            return paletteNumber;

        if (Entries.TryGetValue(tileNumber, out paletteNumber))
            return paletteNumber;

        return 0;
    }

    public void Merge(PaletteTable other)
    {
        foreach (var kvp in other.Overrides)
            Overrides[kvp.Key] = kvp.Value;

        foreach (var kvp in other.Entries)
            Entries[kvp.Key] = kvp.Value;
    }
}