using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DALib.Abstractions;
using DALib.Data;
using DALib.Extensions;

namespace DALib.Drawing;

/// <summary>
///     Represents a table of tile animations.
/// </summary>
public class TileAnimationTable : ISavable
{
    private readonly Dictionary<int, TileAnimationEntry> Entries = new();

    /// <summary>
    ///     Initializes a new instance of the TileAnimationTable class.
    /// </summary>
    public TileAnimationTable() { }

    private TileAnimationTable(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();

            if (string.IsNullOrEmpty(line))
                continue;

            var split = line.Split(' ');
            var entry = new TileAnimationEntry();

            for (var i = 0; i < split.Length; ++i)
            {
                var valueStr = split[i]
                    .Trim();

                if (!ushort.TryParse(valueStr, out var value))
                    continue;

                //the last value is the animation interval
                if (i == (split.Length - 1))
                    entry.AnimationIntervalMs = value * 100;
                else //all other values are tile ids
                {
                    entry.TileSequence.Add(value);

                    //each entry can be looked up by any tile part of the sequence
                    Entries[value] = entry;
                }
            }
        }
    }

    /// <summary>
    ///     Adds a TileAnimationEntry to the table
    /// </summary>
    /// <param name="entry">
    ///     The TileAnimationEntry to be added.
    /// </param>
    public void Add(TileAnimationEntry entry)
    {
        foreach (var tileId in entry.TileSequence)
            Entries[tileId] = entry;
    }

    /// <summary>
    ///     Removes a TileAnimationEntry from the table
    /// </summary>
    /// <param name="entry">
    ///     The TileAnimationEntry to remove.
    /// </param>
    public void Remove(TileAnimationEntry entry)
    {
        foreach (var tileId in entry.TileSequence)
            Entries.Remove(tileId);
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

        foreach (var entry in Entries.Values.Distinct())
        {
            var line = string.Join(' ', entry.TileSequence);

            line += $" {entry.AnimationIntervalMs / 100:D0}";

            writer.WriteLine(line);
        }
    }
    #endregion

    #region LoadFrom
    /// <summary>
    ///     Loads a TileAnimationTable with the specified fileName from the specified archive
    /// </summary>
    /// <param name="fileName">
    ///     The name of the TBL file to search for in the archive
    /// </param>
    /// <param name="archive">
    ///     The DataArchive from which to retrieve the TBL file.
    /// </param>
    public static TileAnimationTable FromArchive(string fileName, DataArchive archive)
    {
        if (!archive.TryGetValue(fileName.WithExtension(".tbl"), out var entry))
            throw new FileNotFoundException($"TBL file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }

    /// <summary>
    ///     Loads a TileAnimationTable from the specified entry
    /// </summary>
    /// <param name="entry">
    ///     The DataArchiveEntry to load the TileAnimationTable from.
    /// </param>
    public static TileAnimationTable FromEntry(DataArchiveEntry entry)
    {
        using var segment = entry.ToStreamSegment();

        return new TileAnimationTable(segment);
    }

    /// <summary>
    ///     Loads a TileAnimationTable from the specified path
    /// </summary>
    /// <param name="path">
    ///     The path to the file to be read
    /// </param>
    public static TileAnimationTable FromFile(string path)
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

        return new TileAnimationTable(stream);
    }
    #endregion
}