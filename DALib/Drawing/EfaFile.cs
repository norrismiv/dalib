using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using DALib.Data;
using DALib.Extensions;
using DALib.IO;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class EfaFile : Collection<EfaFrame>
{
    public int Unknown1 { get; init; }
    public byte[] Unknown2 { get; init; }

    private EfaFile()
    {
        Unknown1 = 0;
        Unknown2 = new byte[56];
    }

    private EfaFile(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.Default, true);

        Unknown1 = reader.ReadInt32();
        var frameCount = reader.ReadInt32();
        Unknown2 = reader.ReadBytes(56);

        for (var i = 0; i < frameCount; i++)
        {
            var unknown1 = reader.ReadInt32(); //3 on reverse
            var offset = reader.ReadInt32();
            var size = reader.ReadInt32();
            var rawSize = reader.ReadInt32();
            var unknown2 = reader.ReadInt32(); //1 on reverse
            var unknown3 = reader.ReadInt32(); //0 on reverse
            var byteWidth = reader.ReadInt32();
            var unknown4 = reader.ReadInt32(); //4 on reverse
            var byteCount = reader.ReadInt32();
            var unknown5 = reader.ReadInt32(); //0 on reverse
            var originX = reader.ReadInt16();
            var originY = reader.ReadInt16();
            var originFlags = reader.ReadInt32(); //0 on reverse
            var imageWidth = reader.ReadInt16();
            var imageHeight = reader.ReadInt16();
            var pad1Flags = reader.ReadInt32();
            var frameWidth = reader.ReadInt16();
            var frameHeight = reader.ReadInt16();
            var pad2Flags = reader.ReadInt32(); //0 on reverse

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

    public static EfaFile FromImages(params SKImage[] orderedImages)
    {
        var efaFile = new EfaFile();
        var imageWidth = orderedImages.Select(img => img.Width).Max();
        var imageHeight = orderedImages.Select(img => img.Height).Max();

        for (var i = 0; i < orderedImages.Length; i++)
        {
            var image = orderedImages[i];

            //convert the image to Rgb565 if necessary
            if (image.ColorType != SKColorType.Rgb565)
            {
                var oldImage = image;

                using var oldBitmap = SKBitmap.FromImage(image);
                using var newBitmap = new SKBitmap(image.Info.WithColorType(SKColorType.Rgb565));
                oldBitmap.CopyTo(newBitmap, SKColorType.Rgb565);

                image = SKImage.FromBitmap(newBitmap);

                oldImage.Dispose();
            }

            var rawBytes = image.EncodedData.ToArray();

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

        decompressed[..frame.ByteCount].CopyTo(frame.Data);

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
        writer.Write(Unknown2);

        var offset = 0;
        var compressedData = new byte[Count][];

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
            compressedData[i] = compressedBytes;

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
            var compressedBytes = compressedData[i];

            writer.Write(compressedBytes);
        }
    }
    #endregion
}