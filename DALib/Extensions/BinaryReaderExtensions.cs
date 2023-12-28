using System.IO;
using System.Text;
using SkiaSharp;

namespace DALib.Extensions;

public static class BinaryReaderExtensions
{
    public static short ReadInt16(this BinaryReader reader, bool bigEndian)
    {
        if (!bigEndian)
            return reader.ReadInt16();

        var buffer = reader.ReadBytes(2);

        return (short)(buffer[1] | (buffer[0] << 8));
    }

    public static int ReadInt32(this BinaryReader reader, bool bigEndian)
    {
        if (!bigEndian)
            return reader.ReadInt32();

        var buffer = reader.ReadBytes(4);

        return buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24);
    }

    public static SKColor ReadRgb555Color(this BinaryReader reader)
        => reader.ReadUInt16()
                 .ToRgb555Color();

    public static SKColor ReadRgb565Color(this BinaryReader reader)
        => reader.ReadUInt16()
                 .ToRgb565Color();

    public static string ReadString16(this BinaryReader reader, Encoding encoding, bool bigEndian)
    {
        var length = reader.ReadUInt16(bigEndian);
        var buffer = reader.ReadBytes(length);

        return encoding.GetString(buffer);
    }

    public static string ReadString8(this BinaryReader reader, Encoding encoding)
    {
        var length = reader.ReadByte();
        var buffer = reader.ReadBytes(length);

        return encoding.GetString(buffer);
    }

    public static ushort ReadUInt16(this BinaryReader reader, bool bigEndian)
    {
        if (!bigEndian)
            return reader.ReadUInt16();

        var buffer = reader.ReadBytes(2);

        return (ushort)(buffer[1] | (buffer[0] << 8));
    }

    public static uint ReadUInt32(this BinaryReader reader, bool bigEndian)
    {
        if (!bigEndian)
            return reader.ReadUInt32();

        var buffer = reader.ReadBytes(4);

        return (uint)(buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24));
    }
}