using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using DALib.Abstractions;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;
using DALib.IO;
using DALib.Memory;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class EfaFile : Collection<EfaFrame>, ISavable
{
    public EfaBlendingType BlendingType { get; set; }
    public int FrameIntervalMs { get; set; }
    public int Unknown1 { get; set; }
    public byte[] Unknown2 { get; set; }

    public EfaFile()
    {
        Unknown1 = 0;
        BlendingType = EfaBlendingType.Luminance;
        FrameIntervalMs = 50;
        Unknown2 = new byte[51];
    }

    private EfaFile(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.Default, true);

        Unknown1 = reader.ReadInt32();
        var frameCount = reader.ReadInt32();
        FrameIntervalMs = reader.ReadInt32();
        var blendingType = reader.ReadByte();
        BlendingType = (EfaBlendingType)blendingType;
        Unknown2 = reader.ReadBytes(51);

        for (var i = 0; i < frameCount; i++)
        {
            var unknown1 = reader.ReadInt32();
            var offset = reader.ReadInt32();
            var size = reader.ReadInt32();
            var rawSize = reader.ReadInt32();
            var unknown2 = reader.ReadInt32();
            var unknown3 = reader.ReadInt32();
            var byteWidth = reader.ReadInt32();
            var unknown4 = reader.ReadInt32();
            var byteCount = reader.ReadInt32();
            var unknown5 = reader.ReadInt32();
            var originX = reader.ReadInt16();
            var originY = reader.ReadInt16();
            var originFlags = reader.ReadInt32();
            var imageWidth = reader.ReadInt16();
            var imageHeight = reader.ReadInt16();
            var pad1Flags = reader.ReadInt32();
            var frameWidth = reader.ReadInt16();
            var frameHeight = reader.ReadInt16();
            var pad2Flags = reader.ReadInt32();

            Add(
                new EfaFrame
                {
                    Unknown1 = unknown1,
                    Offset = offset,
                    Size = size,
                    RawSize = rawSize,
                    Unknown2 = unknown2,
                    Unknown3 = unknown3,
                    ByteWidth = byteWidth,
                    Unknown4 = unknown4,
                    ByteCount = byteCount,
                    Unknown5 = unknown5,
                    OriginX = originX,
                    OriginY = originY,
                    OriginFlags = originFlags,
                    ImageWidth = imageWidth,
                    ImageHeight = imageHeight,
                    Pad1Flags = pad1Flags,
                    FrameWidth = frameWidth,
                    FrameHeight = frameHeight,
                    Pad2Flags = pad2Flags,
                    Data = new byte[byteCount]
                });
        }

        using var dataSegment = new StreamSegment(stream, stream.Position, stream.Length - stream.Position);

        for (var i = 0; i < frameCount; i++)
        {
            var frame = this[i];

            DecompressToFrame(dataSegment, frame);
        }
    }

    #region LoadFrom
    public static EfaFile FromArchive(string fileName, DataArchive archive)
    {
        if (!archive.TryGetValue(fileName.WithExtension(".efa"), out var entry))
            throw new FileNotFoundException($"EFA file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }

    public static EfaFile FromEntry(DataArchiveEntry entry)
    {
        using var segment = entry.ToStreamSegment();

        return new EfaFile(segment);
    }

    public static EfaFile FromFile(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".efa"),
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new EfaFile(stream);
    }

    public static EfaFile FromImages(IEnumerable<SKImage> orderedFrames) => FromImages(orderedFrames.ToArray());

    public static EfaFile FromImages(params SKImage[] orderedFrames)
    {
        var efaFile = new EfaFile();

        var imageWidth = orderedFrames.Select(img => img.Width)
                                      .Max();

        var imageHeight = orderedFrames.Select(img => img.Height)
                                       .Max();

        for (var i = 0; i < orderedFrames.Length; i++)
        {
            var image = orderedFrames[i];
            using var bitmap = SKBitmap.FromImage(image);
            byte[] rawBytes;

            if (image.ColorType == SKColorType.Rgb565)
                rawBytes = bitmap.Bytes;
            else
            {
                var writer = new SpanWriter(Encoding.Default, endianness: Endianness.LittleEndian);

                for (var y = 0; y < image.Height; y++)
                    for (var x = 0; x < image.Width; x++)
                    {
                        var color = bitmap.GetPixel(x, y);
                        writer.WriteRgb565Color(color);
                    }

                rawBytes = writer.ToSpan()
                                 .ToArray();
            }

            efaFile.Add(
                new EfaFrame
                {
                    Unknown1 = 3,
                    Offset = -1,
                    Size = -1,
                    RawSize = rawBytes.Length,
                    Unknown2 = 1,
                    Unknown3 = 0,
                    ByteWidth = image.Width * 2,
                    Unknown4 = 4,
                    ByteCount = rawBytes.Length,
                    Unknown5 = 0,
                    OriginX = 0,
                    OriginY = 0,
                    OriginFlags = 0,
                    ImageWidth = (short)imageWidth,
                    ImageHeight = (short)imageHeight,
                    Pad1Flags = 0,
                    FrameWidth = (short)image.Width,
                    FrameHeight = (short)image.Height,
                    Pad2Flags = 0,
                    Data = rawBytes
                });
        }

        return efaFile;
    }

    private static void DecompressToFrame(Stream dataStream, EfaFrame frame)
    {
        using var compressedSegment = new StreamSegment(dataStream, frame.Offset, frame.Size);
        using var decompressor = new ZLibStream(compressedSegment, CompressionMode.Decompress, true);

        Span<byte> decompressed = stackalloc byte[frame.RawSize];

        decompressor.ReadAtLeast(decompressed, frame.RawSize);

        decompressed[..frame.ByteCount]
            .CopyTo(frame.Data);

        /*
        //Not sure what these numbers are
        var reader = new SpanReader(Encoding.Default, decompressed[frame.ByteCount..], Endianness.LittleEndian);

        //not even sure if these should be ushort
        //could they be colors? transparency map?
        //the length of the tail data seems relevant to the size of the image
        var nums = new List<byte>();

        while (!reader.EndOfSpan)
            nums.Add(reader.ReadByte());
        */
    }
    #endregion

    #region SaveTo
    public void Save(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".efa"),
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

        //write file header
        writer.Write(Unknown1);
        writer.Write(Count);
        writer.Write(FrameIntervalMs);
        writer.Write((byte)BlendingType);
        writer.Write(Unknown2);

        var offset = 0;
        var compressedFrames = new byte[Count][];

        for (var i = 0; i < Count; i++)
        {
            var frame = this[i];

            //compress the frame data and store it to be written later
            using var compressed = new MemoryStream();

            using (var compressor = new ZLibStream(compressed, CompressionLevel.Optimal, true))
            {
                compressor.Write(frame.Data, 0, frame.Data.Length);
            }

            var compressedBytes = compressed.ToArray();
            var compressedSize = compressedBytes.Length;
            compressedFrames[i] = compressedBytes;

            //offset starts at 0 and grows with each frame's compressed data size (including the zlib header)
            frame.Offset = offset;
            frame.Size = compressedSize;

            writer.Write(frame.Unknown1);
            writer.Write(frame.Offset);
            writer.Write(frame.Size);
            writer.Write(frame.RawSize);
            writer.Write(frame.Unknown2);
            writer.Write(frame.Unknown3);
            writer.Write(frame.ByteWidth);
            writer.Write(frame.Unknown4);
            writer.Write(frame.ByteCount);
            writer.Write(frame.Unknown5);
            writer.Write(frame.OriginX);
            writer.Write(frame.OriginY);
            writer.Write(frame.OriginFlags);
            writer.Write(frame.ImageWidth);
            writer.Write(frame.ImageHeight);
            writer.Write(frame.Pad1Flags);
            writer.Write(frame.FrameWidth);
            writer.Write(frame.FrameHeight);
            writer.Write(frame.Pad2Flags);

            offset += compressedSize;
        }

        for (var i = 0; i < Count; i++)
        {
            var compressedBytes = compressedFrames[i];

            writer.Write(compressedBytes);
        }
    }
    #endregion
}