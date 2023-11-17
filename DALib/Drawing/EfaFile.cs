using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using DALib.Data;
using DALib.Extensions;
using DALib.IO;

namespace DALib.Drawing;

public sealed class EfaFile : Collection<EfaFrame>
{
    public int Unknown1 { get; init; }
    public byte[] Unknown2 { get; init; }

    public EfaFile(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.Default, true);

        Unknown1 = reader.ReadInt32();
        var frameCount = reader.ReadInt32();
        Unknown2 = reader.ReadBytes(56);

        for (var i = 0; i < frameCount; i++)
        {
            var unknown1 = reader.ReadInt32();
            var offset = reader.ReadInt32();
            var size = reader.ReadInt32();
            var rawSize = reader.ReadInt32();
            var unknown2 = reader.ReadInt32();
            var unknown3 = reader.ReadInt32();
            var width = reader.ReadInt32();
            var unknown4 = reader.ReadInt32();
            var byteCount = reader.ReadInt32();
            var unknown5 = reader.ReadInt32();
            var originX = reader.ReadInt16();
            var originY = reader.ReadInt16();
            var originFlags = reader.ReadInt32();
            var pad1X = reader.ReadInt16();
            var pad1Y = reader.ReadInt16();
            var pad1Flags = reader.ReadInt32();
            var pad2X = reader.ReadInt16();
            var pad2Y = reader.ReadInt16();
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
                    ByteWidth = width,
                    Unknown4 = unknown4,
                    ByteCount = byteCount,
                    Unknown5 = unknown5,
                    OriginX = originX,
                    OriginY = originY,
                    OriginFlags = originFlags,
                    Pad1X = pad1X,
                    Pad1Y = pad1Y,
                    Pad1Flags = pad1Flags,
                    Pad2X = pad2X,
                    Pad2Y = pad2Y,
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

    private static void DecompressToFrame(Stream dataStream, EfaFrame frame)
    {
        using var compressedSegment = new StreamSegment(dataStream, frame.Offset + 2, frame.Size - 2);
        using var decompressor = new DeflateStream(compressedSegment, CompressionMode.Decompress);

        Span<byte> decompressed = stackalloc byte[frame.RawSize];

        decompressor.ReadAtLeast(decompressed, frame.RawSize);

        decompressed[..frame.ByteCount].CopyTo(frame.Data);

        /* Not sure what these numbers are
        var reader = new SpanReader(Encoding.Default, decompressed[frame.ByteCount..], Endianness.LittleEndian);

        //not even sure if these should be ushort
        //could they be colors? transparency map?
        //the length of the tail data seems relevant to the size of the image
        var nums = new List<ushort>();

        while (!reader.EndOfSpan)
            nums.Add(reader.ReadUInt16());
        */
    }

    public static EfaFile FromArchive(string fileName, DataArchive archive)
    {
        if (!archive.TryGetValue(fileName.WithExtension(".efa"), out var entry))
            throw new FileNotFoundException($"EFA file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }

    public static EfaFile FromEntry(DataArchiveEntry entry) => new(entry.ToStreamSegment());

    public static EfaFile FromFile(string path)
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

        return new EfaFile(stream);
    }
}