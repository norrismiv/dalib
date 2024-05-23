using System;
using System.IO;
using DALib.Extensions;

namespace DALib.Data;

/// <summary>
///     Represents an entry in a data archive.
/// </summary>
public class DataArchiveEntry(
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
    ///     Converts the entry to a span of bytes
    /// </summary>
    public Span<byte> ToSpan()
    {
        archive.ThrowIfDisposed();

        using var segment = ToStreamSegment();

        return segment.ToSpan();
    }

    /// <summary>
    ///     Returns a segment of the underlying stream that represents the portion of data for this entry
    /// </summary>
    public Stream ToStreamSegment()
    {
        archive.ThrowIfDisposed();

        return archive.GetEntryStream(this);
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