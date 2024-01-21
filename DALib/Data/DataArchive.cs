using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using DALib.Abstractions;
using DALib.Definitions;
using DALib.Extensions;

namespace DALib.Data;

/// <summary>
///     Represents a DarkAges data archive that can be used for storing and manipulating data entries.
/// </summary>
public sealed class DataArchive() : KeyedCollection<string, DataArchiveEntry>(StringComparer.OrdinalIgnoreCase), ISavable, IDisposable
{
    private bool IsDisposed;
    internal Stream? DataStream { get; set; }

    private DataArchive(Stream stream, bool newFormat = false)
        : this()
    {
        DataStream = stream;

        using var reader = new BinaryReader(stream, Encoding.Default, true);

        var expectedNumberOfEntries = reader.ReadInt32() - 1;

        for (var i = 0; i < expectedNumberOfEntries; ++i)
        {
            var startAddress = reader.ReadInt32();

            var nameLength = newFormat ? 12 : CONSTANTS.DATA_ARCHIVE_ENTRY_NAME_LENGTH;

            var nameBytes = new byte[nameLength];
            _ = reader.Read(nameBytes, 0, nameLength);

            var name = Encoding.ASCII.GetString(nameBytes);
            var nullChar = name.IndexOf('\0');

            if (nullChar > -1)
                name = name[..nullChar];

            if (newFormat)
                _ = reader.ReadBytes(20); //idk

            var endAddress = reader.ReadInt32();

            stream.Seek(-4, SeekOrigin.Current);

            Add(
                new DataArchiveEntry(
                    this,
                    name,
                    startAddress,
                    endAddress - startAddress));
        }
    }

    /// <summary>
    ///     Returns a collection of data archive entries that match the given pattern and extension.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern to match the entry name against.
    /// </param>
    /// <param name="extension">
    ///     The extension to match the entry name against.
    /// </param>
    /// <returns>
    ///     A collection of <see cref="DataArchiveEntry" /> objects that match the pattern and extension.
    /// </returns>
    public IEnumerable<DataArchiveEntry> GetEntries(string pattern, string extension)
    {
        foreach (var entry in this)
        {
            if (!entry.EntryName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!entry.EntryName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                continue;

            yield return entry;
        }
    }

    /// <summary>
    ///     Retrieves all entries with a specified extension.
    /// </summary>
    /// <param name="extension">
    ///     The extension to filter the entries with.
    /// </param>
    /// <returns>
    ///     An <see cref="IEnumerable{T}" /> of <see cref="DataArchiveEntry" /> containing all entries with the specified
    ///     extension.
    /// </returns>
    public IEnumerable<DataArchiveEntry> GetEntries(string extension)
    {
        foreach (var entry in this)
        {
            if (!entry.EntryName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                continue;

            yield return entry;
        }
    }

    #region KeyedCollection implementation
    /// <inheritdoc />
    protected override string GetKeyForItem(DataArchiveEntry item) => item.EntryName;
    #endregion

    /// <summary>
    ///     Patches the DataArchive by appending a new entry that replaces an existing entry with the same name, or adds a new
    ///     entry if no existing entry has the same name.
    /// </summary>
    /// <param name="entryName">
    ///     The name of the entry to be patched.
    /// </param>
    /// <param name="item">
    ///     The item to be patched.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the DataArchive is not in memory.
    /// </exception>
    public void Patch(string entryName, ISavable item)
    {
        ThrowIfDisposed();

        if (DataStream is not MemoryStream)
            throw new InvalidOperationException("DataArchive must be in memory to patch (use cacheArchive=true)");

        //if an entry exists with the same name, remove it
        Remove(entryName);

        using var buffer = new MemoryStream();
        item.Save(buffer);

        //create a new entry (this entry will be appended to the end of the archive)
        var entry = new DataArchiveEntry(
            this,
            entryName,
            (int)DataStream!.Length,
            (int)buffer.Length);

        //seek to the end of the archive and append the new entry
        DataStream!.Seek(0, SeekOrigin.End);
        buffer.Seek(0, SeekOrigin.Begin);

        buffer.CopyTo(DataStream);

        //add entry to archive
        Add(entry);
    }

    #region SaveTo
    /// <inheritdoc />
    /// <exception cref="ObjectDisposedException">
    ///     Thrown if the object is already disposed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the path is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown if the path is empty or contains invalid characters.
    /// </exception>
    /// <exception cref="PathTooLongException">
    ///     Thrown if the path exceeds the maximum length allowed.
    /// </exception>
    public void Save(string path)
    {
        ThrowIfDisposed();

        using var stream = File.Open(
            path.WithExtension(".dat"),
            new FileStreamOptions
            {
                Access = FileAccess.Write,
                Mode = FileMode.Create,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite,
                BufferSize = 81920
            });

        Save(stream);
    }

    /// <inheritdoc />
    /// <exception cref="ObjectDisposedException">
    ///     Thrown if the object has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if an entry name is too long (must be 13 characters or less).
    /// </exception>
    public void Save(Stream stream)
    {
        ThrowIfDisposed();

        const int HEADER_LENGTH = 4;
        const int ENTRY_HEADER_LENGTH = 4 + CONSTANTS.DATA_ARCHIVE_ENTRY_NAME_LENGTH;
        using var writer = new BinaryWriter(stream, Encoding.Default, true);

        writer.Write(Count + 1);

        //add the file header length
        //plus the entry header length * number of entries
        //plus 4 bytes for the final entry's end address (which could also be considered the total number of bytes)
        var address = HEADER_LENGTH + Count * ENTRY_HEADER_LENGTH + 4;

        var orderedEntries = this.OrderBy(entry => entry.EntryName)
                                 .ToList();

        foreach (var entry in orderedEntries)
        {
            //reconstruct the name field with the required terminator
            var nameStr = entry.EntryName;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (nameStr.Length > CONSTANTS.DATA_ARCHIVE_ENTRY_NAME_LENGTH)
                throw new InvalidOperationException("Entry name is too long, must be 13 characters or less");

            if (nameStr.Length < CONSTANTS.DATA_ARCHIVE_ENTRY_NAME_LENGTH)
                nameStr = nameStr.PadRight(CONSTANTS.DATA_ARCHIVE_ENTRY_NAME_LENGTH, '\0');

            //get bytes for the name field (binaryWriter.Write(string) doesn't work for this)
            var nameStrBytes = Encoding.ASCII.GetBytes(nameStr);

            writer.Write(address);
            writer.Write(nameStrBytes);

            address += entry.FileSize;
        }

        writer.Write(address);

        foreach (var entry in orderedEntries)
        {
            using var segment = entry.ToStreamSegment();
            segment.CopyTo(stream);
        }
    }

    /// <summary>
    ///     Extracts the contents of the current archive to the specified directory.
    /// </summary>
    /// <param name="dir">
    ///     The directory to which the contents will be extracted.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the archive is already disposed.
    /// </exception>
    public void ExtractTo(string dir)
    {
        ThrowIfDisposed();

        foreach (var entry in this)
        {
            var path = Path.Combine(dir, entry.EntryName);

            using var stream = File.Create(path);
            using var segment = entry.ToStreamSegment();

            segment.CopyTo(stream);
        }
    }
    #endregion

    #region LoadFrom
    /// <summary>
    ///     Loads a DataArchive from a directory that contains already extracted entries
    /// </summary>
    /// <param name="dir">
    ///     The directory path.
    /// </param>
    /// <returns>
    ///     A new DataArchive object.
    /// </returns>
    public static DataArchive FromDirectory(string dir)
    {
        //create a new in-memory archive
        var archive = new DataArchive();
        archive.DataStream = new MemoryStream();

        var address = 0;

        //enumerate the directory, copying each file into the memory archive
        //maintain accurate address offsets for each file so that the archive is useable
        foreach (var file in Directory.EnumerateFiles(dir))
        {
            using var stream = File.Open(
                file,
                new FileStreamOptions
                {
                    Access = FileAccess.Read,
                    Mode = FileMode.Open,
                    Options = FileOptions.SequentialScan,
                    Share = FileShare.ReadWrite,
                    BufferSize = 8192
                });

            stream.CopyTo(archive.DataStream);

            var entryName = Path.GetFileName(file);
            var length = (int)stream.Length;

            var entry = new DataArchiveEntry(
                archive,
                entryName,
                address,
                length);

            address += length;

            archive.Add(entry);
        }

        return archive;
    }

    /// <summary>
    ///     Loads a DataArchive from the specified path
    /// </summary>
    /// <param name="path">
    ///     The path to the file to create the DataArchive from.
    /// </param>
    /// <param name="cacheArchive">
    ///     Indicates whether to cache the archive in memory or read from file using pointers. Default is false.
    /// </param>
    /// <param name="newformat">
    ///     Indicates whether to use the new format when reading the archive. Default is false.
    /// </param>
    /// <returns>
    ///     A new instance of the DataArchive class.
    /// </returns>
    public static DataArchive FromFile(string path, bool cacheArchive = false, bool newformat = false)
    {
        //if we don't want to cache the archive
        //return an archive that reads using pointers from an open file handle
        if (!cacheArchive)
            return new DataArchive(
                File.Open(
                    path.WithExtension(".dat"),
                    new FileStreamOptions
                    {
                        Access = FileAccess.Read,
                        Mode = FileMode.Open,
                        Options = FileOptions.RandomAccess,
                        Share = FileShare.ReadWrite,
                        BufferSize = 8192
                    }),
                newformat);

        //if we do want to cache the archive
        //copy the whole file into a memorystream and use that
        //pointers will still be used, but the data will be cached in memory
        using var stream = File.Open(
            path.WithExtension(".dat"),
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite,
                BufferSize = 81920
            });

        var memory = new MemoryStream((int)stream.Length);
        stream.CopyTo(memory);

        memory.Seek(0, SeekOrigin.Begin);

        return new DataArchive(memory, newformat);
    }
    #endregion

    #region IDisposable implementation
    /// <inheritdoc />
    public void Dispose()
    {
        DataStream?.Dispose();
        IsDisposed = true;
    }

    internal void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(IsDisposed, this);
    #endregion
}