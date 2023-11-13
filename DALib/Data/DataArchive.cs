﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using DALib.Definitions;

namespace DALib.Data;

public sealed class DataArchive : KeyedCollection<string, DataArchiveEntry>, IDisposable
{
    private bool IsDisposed;
    internal Stream? DataStream { get; }

    public DataArchive()
        : base(StringComparer.OrdinalIgnoreCase) { }

    public DataArchive(Stream stream)
    {
        DataStream = stream;

        using var reader = new BinaryReader(stream, Encoding.GetEncoding(949), true);

        var expectedNumberOfEntries = reader.ReadInt32() - 1;

        for (var i = 0; i < expectedNumberOfEntries; ++i)
        {
            var startAddress = reader.ReadInt32();

            var nameBytes = new byte[CONSTANTS.DATA_ARCHIVE_ENTRY_NAME_LENGTH];
            _ = reader.Read(nameBytes, 0, CONSTANTS.DATA_ARCHIVE_ENTRY_NAME_LENGTH);

            var name = Encoding.ASCII.GetString(nameBytes);
            var nullChar = name.IndexOf('\0');

            if (nullChar > -1)
                name = name[..nullChar];

            var endAddress = reader.ReadInt32();

            stream.Seek(-4, SeekOrigin.Current);

            var entry = new DataArchiveEntry(
                this,
                name,
                startAddress,
                endAddress - startAddress);

            Add(entry);
        }
    }

    public IEnumerable<DataArchiveEntry> GetEntriesThatStartWith(string pattern, string format)
    {
        foreach (var entry in this)
        {
            if (!entry.EntryName.EndsWith(format, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!entry.EntryName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                continue;

            yield return entry;
        }
    }
    
    public static DataArchive FromFile(string path) => new(
        File.Open(
            path,
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.RandomAccess,
                Share = FileShare.ReadWrite
            }));

    #region KeyedCollection implementation
    /// <inheritdoc />
    protected override string GetKeyForItem(DataArchiveEntry item) => item.EntryName;
    #endregion

    #region IDisposable implementation
    /// <inheritdoc />
    public void Dispose()
    {
        DataStream?.Dispose();
        IsDisposed = true;
    }

    internal void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(DataArchive));
    }
    #endregion
}