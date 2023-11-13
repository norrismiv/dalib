using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;
using DALib.Memory;

namespace DALib.Drawing;

public class EpfFile
{
    public int Height { get; set; }
    public byte[] UnknownBytes { get; set; }

    public int Width { get; set; }
    public List<EpfFrame> Frames { get; }

    public EpfFile(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.Default, true);

        var frameCount = reader.ReadInt16();
        Width = reader.ReadInt16();
        Height = reader.ReadInt16();
        UnknownBytes = reader.ReadBytes(2);
        Frames = new List<EpfFrame>();
        var tocAddress = reader.ReadInt32() + 12;

        for (var i = 0; i < frameCount; ++i)
        {
            stream.Seek(tocAddress + i * 16, SeekOrigin.Begin);

            var top = reader.ReadInt16();
            var left = reader.ReadInt16();
            var bottom = reader.ReadInt16();
            var right = reader.ReadInt16();

            var width = right - left;
            var height = bottom - top;

            var startAddress = reader.ReadInt32() + 12;
            var endAddress = reader.ReadInt32() + 12;

            stream.Seek(startAddress, SeekOrigin.Begin);

            var data = endAddress - startAddress == width * height
                ? reader.ReadBytes(endAddress - startAddress)
                : reader.ReadBytes(tocAddress - startAddress);

            Frames.Add(
                new EpfFrame
                {
                    Top = top,
                    Left = left,
                    Bottom = bottom,
                    Right = right,
                    Data = data
                });
        }
    }

    public EpfFile(Span<byte> buffer)
    {
        var reader = new SpanReader(Encoding.Default, buffer, Endianness.LittleEndian);

        var frameCount = reader.ReadInt16();
        Width = reader.ReadInt16();
        Height = reader.ReadInt16();
        UnknownBytes = reader.ReadBytes(2);
        Frames = new List<EpfFrame>();
        var tocAddress = reader.ReadInt32() + 12;

        for (var i = 0; i < frameCount; ++i)
        {
            reader.Position = tocAddress + i * 16;

            var top = reader.ReadInt16();
            var left = reader.ReadInt16();
            var bottom = reader.ReadInt16();
            var right = reader.ReadInt16();

            var width = right - left;
            var height = bottom - top;

            var startAddress = reader.ReadInt32() + 12;
            var endAddress = reader.ReadInt32() + 12;

            reader.Position = startAddress;

            var data = endAddress - startAddress == width * height
                ? reader.ReadBytes(endAddress - startAddress)
                : reader.ReadBytes(tocAddress - startAddress);

            Frames.Add(
                new EpfFrame
                {
                    Top = top,
                    Left = left,
                    Bottom = bottom,
                    Right = right,
                    Data = data
                });
        }
    }

    public static EpfFile FromArchive(string fileName, DataArchive archive)
    {
        if(!archive.TryGetValue(fileName.WithExtension(".epf"), out var entry))
            throw new FileNotFoundException($"EPF file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }
    
    public static EpfFile FromEntry(DataArchiveEntry entry) => new(entry.ToStreamSegment());

    public static EpfFile FromFile(string path)
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

        return new EpfFile(stream);
    }
}