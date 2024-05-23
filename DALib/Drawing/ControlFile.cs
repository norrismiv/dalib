using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using DALib.Data;
using DALib.Extensions;
using DALib.Utility;

namespace DALib.Drawing;

/// <summary>
///     Represents a control file that contains a collection of controls.
/// </summary>
public sealed class ControlFile : KeyedCollection<string, Control>
{
    internal ControlFile(Stream stream, ControlFileParser? parser = null)
    {
        parser ??= new ControlFileParser();

        parser.Parse(this, stream);
    }

    /// <summary>
    ///     Loads all ControlFiles from the given <see cref="DataArchive" />.
    /// </summary>
    /// <param name="archive">
    ///     The <see cref="DataArchive" /> containing the control files.
    /// </param>
    /// <returns>
    ///     A dictionary of control files, where the key is the filename without extension and the value is the corresponding
    ///     <see cref="ControlFile" /> instance.
    /// </returns>
    public static Dictionary<string, ControlFile> FromArchive(DataArchive archive)
    {
        var controlFileLookup = new Dictionary<string, ControlFile>(StringComparer.OrdinalIgnoreCase);
        var parser = new ControlFileParser();

        foreach (var entry in archive.GetEntries(".txt"))
            try
            {
                var name = Path.GetFileNameWithoutExtension(entry.EntryName);
                var controlFile = FromEntry(entry, parser);

                controlFileLookup.Add(name, controlFile);
            } catch
            {
                //ignored because there's a chance not all .txt files will be a control files
            }

        return controlFileLookup;
    }

    /// <summary>
    ///     Loads a ControlFile from the specified archive entry.
    /// </summary>
    /// <param name="entry">
    ///     The DataArchiveEntry to create the ControlFile from.
    /// </param>
    /// <param name="parser">
    ///     An optional ControlFileParser to use for parsing the ControlFile. If this is not specified, the default parser will
    ///     be used
    /// </param>
    public static ControlFile FromEntry(DataArchiveEntry entry, ControlFileParser? parser = null)
    {
        using var segment = entry.ToStreamSegment();

        return new ControlFile(segment, parser);
    }

    /// <summary>
    ///     Loads a ControlFile from the specified path
    /// </summary>
    /// <param name="path">
    ///     The path of the control file to read.
    /// </param>
    /// <param name="parser">
    ///     An optional ControlFileParser to use for parsing the ControlFile. If this is not specified, the default parser will
    ///     be used
    /// </param>
    /// <returns>
    ///     A ControlFile object representing the contents of the control file.
    /// </returns>
    public static ControlFile FromFile(string path, ControlFileParser? parser = null)
    {
        using var stream = File.Open(
            path.WithExtension(".txt"),
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new ControlFile(stream, parser);
    }

    /// <inheritdoc />
    protected override string GetKeyForItem(Control item) => item.Name;
}