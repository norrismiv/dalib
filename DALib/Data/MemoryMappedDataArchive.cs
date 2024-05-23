using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using DALib.Abstractions;

namespace DALib.Data;

/// <summary>
///     A <see cref="DataArchive" /> that utilizes Memory Mapped Files, so that the entire archvive is not loaded into
///     memory.
/// </summary>
public sealed class MemoryMappedDataArchive : DataArchive
{
    private readonly MemoryMappedFile MappedFile;

    internal MemoryMappedDataArchive(MemoryMappedFile mappedFile, bool newFormat = false)
        : base(mappedFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read), newFormat)
        => MappedFile = mappedFile;

    /// <inheritdoc />
    public override void Dispose()
    {
        MappedFile.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    public override Stream GetEntryStream(DataArchiveEntry entry)
        => MappedFile.CreateViewStream(entry.Address, entry.FileSize, MemoryMappedFileAccess.Read);

    /// <inheritdoc />
    public override void Patch(string entryName, ISavable item) => throw new NotSupportedException("Cannot patch a memory-mapped archive.");

    /// <inheritdoc />
    public override void Save(string path) => throw new NotSupportedException("Cannot save a memory-mapped archive.");

    /// <inheritdoc />
    public override void Save(Stream stream) => throw new NotSupportedException("Cannot save a memory-mapped archive.");
}