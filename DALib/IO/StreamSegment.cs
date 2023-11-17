using System;
using System.IO;

namespace DALib.IO;

public class StreamSegment : Stream
{
    private readonly long BaseOffset;
    private readonly bool LeaveOpen;

    /// <inheritdoc />
    public override long Position { get; set; }

    public Stream BaseStream { get; }

    /// <inheritdoc />
    public override long Length { get; }

    /// <inheritdoc />
    public override bool CanRead => BaseStream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => BaseStream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => BaseStream.CanWrite;

    private long OffsetPosition => BaseOffset + Position;

    public StreamSegment(
        Stream baseStream,
        long offset,
        long segmentLength,
        bool leaveOpen = true)
    {
        if ((offset + segmentLength) > baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(segmentLength), segmentLength, null);

        BaseStream = baseStream;
        BaseOffset = offset;
        Length = segmentLength;
        LeaveOpen = leaveOpen;
    }

    public new void Dispose()
    {
        if (!LeaveOpen)
            BaseStream.Dispose();

        base.Dispose();
    }

    /// <inheritdoc />
    public override void Flush() { BaseStream.Flush(); }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        if ((Position + count) > Length)
            count = (int)(Length - Position);

        BaseStream.Seek(OffsetPosition, SeekOrigin.Begin);
        var ret = BaseStream.Read(buffer, offset, count);

        SetPositionFromBaseStream();

        return ret;
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        return origin switch
        {
            SeekOrigin.Begin   => Position = offset,
            SeekOrigin.Current => Position += offset,
            SeekOrigin.End     => Position = Length - offset,
            _                  => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };
    }

    /// <inheritdoc />
    public override void SetLength(long value) { throw new NotImplementedException(); }

    private void SetPositionFromBaseStream() { Position = BaseStream.Position - BaseOffset; }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        BaseStream.Seek(OffsetPosition, SeekOrigin.Begin);
        BaseStream.Write(buffer, offset, count);

        SetPositionFromBaseStream();
    }
}