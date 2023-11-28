using System.IO;
using System.Text;
using DALib.Definitions;
using DALib.Utility;
using SkiaSharp;

namespace DALib.Extensions;

public static class BinaryReaderExtensions
{
    public static SKColor ReadArgb1555Color(this BinaryReader reader, bool scaleToArgb8888)
    {
        var color = reader.ReadUInt16();

        var a = (color & 0b1) == 0b1; //maybe?
        var r = (byte)((color >> 10) & CONSTANTS.FIVE_BIT_MASK);
        var g = (byte)((color >> 5) & CONSTANTS.FIVE_BIT_MASK);
        var b = (byte)(color & CONSTANTS.FIVE_BIT_MASK);

        if (scaleToArgb8888)
        {
            r = MathEx.ScaleRange<byte, byte>(
                r,
                0,
                CONSTANTS.FIVE_BIT_MASK,
                0,
                byte.MaxValue);

            g = MathEx.ScaleRange<byte, byte>(
                g,
                0,
                CONSTANTS.FIVE_BIT_MASK,
                0,
                byte.MaxValue);

            b = MathEx.ScaleRange<byte, byte>(
                b,
                0,
                CONSTANTS.FIVE_BIT_MASK,
                0,
                byte.MaxValue);
        }

        return new SKColor(
            r,
            g,
            b,
            a ? byte.MaxValue : byte.MinValue);
    }

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

    public static SKColor ReadRgb565Color(this BinaryReader reader, bool scaleToRgb888)
    {
        var color = reader.ReadUInt16();

        var r = (byte)(color >> 11);
        var g = (byte)((color >> 5) & CONSTANTS.SIX_BIT_MASK);
        var b = (byte)(color & CONSTANTS.FIVE_BIT_MASK);

        if (scaleToRgb888)
        {
            r = MathEx.ScaleRange<byte, byte>(
                r,
                0,
                CONSTANTS.FIVE_BIT_MASK,
                0,
                byte.MaxValue);

            g = MathEx.ScaleRange<byte, byte>(
                g,
                0,
                CONSTANTS.SIX_BIT_MASK,
                0,
                byte.MaxValue);

            b = MathEx.ScaleRange<byte, byte>(
                b,
                0,
                CONSTANTS.FIVE_BIT_MASK,
                0,
                byte.MaxValue);
        }

        return new SKColor(r, g, b);
    }

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