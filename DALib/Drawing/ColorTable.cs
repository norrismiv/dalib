using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using DALib.Data;
using DALib.Definitions;
using DALib.Memory;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class ColorTable : KeyedCollection<int, ColorTableEntry>
{
    public ColorTable(Stream stream)
    {
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            true,
            1024,
            true);

        if (!int.TryParse(reader.ReadLine(), out var colorsPerEntry))
            return;

        while (!reader.EndOfStream && byte.TryParse(reader.ReadLine(), out var colorIndex))
        {
            var colors = new SKColor[colorsPerEntry];

            for (var i = 0; (i < colorsPerEntry) && !reader.EndOfStream; ++i)
            {
                var line = reader.ReadLine();

                if (!string.IsNullOrEmpty(line))
                {
                    var values = line.Split(',');

                    if ((values.Length != 3)
                        || !int.TryParse(values[0], out var r)
                        || !int.TryParse(values[1], out var g)
                        || !int.TryParse(values[2], out var b))
                        return;

                    colors[i] = new SKColor((byte)(r % 256), (byte)(g % 256), (byte)(b % 256));
                } else
                    colors[i] = new SKColor();
            }

            Add(new ColorTableEntry(colorIndex, colors));
        }
    }

    public ColorTable(Span<byte> buffer)
    {
        var reader = new SpanReader(Encoding.UTF8, buffer, Endianness.LittleEndian);

        if (!int.TryParse(reader.ReadString(), out var colorsPerEntry))
            return;

        while (!reader.EndOfSpan && byte.TryParse(reader.ReadString(), out var colorIndex))
        {
            var colors = new SKColor[colorsPerEntry];

            for (var i = 0; (i < colorsPerEntry) && !reader.EndOfSpan; ++i)
            {
                var line = reader.ReadString();

                if (!string.IsNullOrEmpty(line))
                {
                    var values = line.Split(',');

                    if ((values.Length != 3)
                        || !int.TryParse(values[0], out var r)
                        || !int.TryParse(values[1], out var g)
                        || !int.TryParse(values[2], out var b))
                        return;

                    colors[i] = new SKColor((byte)(r % 256), (byte)(g % 256), (byte)(b % 256));
                } else
                    colors[i] = new SKColor();
            }

            Add(new ColorTableEntry(colorIndex, colors));
        }
    }

    public static ColorTable FromEntry(DataArchiveEntry entry) => new(entry.ToStreamSegment());

    public static ColorTable FromFile(string path)
    {
        using var stream = File.Open(
            path,
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new ColorTable(stream);
    }

    #region KeyedCollection implementation
    /// <inheritdoc />
    protected override int GetKeyForItem(ColorTableEntry item) => item.ColorIndex;
    #endregion
}