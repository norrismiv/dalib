using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Text;
using DALib.Data;
using DALib.Extensions;

namespace DALib.Drawing;

public class MpfFile : Collection<MpfFrame>
{
    public int Attack2FrameCount { get; init; }
    public int Attack2StartIndex { get; init; }
    public int Attack3FrameCount { get; init; }
    public int Attack3StartIndex { get; init; }
    public int AttackFrameCount { get; init; }
    public int AttackFrameIndex { get; init; }
    public int Height { get; init; }
    public int PaletteNumber { get; init; }
    public int StopFrameCount { get; init; }
    public int StopFrameIndex { get; init; }
    public int StopMotionFrameCount { get; init; }
    public int StopMotionProbability { get; init; }
    public byte[] UnknownBytes { get; init; }
    public int WalkFrameCount { get; init; }
    public int WalkFrameIndex { get; init; }

    public int Width { get; }
    public Size Size => new(Width, Height);

    public MpfFile(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);

        if (reader.ReadInt32() == -1)
        {
            var unknown = reader.ReadInt32();
            UnknownBytes = unknown == 4 ? reader.ReadBytes(8) : BitConverter.GetBytes(unknown);
        } else
        {
            UnknownBytes = Array.Empty<byte>();
            stream.Seek(-4, SeekOrigin.Current);
        }

        var frameCount = reader.ReadByte();

        Width = reader.ReadInt16();
        Height = reader.ReadInt16();

        var dataLength = reader.ReadInt32();

        WalkFrameIndex = reader.ReadByte();
        WalkFrameCount = reader.ReadByte();

        if (reader.ReadInt16() == -1)
        {
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
        } else
        {
            stream.Seek(-2, SeekOrigin.Current);
            AttackFrameIndex = reader.ReadByte();
            AttackFrameCount = reader.ReadByte();
            StopFrameIndex = reader.ReadByte();
            StopFrameCount = reader.ReadByte();
            StopMotionFrameCount = reader.ReadByte();
            StopMotionProbability = reader.ReadByte();
        }

        var dataStart = stream.Length - dataLength;

        for (var i = 0; i < frameCount; ++i)
        {
            var left = reader.ReadInt16();
            var top = reader.ReadInt16();
            var right = reader.ReadInt16();
            var bottom = reader.ReadInt16();
            var xOffset = reader.ReadInt16(true);
            var yOffset = reader.ReadInt16(true);
            var startAddress = reader.ReadInt32();

            if ((left == -1) && (top == -1))
            {
                PaletteNumber = startAddress;
                --frameCount;

                continue;
            }

            var frameWidth = right - left;
            var frameHeight = bottom - top;

            byte[] data;

            if ((frameWidth > 0) && (frameHeight > 0))
            {
                var position = stream.Position;
                stream.Seek(dataStart + startAddress, SeekOrigin.Begin);
                data = reader.ReadBytes(frameWidth * frameHeight);
                stream.Seek(position, SeekOrigin.Begin);
            } else
            {
                data = Array.Empty<byte>();
            }

            Add(
                new MpfFrame
                {
                    Top = top,
                    Left = left,
                    Bottom = bottom,
                    Right = right,
                    XOffset = xOffset,
                    YOffset = yOffset,
                    Data = data
                });
        }
    }

    public static MpfFile FromArchive(string fileName, DataArchive archive)
    {
        if (!archive.TryGetValue(fileName.WithExtension(".mpf"), out var entry))
            throw new FileNotFoundException($"MPF file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }

    public static MpfFile FromEntry(DataArchiveEntry entry) => new(entry.ToStreamSegment());

    public static MpfFile FromFile(string path)
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

        return new MpfFile(stream);
    }
}