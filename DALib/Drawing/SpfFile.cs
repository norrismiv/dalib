using System;
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
    public SpfFormatType Format { get; set; }
    public Palette PrimaryColors { get; set; }
    public Palette SecondaryColors { get; set; }
    public uint Unknown1 { get; set; }
    public uint Unknown2 { get; set; }

    public SpfFile(Palette primaryColors, Palette secondaryColors)
    {
        Format = SpfFormatType.Palettized;
        PrimaryColors = primaryColors;
        SecondaryColors = secondaryColors;
    }

    public SpfFile()
    {
        Format = SpfFormatType.Colorized;
        PrimaryColors = new Palette();
        SecondaryColors = new Palette();
    }

    private SpfFile(Stream stream)
    {
        PrimaryColors = new Palette();
        SecondaryColors = new Palette();

        using var reader = new BinaryReader(stream, Encoding.Default, true);

        Unknown1 = reader.ReadUInt32();
        Unknown2 = reader.ReadUInt32();
        Format = (SpfFormatType)reader.ReadUInt32();

        switch (Format)
        {
            case SpfFormatType.Colorized:
                ReadColorized(reader);

                break;
            case SpfFormatType.Palettized:
                ReadPalettized(reader);

                break;
            default:
                throw new InvalidDataException($"SPF format \"{Format}\" is not supported");
        }
    }

    private void ReadColorized(BinaryReader reader)
    {
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
                    ColorData = new SKColor[pixelWidth * pixelHeight]
                });
        }

        var totalByteAcount = reader.ReadUInt32();

        using var segment = new StreamSegment(reader.BaseStream, reader.BaseStream.Position, totalByteAcount);

        for (var i = 0; i < frameCount; i++)
        {
            var frame = Items[i];
            segment.Seek(frame.StartAddress, SeekOrigin.Begin);
            var index = 0;

            for (var y = 0; y < frame.PixelHeight; y++)
            {
                for (var x = 0; x < frame.PixelWidth; x++)
                {
                    var color = reader.ReadRgb565Color();
                    frame.ColorData![index++] = color;
                }
            }
        }
    }

    private void ReadPalettized(BinaryReader reader)
    {
        for (var i = 0; i < 256; i++)
            PrimaryColors[i] = reader.ReadRgb565Color();

        for (var i = 0; i < 256; i++)
            SecondaryColors[i] = reader.ReadRgb555Color();

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

        using var segment = new StreamSegment(reader.BaseStream, reader.BaseStream.Position, totalByteAcount);

        for (var i = 0; i < frameCount; i++)
        {
            var frame = Items[i];
            segment.Seek(frame.StartAddress, SeekOrigin.Begin);
            _ = segment.Read(frame.Data!, 0, (int)frame.ByteCount);
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

    //TODO: maybe make this always save colorized SPFs?
    public void Save(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.Default, true);

        writer.Write(Unknown1);
        writer.Write(Unknown2);
        writer.Write((int)Format);

        switch (Format)
        {
            case SpfFormatType.Palettized:
                SavePalettized(writer);

                break;
            case SpfFormatType.Colorized:
                SaveColorized(writer);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SaveColorized(BinaryWriter writer)
    {
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

            //write primary color data
            for (var y = 0; y < frame.PixelHeight; y++)
            {
                for (var x = 0; x < frame.PixelWidth; x++)
                {
                    var color = frame.ColorData![y * frame.PixelWidth + x];
                    writer.WriteRgb565Color(color);
                }
            }

            //write secondary color dta
            for (var y = 0; y < frame.PixelHeight; y++)
            {
                for (var x = 0; x < frame.PixelWidth; x++)
                {
                    var color = frame.ColorData![y * frame.PixelWidth + x];
                    writer.WriteRgb555Color(color);
                }
            }
        }
    }

    private void SavePalettized(BinaryWriter writer)
    {
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
            writer.Write(frame.Data!);
        }
    }
    #endregion

    #region LoadFrom
    public static SpfFile FromImages(IEnumerable<SKImage> orderedFrames) => FromImages(orderedFrames.ToArray());

    public static SpfFile FromImages(params SKImage[] orderedFrames)
    {
        var spfFile = new SpfFile();

        for (var i = 0; i < orderedFrames.Length; i++)
        {
            var image = orderedFrames[i];
            using var bitmap = SKBitmap.FromImage(image);

            var frame = new SpfFrame
            {
                PadWidth = 0,
                PadHeight = 0,
                PixelWidth = (ushort)image.Width,
                PixelHeight = (ushort)image.Height,
                Reserved = 0,
                StartAddress = 0,
                ByteWidth = (uint)image.Width * 2,
                ByteCount = (uint)(image.Width * image.Height * 4), //2 bytes per pixel, 2 copies of image
                SemiByteCount = (uint)(image.Width * image.Height * 2),
                ColorData = new SKColor[image.Width * image.Height]
            };

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    frame.ColorData[y * image.Width + x] = color;
                }
            }

            spfFile.Add(frame);
        }

        return spfFile;
    }

    public static SpfFile FromImages(QuantizerOptions options, IEnumerable<SKImage> orderedFrames)
        => FromImages(options, orderedFrames.ToArray());

    public static SpfFile FromImages(QuantizerOptions options, params SKImage[] orderedFrames)
    {
        using var quantized = ImageProcessor.QuantizeMultiple(options, orderedFrames);
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