using System;
using System.IO;
using DALib.Abstractions;

namespace DALib.Data;

/// <summary>
///     Represents an entry that can be used to patch a DataArchive without knowing it's underlying type
/// </summary>
public class PatchEntry : ISavable, IDisposable
{
    private readonly Stream SourceStream;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PatchEntry" /> class
    /// </summary>
    /// <param name="stream">
    ///     The stream containing the data of this entry
    /// </param>
    public PatchEntry(Stream stream) => SourceStream = stream;

    /// <inheritdoc />
    public void Dispose() => SourceStream.Dispose();

    /// <inheritdoc />
    public void Save(string path)
    {
        using var stream = File.Open(
            path,
            new FileStreamOptions
            {
                Access = FileAccess.Write,
                Mode = FileMode.Create,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        Save(stream);
    }

    /// <inheritdoc />
    public void Save(Stream stream)
    {
        SourceStream.Seek(0, SeekOrigin.Begin);
        SourceStream.CopyTo(stream);
        SourceStream.Seek(0, SeekOrigin.Begin);
    }
}