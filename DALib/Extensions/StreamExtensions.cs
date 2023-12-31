using System;
using System.IO;
using DALib.IO;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for Streams
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    ///     Returns a slice of the original stream, starting from the specified offset and with the specified length.
    /// </summary>
    /// <param name="stream">The original stream.</param>
    /// <param name="offset">The starting position of the slice.</param>
    /// <param name="length">The length of the slice.</param>
    /// <param name="leaveOpen">
    ///     <c>true</c> to leave the original stream open when the slice is disposed;
    ///     <c>false</c> to close it.
    ///     Default is <c>true</c>.
    /// </param>
    /// <remarks>
    ///     This creates a <see cref="StreamSegment" /> that wraps the original stream. This segment can not be used
    ///     concurrently with the original stream, or with other segments of the same stream.
    /// </remarks>
    public static Stream Slice(
        this Stream stream,
        long offset,
        long length,
        bool leaveOpen = true)
        => new StreamSegment(
            stream,
            offset,
            length,
            leaveOpen);

    /// <summary>
    ///     Reads the remaining data from the stream and returns it as a byte array
    /// </summary>
    /// <param name="stream">The stream to convert.</param>
    public static byte[] ToArray(this Stream stream)
    {
        if (stream is MemoryStream memoryStream)
            return memoryStream.ToArray();

        var buffer = new byte[stream.Length - stream.Position];
        stream.ReadExactly(buffer);

        return buffer;
    }

    /// <summary>
    ///     Reads the remaining data from the stream and returns it as a span of bytes
    /// </summary>
    /// <param name="stream">The stream to convert.</param>
    public static Span<byte> ToSpan(this Stream stream)
    {
        var buffer = new Span<byte>(new byte[stream.Length - stream.Position]);
        stream.ReadExactly(buffer);

        return buffer;
    }
}