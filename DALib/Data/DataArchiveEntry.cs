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

    /// <summary>
    ///     Returns a dedicated stream for the portion of data for this entry.
    ///     Dedicated streams can be used concurrently.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If the archive's stream is of an unexpected type</exception>
    public Stream ToDedicatedStream()
    {
        archive.ThrowIfDisposed();

        switch (archive.DataStream)
        {
            case FileStream fileStream:
            {
                var dedicatedFileStream = new FileStream(
                    fileStream.Name,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);

                //if this segment is closed, close the underlying filestream (leaveOpen: false)
                return dedicatedFileStream.Slice(Address, FileSize, false);
            }
            case MemoryStream:
            {
                //apparently there's no good way to copy memoryStreams to eachother with offset/length
                //so the best way to do it is to copy the segment to a new MemoryStream
                using var segment = ToStreamSegment();
                var dedicatedMemoryStream = new MemoryStream(FileSize);

                segment.CopyTo(dedicatedMemoryStream);

                dedicatedMemoryStream.Seek(0, SeekOrigin.Begin);

                return dedicatedMemoryStream;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(archive.DataStream), "Encountered unexpected stream type");
        }
    }

    public Span<byte> ToSpan()
    {
        archive.ThrowIfDisposed();

        archive.DataStream!.Seek(Address, SeekOrigin.Begin);
        var span = new Span<byte>(new byte[FileSize]);
        _ = archive.DataStream.Read(span);

        return span;
    }

    /// <summary>
    ///     Returns a segment of the underlying stream that represents the portion of data for this entry.
    ///     Segments of an underlying stream should not be used concurrently.
    /// </summary>
    /// <param name="leaveOpen">Whether or not to leave the underlying stream open when this segment is disposed</param>
    public Stream ToStreamSegment(bool leaveOpen = true)
    {
        archive.ThrowIfDisposed();

        return archive.DataStream!.Slice(Address, FileSize, leaveOpen);
    }

    public bool TryGetNumericIdentifier(out int identifier)
    {
        identifier = -1;

        var fileName = Path.GetFileNameWithoutExtension(EntryName);
        var indexOfFirstNumber = -1;

        for (var i = 0; i < fileName.Length; ++i)
            if (char.IsDigit(fileName[i]))
            {
                indexOfFirstNumber = i;

                break;
            }

        if (indexOfFirstNumber == -1)
            return false;

        var numericIdentifierStr = fileName[indexOfFirstNumber..];

        return int.TryParse(numericIdentifierStr, out identifier);
    }
}