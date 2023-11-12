using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using DALib.Extensions;

namespace DALib.Data;

public sealed class MetaFile : Collection<MetaFileEntry>
{
    public MetaFile(Stream stream, bool leaveOpen = false)
    {
        var encoding = Encoding.GetEncoding(949);
        using var reader = new BinaryReader(stream, encoding, leaveOpen);

        var entryCount = reader.ReadInt16(true);

        for (var i = 0; i < entryCount; i++)
        {
            var entryName = reader.ReadString8(encoding);
            var propertyCount = reader.ReadInt16(true);
            var properties = new List<string>();

            for (var j = 0; j < propertyCount; ++j)
            {
                var propertyValue = reader.ReadString16(encoding, true);
                properties.Add(propertyValue);
            }

            var entry = new MetaFileEntry(entryName, properties);

            Add(entry);
        }
    }

    public static MetaFile FromFile(string path, bool isCompressed)
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

        if (!isCompressed)
            return new MetaFile(stream);

        using var decompressor = new DeflateStream(stream, CompressionMode.Decompress);

        return new MetaFile(decompressor);
    }
}