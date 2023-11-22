using System;
using System.IO;
using System.Text;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;
using DALib.IO;
using DALib.Memory;

namespace DALib.Drawing;

public sealed class HpfFile
{
    public byte[] Data { get; }
    public byte[] HeaderBytes { get; }
    public int Height => Data.Length / CONSTANTS.HPF_TILE_WIDTH;

    public HpfFile(Stream stream)
        : this(stream.ToSpan()) { }

    public HpfFile(Span<byte> buffer)
    {
        var reader = new SpanReader(Encoding.Default, buffer, Endianness.LittleEndian);
        var signature = reader.ReadUInt32();

        if (signature == 0xFF02AA55)
            Compression.DecompressHpf(ref buffer);

        HeaderBytes = buffer[..8].ToArray();
        Data = buffer[8..].ToArray();
    }

    public static HpfFile FromArchive(string fileName, DataArchive archive)
    {
        if (!archive.TryGetValue(fileName.WithExtension(".hpf"), out var entry))
            throw new FileNotFoundException($"HPF file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }

    public static HpfFile FromEntry(DataArchiveEntry entry) => new(entry.ToSpan());

    public static HpfFile FromFile(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".hpf"),
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new HpfFile(stream);
    }
}