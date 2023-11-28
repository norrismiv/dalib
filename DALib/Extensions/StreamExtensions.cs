using System;
using System.IO;
using DALib.IO;

namespace DALib.Extensions;

public static class StreamExtensions
{
    public static Stream Slice(this Stream stream, long offset, long length) => new StreamSegment(stream, offset, length);

    public static byte[] ToArray(this Stream stream)
    {
        if (stream is MemoryStream memoryStream)
            return memoryStream.ToArray();

        using var memory = new MemoryStream();
        stream.CopyTo(memory);

        return memory.ToArray();
    }

    public static Span<byte> ToSpan(this Stream stream)
    {
        var buffer = new Span<byte>(new byte[stream.Length - stream.Position]);
        stream.ReadExactly(buffer);

        return buffer;
    }
}