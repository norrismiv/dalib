using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using DALib.Data;
using DALib.Memory;

namespace DALib.Drawing;

public class PaletteTable : Collection<PaletteTableEntry>
{
    public PaletteTable() { }

    public PaletteTable(Stream stream)
    {
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            true,
            1024,
            true);

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
                    Add(new PaletteTableEntry(min, min, paletteNumOrMax));

                    break;
                }
                case 3 when int.TryParse(vals[2], out var paletteNumber):
                    Add(
                        paletteNumOrMax < min
                            ? new PaletteTableEntry(min, min, paletteNumber)
                            : new PaletteTableEntry(min, paletteNumOrMax, paletteNumber));

                    break;
            }
        }
    }

    public PaletteTable(Span<byte> buffer)
    {
        var reader = new SpanReader(Encoding.UTF8, buffer);

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
                    Add(new PaletteTableEntry(min, min, paletteNumOrMax));

                    break;
                }
                case 3 when int.TryParse(vals[2], out var paletteNumber):
                    Add(new PaletteTableEntry(min, paletteNumOrMax, paletteNumber));

                    break;
            }
        }
    }

    public static PaletteTable FromArchive(string pattern, DataArchive archive)
    {
        var table = new PaletteTable();

        foreach (var entry in archive)
        {
            if(!entry.EntryName.EndsWith(".tbl", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!entry.EntryName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                continue;

            var tablePart = FromEntry(entry);

            foreach (var pte in tablePart)
                table.Add(pte);
        }

        return table;
    }

    public static PaletteTable FromEntry(DataArchiveEntry entry) => new(entry.ToStreamSegment());

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
        foreach (var entry in Items)
            if ((tileNumber >= entry.MinTileNumber) && (tileNumber <= entry.MaxTileNumber))
                return entry.PaletteNumber;

        return 0;
    }
}