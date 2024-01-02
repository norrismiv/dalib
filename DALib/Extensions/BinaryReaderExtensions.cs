using System.IO;
using System.Text;
using DALib.Utility;
using SkiaSharp;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for BinaryReader.
/// </summary>
public static class BinaryReaderExtensions
{
    /// <summary>
    ///     Reads a 16-bit signed short from the current stream and advances the position of the stream by two bytes
    /// </summary>
    /// <param name="reader">The BinaryReader from which to read the short.</param>
    /// <param name="bigEndian">A boolean flag indicating whether the short should be read in big-endian format.</param>
    /// <returns>A 16-bit signed integer read from the stream.</returns>
    public static short ReadInt16(this BinaryReader reader, bool bigEndian)
    {
        if (!bigEndian)
            return reader.ReadInt16();

        var buffer = reader.ReadBytes(2);

        return (short)(buffer[1] | (buffer[0] << 8));
    }

    /// <summary>
    ///     Reads a 32-bit signed integer from the current stream and advances the position by four bytes.
    /// </summary>
    /// <param name="reader">The BinaryReader to read from.</param>
    /// <param name="bigEndian">A boolean flag indicating whether the integer should be read in big-endian format.</param>
    /// <returns>
    ///     The 32-bit signed integer read from the current stream
    /// </returns>
    public static int ReadInt32(this BinaryReader reader, bool bigEndian)
    {
        if (!bigEndian)
            return reader.ReadInt32();

        var buffer = reader.ReadBytes(4);

        return buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24);
    }

    /// <summary>
    ///     Reads a 16-bit color value encoded as RGB555 from the specified BinaryReader and scales it to RGB888
    /// </summary>
    /// <param name="reader">The BinaryReader to read from.</param>
    /// <remarks>
    ///     The color is read as an RGB555 encoded color, then scaled to RGB888 and stored as an SKColor
    /// </remarks>
    public static SKColor ReadRgb555Color(this BinaryReader reader) => ColorCodec.DecodeRgb555(reader.ReadUInt16());

    /// <summary>
    ///     Reads a 16-bit color value encoded as RGB565 from the specified BinaryReader and scales it to RGB888
    /// </summary>
    /// <param name="reader">The BinaryReader to read from.</param>
    /// <remarks>
    ///     The color is read as an RGB565 encoded color, then scaled to RGB888 and stored as an SKColor
    /// </remarks>
    public static SKColor ReadRgb565Color(this BinaryReader reader) => ColorCodec.DecodeRgb565(reader.ReadUInt16());

    /// <summary>
    ///     Reads a string from the BinaryReader
    /// </summary>
    /// <param name="reader">The BinaryReader to read from.</param>
    /// <param name="encoding">The encoding used to decode the string.</param>
    /// <param name="bigEndian">A boolean flag indicating whether the length prefix should be read in big-endian format.</param>
    public static string ReadString16(this BinaryReader reader, Encoding encoding, bool bigEndian)
    {
        var length = reader.ReadUInt16(bigEndian);
        var buffer = reader.ReadBytes(length);

        return encoding.GetString(buffer);
    }

    /// <summary>
    ///     Reads a string from the BinaryReader
    /// </summary>
    /// <param name="reader">The BinaryReader to read from.</param>
    /// <param name="encoding">The encoding used to decode the string.</param>
    public static string ReadString8(this BinaryReader reader, Encoding encoding)
    {
        var length = reader.ReadByte();
        var buffer = reader.ReadBytes(length);

        return encoding.GetString(buffer);
    }

    /// <summary>
    ///     Reads a 16-bit unsigned short from the current stream and advances the position of the stream by two bytes
    /// </summary>
    /// <param name="reader">The BinaryReader from which to read the short.</param>
    /// <param name="bigEndian">A boolean flag indicating whether the short should be read in big-endian format.</param>
    /// <returns>A 16-bit unsigned integer read from the stream.</returns>
    public static ushort ReadUInt16(this BinaryReader reader, bool bigEndian)
    {
        if (!bigEndian)
            return reader.ReadUInt16();

        var buffer = reader.ReadBytes(2);

        return (ushort)(buffer[1] | (buffer[0] << 8));
    }

    /// <summary>
    ///     Reads a 32-bit unsigned integer from the current stream and advances the position by four bytes.
    /// </summary>
    /// <param name="reader">The BinaryReader to read from.</param>
    /// <param name="bigEndian">A boolean flag indicating whether the integer should be read in big-endian format.</param>
    /// <returns>
    ///     The 32-bit unsigned integer read from the current stream
    /// </returns>
    public static uint ReadUInt32(this BinaryReader reader, bool bigEndian)
    {
        if (!bigEndian)
            return reader.ReadUInt32();

        var buffer = reader.ReadBytes(4);

        return (uint)(buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24));
    }
}