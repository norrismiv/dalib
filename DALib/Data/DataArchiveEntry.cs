using System;
using System.IO;
using DALib.Extensions;

namespace DALib.Data;

/// <summary>
///     Represents an entry in a data archive.
/// </summary>
public sealed class DataArchiveEntry(
    DataArchive archive,
    string entryName,
    int address,
    int fileSize)
{
    /// <summary>
    ///     The starting address of the entry within it's containing archive
    /// </summary>
    public int Address { get; } = address;

    /// <summary>
    ///     The name of the entry. Will contain file extension if present
    /// </summary>
    public string EntryName { get; } = entryName;

    /// <summary>
    ///     The size of the entry in bytes.
    /// </summary>
    public int FileSize { get; } = fileSize;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataArchiveEntry" /> class with it's containing archive, entry name,
    ///     and file size.
    /// </summary>
    /// <param name="archive">
    ///     The archive that contains the entry.
    /// </param>
    /// <param name="entryName">
    ///     The name of the entry.
    /// </param>
    /// <param name="fileSize">
    ///     The size of the file.
    /// </param>
    public DataArchiveEntry(DataArchive archive, string entryName, int fileSize)
        : this(
            archive,
            entryName,
            -1,
            fileSize) { }

    /// <summary>
    ///     Returns a dedicated stream for the portion of data for this entry. Dedicated streams can be used concurrently.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If the archive's stream is of an unexpected type
    /// </exception>
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

    /// <summary>
    ///     Converts the entry to a span of bytes
    /// </summary>
    public Span<byte> ToSpan()
    {
        archive.ThrowIfDisposed();

        archive.DataStream!.Seek(Address, SeekOrigin.Begin);
        var span = new Span<byte>(new byte[FileSize]);
        _ = archive.DataStream.Read(span);

        return span;
    }

    /// <summary>
    ///     Returns a segment of the underlying stream that represents the portion of data for this entry. Segments of an
    ///     underlying stream should not be used concurrently.
    /// </summary>
    /// <param name="leaveOpen">
    ///     Whether or not to leave the underlying stream open when this segment is disposed
    /// </param>
    public Stream ToStreamSegment(bool leaveOpen = true)
    {
        archive.ThrowIfDisposed();

        return archive.DataStream!.Slice(Address, FileSize, leaveOpen);
    }

    /// <summary>
    ///     Attempts to retrieve a numeric identifier from the EntryName property.
    /// </summary>
    /// <param name="identifier">
    ///     The retrieved numeric identifier, if successful.
    /// </param>
    /// <param name="numDigits">
    ///     The maximum number of digits to consider as the identifier.
    /// </param>
    /// <returns>
    ///     True if a numeric identifier is successfully retrieved, false otherwise.
    /// </returns>
    public bool TryGetNumericIdentifier(out int identifier, int numDigits = int.MaxValue)
    {
        identifier = -1;

        var fileName = Path.GetFileNameWithoutExtension(EntryName);
        var numberStartIndex = -1;
        var numberEndIndex = -1;

        for (var i = 0; i < fileName.Length; ++i)
            if (char.IsDigit(fileName[i]))
            {
                if (numberStartIndex == -1)
                    numberStartIndex = i;

                numberEndIndex = i;
            }

        if (numberStartIndex == -1)
            return false;

        numberEndIndex++;

        if ((numberEndIndex - numberStartIndex) > numDigits)
            numberEndIndex = numberStartIndex + numDigits;

        var numericIdentifierStr = fileName[numberStartIndex..numberEndIndex];

        return int.TryParse(numericIdentifierStr, out identifier);
    }
}