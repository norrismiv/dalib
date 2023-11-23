using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;
using DALib.IO;
using DALib.Memory;
using DALib.Utility;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class HpfFile
{
    public byte[] Data { get; }
    public byte[] HeaderBytes { get; }
    public int Height => Data.Length / CONSTANTS.HPF_TILE_WIDTH;

    private HpfFile(byte[] headerBytes, byte[] data)
    {
        HeaderBytes = headerBytes;
        Data = data;
    }

    private HpfFile(Stream stream)
        : this(stream.ToSpan()) { }

    private HpfFile(Span<byte> buffer)
    {
        var reader = new SpanReader(Encoding.Default, buffer, Endianness.LittleEndian);
        var signature = reader.ReadUInt32();

        if (signature == 0xFF02AA55)
            Compression.DecompressHpf(ref buffer);
        else
            Debugger.Break();

        HeaderBytes = buffer[..8].ToArray();
        Data = buffer[8..].ToArray();
    }

    #region SaveTo
    public void Save(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".hpf"),
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
        using var writer = new BinaryWriter(stream, Encoding.Default, true);

        writer.Write(HeaderBytes);
        writer.Write(Data);
    }
    #endregion

    #region LoadFrom
    public static Palettized<HpfFile> FromImage(SKImage image)
    {
        using var quantized = ImageProcessor.Quantize(SKColorType.Rgba8888, image);

        (var newImage, var palette) = ImageProcessor.Quantize(SKColorType.Rgba8888, image);

        return new Palettized<HpfFile>
        {
            Entity = new HpfFile(new byte[8], newImage.GetPalettizedPixelData(palette)),
            Palette = palette
        };
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
    #endregion
}