using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using DALib.Abstractions;
using DALib.Data;
using DALib.Extensions;
using DALib.Utility;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class EpfFile : Collection<EpfFrame>, ISavable
{
    public short Height { get; }
    public byte[] UnknownBytes { get; }
    public short Width { get; }

    private EpfFile(short width, short height)
    {
        Height = height;
        Width = width;
        UnknownBytes = new byte[2];
    }

    private EpfFile(Stream stream)
    {
        const int HEADER_LENGTH = 12;
        short frameCount;
        int tocAddress;

        using (var headerReader = new BinaryReader(stream, Encoding.Default, true))
        {
            frameCount = headerReader.ReadInt16();
            Width = headerReader.ReadInt16();
            Height = headerReader.ReadInt16();
            UnknownBytes = headerReader.ReadBytes(2);
            tocAddress = headerReader.ReadInt32();
        }

        using var segment = stream.Slice(HEADER_LENGTH, stream.Length - HEADER_LENGTH);
        using var reader = new BinaryReader(segment, Encoding.Default, true);

        for (var i = 0; i < frameCount; ++i)
        {
            segment.Seek(tocAddress + i * 16, SeekOrigin.Begin);

            var top = reader.ReadInt16();
            var left = reader.ReadInt16();
            var bottom = reader.ReadInt16();
            var right = reader.ReadInt16();

            var width = right - left;
            var height = bottom - top;

            var startAddress = reader.ReadInt32();
            var endAddress = reader.ReadInt32();

            segment.Seek(startAddress, SeekOrigin.Begin);

            var data = (endAddress - startAddress) == (width * height)
                ? reader.ReadBytes(endAddress - startAddress)
                : reader.ReadBytes(tocAddress - startAddress);

            if ((width == 0) || (height == 0))
                continue;

            Add(
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

    #region SaveTo
    public void Save(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".epf"),
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

        writer.Write((short)Count);
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(UnknownBytes);

        var footerStartAddress = this.Sum(frame => frame.Data.Length);

        writer.Write(footerStartAddress);

        //write frame data
        for (var i = 0; i < Count; i++)
        {
            var frame = this[i];

            writer.Write(frame.Data);
        }

        var dataIndex = 0;

        //write footers
        for (var i = 0; i < Count; i++)
        {
            var frame = this[i];

            var dataEndAddress = dataIndex + frame.Data.Length;

            writer.Write(frame.Top);
            writer.Write(frame.Left);
            writer.Write(frame.Bottom);
            writer.Write(frame.Right);
            writer.Write(dataIndex);
            writer.Write(dataEndAddress);

            dataIndex += frame.Data.Length;
        }
    }
    #endregion

    #region LoadFrom
    public static EpfFile FromArchive(string fileName, DataArchive archive)
    {
        if (!archive.TryGetValue(fileName.WithExtension(".epf"), out var entry))
            throw new FileNotFoundException($"EPF file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }

    public static EpfFile FromEntry(DataArchiveEntry entry)
    {
        using var segment = entry.ToStreamSegment();

        return new EpfFile(segment);
    }

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

    public static Palettized<EpfFile> FromImages(IEnumerable<SKImage> orderedFrames) => FromImages(orderedFrames.ToArray());

    public static Palettized<EpfFile> FromImages(params SKImage[] orderedFrames)
    {
        using var quantized = ImageProcessor.QuantizeMultiple(SKColorType.Rgba8888, orderedFrames);
        (var images, var palette) = quantized;
        var imageWidth = (short)orderedFrames.Select(img => img.Width).Max();
        var imageHeight = (short)orderedFrames.Select(img => img.Height).Max();
        var epfFile = new EpfFile(imageWidth, imageHeight);

        for (var i = 0; i < images.Count; i++)
        {
            var image = images[i];

            epfFile.Add(
                new EpfFrame
                {
                    Top = 0,
                    Left = 0,
                    Right = (short)image.Width,
                    Bottom = (short)image.Height,
                    Data = image.GetPalettizedPixelData(palette)
                });
        }

        return new Palettized<EpfFile>
        {
            Entity = epfFile,
            Palette = palette
        };
    }
    #endregion
}