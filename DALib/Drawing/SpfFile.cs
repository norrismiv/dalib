using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using DALib.Abstractions;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;
using DALib.IO;
using DALib.Utility;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class SpfFile : Collection<SpfFrame>, ISavable
{
    public uint ColorFormat { get; }
    public Palette PrimaryColors { get; }
    public Palette SecondaryColors { get; }
    public uint Unknown1 { get; }
    public uint Unknown2 { get; }

    private SpfFile(Palette primaryColors, Palette secondaryColors)
    {
        PrimaryColors = primaryColors;
        SecondaryColors = secondaryColors;
    }

    private SpfFile(Stream stream)
    {
        PrimaryColors = new Palette();
        SecondaryColors = new Palette();

        using var reader = new BinaryReader(stream, Encoding.Default, true);

        Unknown1 = reader.ReadUInt32();
        Unknown2 = reader.ReadUInt32();
        ColorFormat = reader.ReadUInt32();

        for (var i = 0; i < 256; i++)
            PrimaryColors[i] = reader.ReadRgb565Color(true);

        for (var i = 0; i < 256; i++)
            SecondaryColors[i] = reader.ReadArgb1555Color(true);

        var frameCount = reader.ReadUInt32();

        for (var i = 0; i < frameCount; i++)
        {
            var padWidth = reader.ReadUInt16();
            var padHeight = reader.ReadUInt16();
            var pixelWidth = reader.ReadUInt16();
            var pixelHeight = reader.ReadUInt16();
            _ = reader.ReadUInt32();
            var reserved = reader.ReadUInt32();
            var startAddress = reader.ReadUInt32();
            var byteWidth = reader.ReadUInt32();
            var byteCount = reader.ReadUInt32();
            var semiByteCount = reader.ReadUInt32();

            Add(
                new SpfFrame
                {
                    PadWidth = padWidth,
                    PadHeight = padHeight,
                    PixelWidth = pixelWidth,
                    PixelHeight = pixelHeight,
                    Reserved = reserved,
                    StartAddress = startAddress,
                    ByteWidth = byteWidth,
                    ByteCount = byteCount,
                    SemiByteCount = semiByteCount,
                    Data = new byte[byteCount]
                });
        }

        var totalByteAcount = reader.ReadUInt32();

        using var segment = new StreamSegment(stream, stream.Position, totalByteAcount);

        for (var i = 0; i < frameCount; i++)
        {
            var frame = Items[i];
            segment.Seek(frame.StartAddress, SeekOrigin.Begin);
            _ = segment.Read(frame.Data, 0, (int)frame.ByteCount);
        }
    }

    #region SaveTo
    public void Save(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".spf"),
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

        writer.Write(Unknown1);
        writer.Write(Unknown2);
        writer.Write(ColorFormat);

        for (var i = 0; i < CONSTANTS.COLORS_PER_PALETTE; i++)
            writer.WriteRgb565Color(PrimaryColors[i]);

        for (var i = 0; i < CONSTANTS.COLORS_PER_PALETTE; i++)
            writer.WriteRgb555Color(SecondaryColors[i]);

        writer.Write(Count);
        var startAddress = 0u;

        for (var i = 0; i < Count; i++)
        {
            var frame = Items[i];
            frame.StartAddress = startAddress;
            startAddress += frame.ByteCount;

            writer.Write(frame.PadWidth);
            writer.Write(frame.PadHeight);
            writer.Write(frame.PixelWidth);
            writer.Write(frame.PixelHeight);
            writer.Write(SpfFrame.Unknown);
            writer.Write(frame.Reserved);
            writer.Write(frame.StartAddress);
            writer.Write(frame.ByteWidth);
            writer.Write(frame.ByteCount);
            writer.Write(frame.SemiByteCount);
        }

        //startAddress is now the total byte count
        //since all frame data lengths have been added to it
        writer.Write(startAddress);

        for (var i = 0; i < Count; i++)
        {
            var frame = Items[i];
            writer.Write(frame.Data);
        }
    }
    #endregion

    #region LoadFrom
    public static SpfFile FromImages(IEnumerable<SKImage> orderedFrames) => FromImages(orderedFrames.ToArray());

    public static SpfFile FromImages(params SKImage[] orderedFrames)
    {
        using var quantized = ImageProcessor.QuantizeMultiple(SKColorType.Rgba8888, orderedFrames);
        (var images, var palette) = quantized;

        var spfFile = new SpfFile(palette, palette);

        for (var i = 0; i < images.Count; i++)
        {
            var image = images[i];

            spfFile.Add(
                new SpfFrame
                {
                    PadWidth = 0,
                    PadHeight = 0,
                    PixelWidth = (ushort)image.Width,
                    PixelHeight = (ushort)image.Height,
                    Reserved = 0,
                    StartAddress = 0,
                    ByteWidth = (uint)image.Width,
                    ByteCount = (uint)image.Width * (uint)image.Height,
                    SemiByteCount = (uint)image.Width * (uint)image.Height,
                    Data = image.GetPalettizedPixelData(palette)
                });
        }

        return spfFile;
    }

    public static SpfFile FromArchive(string fileName, DataArchive archive)
    {
        if (!archive.TryGetValue(fileName.WithExtension(".spf"), out var entry))
            throw new FileNotFoundException($"SPF file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }

    public static SpfFile FromEntry(DataArchiveEntry entry)
    {
        using var segment = entry.ToStreamSegment();

        return new SpfFile(segment);
    }

    public static SpfFile FromFile(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".spf"),
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new SpfFile(stream);
    }
    #endregion
}