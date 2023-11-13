using System;
using System.Collections.ObjectModel;
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

public sealed class SpfFile : Collection<SpfFrame>
{
    public SKColor[] Argb1555Colors { get; init; }
    public uint ColorFormat { get; init; }
    public SKColor[] Rgb565Colors { get; init; }
    public uint Unknown1 { get; init; }
    public uint Unknown2 { get; init; }

    public SpfFile(Stream stream)
    {
        Rgb565Colors = new SKColor[256];
        Argb1555Colors = new SKColor[256];

        using var reader = new BinaryReader(stream, Encoding.Default, true);

        Unknown1 = reader.ReadUInt32();
        Unknown2 = reader.ReadUInt32();
        ColorFormat = reader.ReadUInt32();

        for (var i = 0; i < 256; i++)
        {
            var color = reader.ReadUInt16();
            
            //@formatter:off
            var r = MathEx.ScaleRange<byte, byte>((byte)(color >> 11), 0, CONSTANTS.FIVE_BIT_MASK, 0, byte.MaxValue);
            var g = MathEx.ScaleRange<byte, byte>((byte)((color >> 5) & CONSTANTS.SIX_BIT_MASK), 0, CONSTANTS.SIX_BIT_MASK, 0, byte.MaxValue);
            var b = MathEx.ScaleRange<byte, byte>((byte)(color & CONSTANTS.FIVE_BIT_MASK), 0, CONSTANTS.FIVE_BIT_MASK, 0, byte.MaxValue);
            //@formatter:on

            Rgb565Colors[i] = new SKColor(r, g, b);
        }

        for (var i = 0; i < 256; i++)
        {
            var color = reader.ReadUInt16();
            
            //@formatter:off
            var r = MathEx.ScaleRange<byte, byte>((byte)(color >> 11), 0, CONSTANTS.FIVE_BIT_MASK, 0, byte.MaxValue);
            var g = MathEx.ScaleRange<byte, byte>((byte)((color >> 5) & CONSTANTS.FIVE_BIT_MASK), 0, CONSTANTS.FIVE_BIT_MASK, 0, byte.MaxValue);
            var b = MathEx.ScaleRange<byte, byte>((byte)(color & CONSTANTS.FIVE_BIT_MASK), 0, CONSTANTS.FIVE_BIT_MASK, 0, byte.MaxValue);
            var a = (color & 0b1) == 0b1; //maybe?
            //@formatter:on

            Argb1555Colors[i] = new SKColor(r, g, b);
        }

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

    public SpfFile(Span<byte> buffer)
    {
        Rgb565Colors = new SKColor[256];
        Argb1555Colors = new SKColor[256];

        var reader = new SpanReader(Encoding.Default, buffer, Endianness.LittleEndian);

        Unknown1 = reader.ReadUInt32();
        Unknown2 = reader.ReadUInt32();
        ColorFormat = reader.ReadUInt32();

        for (var i = 0; i < 256; i++)
        {
            var color = reader.ReadUInt16();
            
            //@formatter:off
            var r = MathEx.ScaleRange<byte, byte>((byte)(color >> 11), 0, CONSTANTS.FIVE_BIT_MASK, 0, byte.MaxValue);
            var g = MathEx.ScaleRange<byte, byte>((byte)((color >> 5) & CONSTANTS.SIX_BIT_MASK), 0, CONSTANTS.SIX_BIT_MASK, 0, byte.MaxValue);
            var b = MathEx.ScaleRange<byte, byte>((byte)(color & CONSTANTS.FIVE_BIT_MASK), 0, CONSTANTS.FIVE_BIT_MASK, 0, byte.MaxValue);
            //@formatter:on

            Rgb565Colors[i] = new SKColor(r, g, b);
        }

        for (var i = 0; i < 256; i++)
        {
            var color = reader.ReadUInt16();
            
            //@formatter:off
            var r = MathEx.ScaleRange<byte, byte>((byte)(color >> 11), 0, CONSTANTS.FIVE_BIT_MASK, 0, byte.MaxValue);
            var g = MathEx.ScaleRange<byte, byte>((byte)((color >> 5) & CONSTANTS.FIVE_BIT_MASK), 0, CONSTANTS.FIVE_BIT_MASK, 0, byte.MaxValue);
            var b = MathEx.ScaleRange<byte, byte>((byte)(color & CONSTANTS.FIVE_BIT_MASK), 0, CONSTANTS.FIVE_BIT_MASK, 0, byte.MaxValue);
            var a = (color & 0b1) == 0b1; //maybe?
            //@formatter:on

            Argb1555Colors[i] = new SKColor(r, g, b);
        }

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

        var segment = buffer[reader.Position..(int)(reader.Position + totalByteAcount)];

        for (var i = 0; i < frameCount; i++)
        {
            var frame = Items[i];
            segment[(int)frame.StartAddress..(int)(frame.StartAddress + frame.ByteCount)].CopyTo(frame.Data);
        }
    }

    public static SpfFile FromArchive(string fileName, DataArchive archive)
    {
        if(!archive.TryGetValue(fileName.WithExtension(".spf"), out var entry))
            throw new FileNotFoundException($"SPF file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }
    
    public static SpfFile FromEntry(DataArchiveEntry entry) => new(entry.ToStreamSegment());

    public static SpfFile FromFile(string path)
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

        return new SpfFile(stream);
    }
}