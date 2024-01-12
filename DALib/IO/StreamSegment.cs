using System;
using System.IO;

namespace DALib.IO;

/// <summary>
///     Represents a segment of a stream, allowing operations only within the specified segment.
/// </summary>
public class StreamSegment : Stream
{
    /// <summary>
    ///     The base offset of the segment within the original stream
    /// </summary>
    protected long BaseOffset { get; set; }

    /// <summary>
    ///     Whether to leave the base stream open when disposing the segment
    /// </summary>
    protected bool LeaveOpen { get; set; }

    /// <summary>
    ///     The current position within the segment
    /// </summary>
    public override long Position { get; set; }

    /// <summary>
    ///     The base stream this segment wraps
    /// </summary>
    public Stream BaseStream { get; }

    /// <summary>
    ///     The length of the segment
    /// </summary>
    public override long Length { get; }

    /// <inheritdoc />
    public override bool CanRead => BaseStream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => BaseStream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => BaseStream.CanWrite;

    /// <summary>
    ///     The current position within the base stream
    /// </summary>
    protected virtual long OffsetPosition => BaseOffset + Position;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamSegment" /> class.
    /// </summary>
    /// <param name="baseStream">The base stream.</param>
    /// <param name="offset">The offset within the base stream where the segment starts.</param>
    /// <param name="segmentLength">The length of the segment.</param>
    /// <param name="leaveOpen">Whether to leave the base stream open when disposing the segment.</param>
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

    /// <inheritdoc cref="IDisposable.Dispose" />
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

        if (BaseStream.Position != OffsetPosition)
            BaseStream.Seek(OffsetPosition, SeekOrigin.Begin);

        var ret = BaseStream.Read(buffer, offset, count);

        SetPositionFromBaseStream();

        return ret;
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        if ((offset > Length) || ((origin == SeekOrigin.Begin) && (offset < 0)))
            throw new ArgumentOutOfRangeException(nameof(offset), offset, null);

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

    /// <summary>
    ///     Updates the current position of the segment based on the base stream's position.
    /// </summary>
    protected virtual void SetPositionFromBaseStream() { Position = BaseStream.Position - BaseOffset; }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (BaseStream.Position != OffsetPosition)
            BaseStream.Seek(OffsetPosition, SeekOrigin.Begin);

        BaseStream.Write(buffer, offset, count);

        SetPositionFromBaseStream();
    }
}