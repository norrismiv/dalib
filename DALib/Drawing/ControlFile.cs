using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using DALib.Data;
using DALib.Extensions;
using DALib.Utility;

namespace DALib.Drawing;

public sealed class ControlFile : KeyedCollection<string, Control>
{
    public ControlFile(Stream stream, ControlFileParser? parser = null)
    {
        parser ??= new ControlFileParser();

        parser.Parse(this, stream);
    }

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

    public static ControlFile FromEntry(DataArchiveEntry entry, ControlFileParser? parser = null)
    {
        using var segment = entry.ToStreamSegment();

        return new ControlFile(segment, parser);
    }

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