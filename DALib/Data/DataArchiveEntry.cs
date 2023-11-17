using System;
using System.IO;
using DALib.Extensions;

namespace DALib.Data;

public sealed class DataArchiveEntry
{
    private readonly DataArchive Archive;

    public int Address { get; }

    public string EntryName { get; }

    public int FileSize { get; }

    public DataArchiveEntry(
        DataArchive archive,
        string entryName,
        int address,
        int fileSize)
    {
        Archive = archive;
        EntryName = entryName;
        Address = address;
        FileSize = fileSize;
    }

    public DataArchiveEntry(DataArchive archive, string entryName, int fileSize)
        : this(
            archive,
            entryName,
            -1,
            fileSize) { }

    public Span<byte> ToSpan()
    {
        Archive.ThrowIfDisposed();

        Archive.DataStream!.Seek(Address, SeekOrigin.Begin);
        var span = new Span<byte>(new byte[FileSize]);
        _ = Archive.DataStream.Read(span);

        return span;
    }

    public Stream ToStreamSegment()
    {
        Archive.ThrowIfDisposed();

        return Archive.DataStream!.Slice(Address, FileSize);
    }
}