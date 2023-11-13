using System;
using System.IO;
using DALib.Data;
using DALib.Extensions;
using DALib.IO;

namespace DALib.Drawing;

public class HpfFile
{
    public byte[] Data { get; }
    public byte[] HeaderBytes { get; }
    public int Height => Data.Length / Width;
    public int Width => 28;

    public HpfFile(Stream stream)
        : this(stream.ToSpan()) { }

    public HpfFile(Span<byte> buffer)
    {
        if (buffer[0] == 0x55)
            Compression.DecompressHpf(ref buffer);
        
        HeaderBytes = buffer[..8].ToArray();
        Data = buffer[8..].ToArray();
    }
    
    public static HpfFile FromArchive(string fileName, DataArchive archive)
    {
        if(!archive.TryGetValue(fileName.WithExtension(".hpf"), out var entry))
            throw new FileNotFoundException($"HPF file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }
    
    public static HpfFile FromEntry(DataArchiveEntry entry) => new(entry.ToSpan());

    public static HpfFile FromFile(string path)
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

        return new HpfFile(stream);
    }
}