using System;
using System.IO;
using DALib.Extensions;

namespace DALib.Data;

public sealed class DataArchiveEntry(
    DataArchive archive,
    string entryName,
    int address,
    int fileSize)
{
    public int Address { get; } = address;

    public string EntryName { get; } = entryName;

    public int FileSize { get; } = fileSize;

    public DataArchiveEntry(DataArchive archive, string entryName, int fileSize)
        : this(
            archive,
            entryName,
            -1,
            fileSize) { }

    public Span<byte> ToSpan()
    {
        archive.ThrowIfDisposed();

        archive.DataStream!.Seek(Address, SeekOrigin.Begin);
        var span = new Span<byte>(new byte[FileSize]);
        _ = archive.DataStream.Read(span);

        return span;
    }

    public Stream ToStreamSegment()
    {
        archive.ThrowIfDisposed();

        return archive.DataStream!.Slice(Address, FileSize);
    }
}