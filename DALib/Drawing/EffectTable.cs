using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using DALib.Abstractions;
using DALib.Data;
using DALib.Extensions;

namespace DALib.Drawing;

/// <summary>
///     Represents a table of effect frame orders
/// </summary>
public class EffectTable : ISavable
{
    private readonly List<EffectTableEntry> Entries = [];

    /// <summary>
    ///     Initializes a new instance of the EffectTable class.
    /// </summary>
    public EffectTable() { }

    private EffectTable(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        //effect count, but not rly important
        if (!int.TryParse(reader.ReadLine(), out _))
            return;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();

            if (string.IsNullOrEmpty(line))
            {
                Entries.Add([]);

                continue;
            }

            var split = line.Split(' ');
            var frameOrder = new List<int>();

            for (var i = 0; i < split.Length; ++i)
            {
                var valueStr = split[i]
                    .Trim();

                if (!int.TryParse(valueStr, out var frameIndex))
                    continue;

                frameOrder.Add(frameIndex);
            }

            var entry = new EffectTableEntry
            {
                FrameSequence = frameOrder
            };
            Entries.Add(entry);
        }
    }

    /// <summary>
    ///     Adds a new frame order to the end of the table (effect number would be Entries.Count after the add)
    /// </summary>
    /// <param name="frameOrder">
    ///     The frame order to add
    /// </param>
    public void Add(IEnumerable<int> frameOrder)
        => Entries.Add(
            new EffectTableEntry
            {
                FrameSequence = frameOrder.ToList()
            });

    /// <summary>
    ///     Adds a new EFA frame order ("0") to the end of the table (effect number would be Entries.Count after the add)
    /// </summary>
    public void AddEfa()
        => Entries.Add(
            new EffectTableEntry
            {
                FrameSequence = [0]
            });

    /// <summary>
    ///     Gets the next available effect id
    /// </summary>
    public int GetNextEffectId() => Entries.Count;

    /// <summary>
    ///     Inserts a frame order for the specified effect number
    /// </summary>
    /// <param name="effectNum">
    ///     The effect number for which to insert (1-based)
    /// </param>
    /// <param name="frameOrder">
    ///     The frame order for the effect
    /// </param>
    public void Insert(int effectNum, IEnumerable<int> frameOrder)
    {
        var entry = new EffectTableEntry
        {
            FrameSequence = frameOrder.ToList()
        };
        Entries.Insert(effectNum - 1, entry);
    }

    /// <summary>
    ///     Inserts an EFA frame order ("0") for the specified effect number
    /// </summary>
    public void InsertEfa()
    {
        var entry = new EffectTableEntry
        {
            FrameSequence = [0]
        };
        Entries.Insert(0, entry);
    }

    /// <summary>
    ///     Clears the frame order for the specified effect id
    /// </summary>
    /// <param name="effectNum">
    ///     The effect number to clear the frame order for. (1-indexed)
    /// </param>
    public void Remove(int effectNum) => Entries[effectNum - 1] = [];

    /// <summary>
    ///     Retrieves the frame order for the specified effect number
    /// </summary>
    /// <param name="effectId">
    ///     The effect id to get the frame order for
    /// </param>
    /// <param name="entry">
    ///     A table entry containing the frame order for the specified effect id
    /// </param>
    public bool TryGetEntry(int effectId, [NotNullWhen(true)] out EffectTableEntry? entry)
    {
        if ((effectId < 1) || (effectId > Entries.Count))
        {
            entry = default;

            return false;
        }

        entry = Entries[effectId - 1];

        return true;
    }

    #region SaveTo
    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Save(Stream stream)
    {
        using var writer = new StreamWriter(stream, Encoding.Default, leaveOpen: true);

        writer.WriteLine(Entries.Count);

        foreach (var entry in Entries)
        {
            var line = string.Join(' ', entry);
            writer.WriteLine(line);
        }
    }
    #endregion

    #region LoadFrom
    /// <summary>
    ///     Loads an EffectTable from the specified archive
    /// </summary>
    /// <param name="rohDat">
    ///     The DataArchive from which to retrieve the TBL file. (must be roh.dat)
    /// </param>
    public static EffectTable FromArchive(DataArchive rohDat)
    {
        if (!rohDat.TryGetValue("effect.tbl", out var entry))
            throw new FileNotFoundException("TBL file with the name \"effect.tbl\" was not found in the archive");

        return FromEntry(entry);
    }

    /// <summary>
    ///     Loads an EffectTable from the specified entry
    /// </summary>
    /// <param name="entry">
    ///     The DataArchiveEntry to load the EffectTable from.
    /// </param>
    public static EffectTable FromEntry(DataArchiveEntry entry)
    {
        using var segment = entry.ToStreamSegment();

        return new EffectTable(segment);
    }

    /// <summary>
    ///     Loads an EffectTable from the specified path
    /// </summary>
    /// <param name="path">
    ///     The path to the file to be read
    /// </param>
    public static EffectTable FromFile(string path)
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

        return new EffectTable(stream);
    }
    #endregion
}