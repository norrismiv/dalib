using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DALib.Abstractions;
using DALib.Data;
using DALib.Extensions;

namespace DALib.Drawing;

/// <remarks>
///     As a palette table is populated, newer entries override older ones. This is intended behavior.
///     In my opinion this makes it meaningless to store and search through all of the entries.
///     You could search through them in reverse order and return the first one you find, but even still...
///     It should be faster this way, where each id is mapped to a palette number
/// </remarks>
public class PaletteTable : ISavable
{
    protected IDictionary<int, int> Entries { get; set; } = new Dictionary<int, int>();
    protected IDictionary<int, int> Overrides { get; set; } = new Dictionary<int, int>();

    public PaletteTable() { }

    private PaletteTable(Stream stream)
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

    public virtual void Add(int id, int paletteNumber) => Overrides[id] = paletteNumber;

    public virtual PaletteTable Freeze() => new FrozenPaletteTable(this);

    public int GetPaletteNumber(int id)
    {
        if (Overrides.TryGetValue(id, out var paletteNumber))
            return paletteNumber;

        if (Entries.TryGetValue(id, out paletteNumber))
            return paletteNumber;

        return 0;
    }

    public virtual void Merge(PaletteTable other)
    {
        foreach (var kvp in other.Overrides)
            Overrides[kvp.Key] = kvp.Value;

        foreach (var kvp in other.Entries)
            Entries[kvp.Key] = kvp.Value;
    }

    public virtual void Remove(int id)
    {
        Overrides.Remove(id);
        Entries.Remove(id);
    }

    private sealed class FrozenPaletteTable : PaletteTable
    {
        public FrozenPaletteTable(PaletteTable table)
        {
            base.Merge(table);

            Entries = Entries.ToFrozenDictionary();
            Overrides = Overrides.ToFrozenDictionary();
        }

        public override void Add(int id, int paletteNumber) => throw new NotSupportedException("The collection is frozen");

        public override PaletteTable Freeze() => this;

        public override void Merge(PaletteTable other) => throw new NotSupportedException("The collection is frozen");

        public override void Remove(int id) => throw new NotSupportedException("The collection is frozen");
    }

    #region SaveTo
    public void Save(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".tbl"),
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
        //construct a dictionary of all entries, with overrides applied
        var entries = Entries.ToDictionary();

        foreach (var kvp in Overrides)
            entries[kvp.Key] = kvp.Value;

        var orderedEntries = entries.OrderBy(kvp => kvp.Key);

        foreach (var set in orderedEntries.GroupBy(kvp => kvp.Value))
        {
            var orderedKeys = set.Select(kvp => kvp.Key)
                                 .Order()
                                 .ToArray();
            var ranges = new List<Range>();

            //extract ranges of consecutive keys for the same palette
            for (var i = 0; i < orderedKeys.Length; ++i)
            {
                var start = orderedKeys[i];

                while ((i < (orderedKeys.Length - 1)) && ((orderedKeys[i] + 1) == orderedKeys[i + 1]))
                    i++;

                var end = orderedKeys[i];
                ranges.Add(new Range(start, end));
            }

            using var writer = new StreamWriter(stream, leaveOpen: true);

            foreach (var range in ranges)
            {
                var paletteId = set.Key;

                writer.WriteLine(
                    range.Start.Value == range.End.Value
                        ? $"{range.Start.Value} {paletteId}"
                        : $"{range.Start.Value} {range.End.Value} {paletteId}");
            }
        }
    }
    #endregion

    #region LoadFrom
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
            path.WithExtension(".tbl"),
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new PaletteTable(stream);
    }
    #endregion
}