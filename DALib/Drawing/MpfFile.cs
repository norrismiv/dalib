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
using DALib.Utility;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class MpfFile : Collection<MpfFrame>, ISavable
{
    public byte Attack2FrameCount { get; set; }
    public byte Attack2StartIndex { get; set; }
    public byte Attack3FrameCount { get; set; }
    public byte Attack3StartIndex { get; set; }
    public byte AttackFrameCount { get; set; }
    public byte AttackFrameIndex { get; set; }
    public MpfFormatType FormatType { get; set; }
    public MpfHeaderType HeaderType { get; set; }
    public short Height { get; set; }
    public int PaletteNumber { get; set; }
    public byte StopFrameCount { get; set; }
    public byte StopFrameIndex { get; set; }
    public byte StopMotionFrameCount { get; set; }
    public byte StopMotionProbability { get; set; }
    public byte[] UnknownHeaderBytes { get; set; }
    public byte WalkFrameCount { get; set; }
    public byte WalkFrameIndex { get; set; }
    public short Width { get; set; }

    public MpfFile(
        MpfHeaderType headerType,
        MpfFormatType formatType,
        short width,
        short height)
    {
        HeaderType = headerType;
        FormatType = formatType;
        UnknownHeaderBytes = HeaderType == MpfHeaderType.Unknown ? new byte[4] : Array.Empty<byte>();
        Width = width;
        Height = height;
    }

    private MpfFile(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.Default, true);

        HeaderType = (MpfHeaderType)reader.ReadInt32();

        switch (HeaderType)
        {
            case MpfHeaderType.Unknown:
                //read 4 bytes
                var headerBytes = reader.ReadBytes(4);

                //convert those bytes to a number
                var num = BitConverter.ToInt32(headerBytes);

                //if they equal "4", read 8 more bytes into the header
                if (num == 4)
                {
                    var moreBytes = reader.ReadBytes(8);

                    headerBytes = headerBytes.Concat(moreBytes)
                                             .ToArray();
                }

                UnknownHeaderBytes = headerBytes;

                break;
            default:
                stream.Seek(-4, SeekOrigin.Current);

                UnknownHeaderBytes = Array.Empty<byte>();

                break;
        }

        var frameCount = reader.ReadByte();

        Width = reader.ReadInt16();
        Height = reader.ReadInt16();

        var dataLength = reader.ReadInt32();

        WalkFrameIndex = reader.ReadByte();
        WalkFrameCount = reader.ReadByte();

        FormatType = (MpfFormatType)reader.ReadInt16();

        switch (FormatType)
        {
            case MpfFormatType.MultipleAttacks:
                StopFrameIndex = reader.ReadByte();
                StopFrameCount = reader.ReadByte();
                StopMotionFrameCount = reader.ReadByte();
                StopMotionProbability = reader.ReadByte();
                AttackFrameIndex = reader.ReadByte();
                AttackFrameCount = reader.ReadByte();
                Attack2StartIndex = reader.ReadByte();
                Attack2FrameCount = reader.ReadByte();
                Attack3StartIndex = reader.ReadByte();
                Attack3FrameCount = reader.ReadByte();

                break;
            default:
                stream.Seek(-2, SeekOrigin.Current);

                AttackFrameIndex = reader.ReadByte();
                AttackFrameCount = reader.ReadByte();
                StopFrameIndex = reader.ReadByte();
                StopFrameCount = reader.ReadByte();
                StopMotionFrameCount = reader.ReadByte();
                StopMotionProbability = reader.ReadByte();

                break;
        }

        var dataStart = stream.Length - dataLength;

        using var dataSegment = stream.Slice(dataStart, dataLength);

        for (var i = 0; i < frameCount; ++i)
        {
            var left = reader.ReadInt16();
            var top = reader.ReadInt16();
            var right = reader.ReadInt16();
            var bottom = reader.ReadInt16();
            var centerX = reader.ReadInt16();
            var centerY = reader.ReadInt16();
            var startAddress = reader.ReadInt32();

            if ((left == -1) && (top == -1))
            {
                PaletteNumber = startAddress;
                --frameCount;

                continue;
            }

            var frameWidth = right - left;
            var frameHeight = bottom - top;

            Add(
                new MpfFrame
                {
                    Top = top,
                    Left = left,
                    Bottom = bottom,
                    Right = right,
                    CenterX = centerX,
                    CenterY = centerY,
                    StartAddress = startAddress,
                    Data = new byte[frameWidth * frameHeight]
                });
        }

        foreach (var frame in this)
        {
            dataSegment.Seek(frame.StartAddress, SeekOrigin.Begin);

            dataSegment.ReadExactly(frame.Data);
        }
    }

    #region SaveTo
    public void Save(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".mpf"),
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

        if (HeaderType == MpfHeaderType.Unknown)
        {
            writer.Write((int)HeaderType);
            writer.Write(UnknownHeaderBytes);
        }

        var frameCount = (byte)(Count + 1);

        writer.Write(frameCount);
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(this.Sum(frame => frame.Data.Length));
        writer.Write(WalkFrameIndex);
        writer.Write(WalkFrameCount);

        if (FormatType == MpfFormatType.MultipleAttacks)
        {
            writer.Write((short)FormatType);
            writer.Write(StopFrameIndex);
            writer.Write(StopFrameCount);
            writer.Write(StopMotionFrameCount);
            writer.Write(StopMotionProbability);
            writer.Write(AttackFrameIndex);
            writer.Write(AttackFrameCount);
            writer.Write(Attack2StartIndex);
            writer.Write(Attack2FrameCount);
            writer.Write(Attack3StartIndex);
            writer.Write(Attack3FrameCount);
        } else
        {
            writer.Write(AttackFrameIndex);
            writer.Write(AttackFrameCount);
            writer.Write(StopFrameIndex);
            writer.Write(StopFrameCount);
            writer.Write(StopMotionFrameCount);
            writer.Write(StopMotionProbability);
        }

        var startAddress = 0;

        foreach (var frame in this)
        {
            writer.Write(frame.Left);
            writer.Write(frame.Top);
            writer.Write(frame.Right);
            writer.Write(frame.Bottom);
            writer.Write(frame.CenterX);
            writer.Write(frame.CenterY);

            frame.StartAddress = startAddress;
            startAddress += frame.Data.Length;

            writer.Write(frame.StartAddress);
        }

        //write palette "frame"
        //byte.MaxValue == 0xFF, "-1" as a short is 0xFFFF
        var paletteFrameBuffer = Enumerable.Repeat(byte.MaxValue, 12)
                                           .ToArray();
        writer.Write(paletteFrameBuffer);
        writer.Write(PaletteNumber);

        foreach (var frame in this)
            writer.Write(frame.Data);
    }
    #endregion

    #region LoadFrom
    public static Palettized<MpfFile> FromImages(QuantizerOptions options, MpfFormatType formatType, IEnumerable<SKImage> orderedFrames)
        => FromImages(options, formatType, orderedFrames.ToArray());

    public static Palettized<MpfFile> FromImages(QuantizerOptions options, MpfFormatType formatType, params SKImage[] orderedFrames)
    {
        using var quantized = ImageProcessor.QuantizeMultiple(options, orderedFrames);

        (var images, var palette) = quantized;

        var width = (short)images.Max(img => img.Width);
        var height = (short)images.Max(img => img.Height);

        var mpfFile = new MpfFile(
            formatType == MpfFormatType.SingleAttack ? MpfHeaderType.None : MpfHeaderType.Unknown,
            formatType,
            width,
            height);

        foreach (var image in images)
            mpfFile.Add(
                new MpfFrame
                {
                    Right = (short)image.Width,
                    Bottom = (short)image.Height,
                    StartAddress = -1,
                    Data = image.GetPalettizedPixelData(palette)
                });

        return new Palettized<MpfFile>
        {
            Entity = mpfFile,
            Palette = palette
        };
    }

    public static MpfFile FromArchive(string fileName, DataArchive archive)
    {
        if (!archive.TryGetValue(fileName.WithExtension(".mpf"), out var entry))
            throw new FileNotFoundException($"MPF file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }

    public static MpfFile FromEntry(DataArchiveEntry entry)
    {
        using var segment = entry.ToStreamSegment();

        return new MpfFile(segment);
    }

    public static MpfFile FromFile(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".mpf"),
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new MpfFile(stream);
    }
    #endregion
}