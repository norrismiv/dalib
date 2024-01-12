using System.Collections.ObjectModel;
using System.IO;
using DALib.Data;
using DALib.Extensions;
using SkiaSharp;

namespace DALib.Drawing;

/// <summary>
///     Represents a table of colors. These are most commonly used as dye tables.
/// </summary>
public sealed class ColorTable() : KeyedCollection<int, ColorTableEntry>
{
    private ColorTable(Stream stream)
        : this()
    {
        using var reader = new StreamReader(stream, leaveOpen: true);

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

            Add(
                new ColorTableEntry
                {
                    ColorIndex = colorIndex,
                    Colors = colors
                });
        }
    }

    #region KeyedCollection implementation
    /// <inheritdoc />
    protected override int GetKeyForItem(ColorTableEntry item) => item.ColorIndex;
    #endregion

    #region LoadFrom
    /// <summary>
    ///     Loads a ColorTable from the specified archive entry
    /// </summary>
    /// <param name="entry">The DataArchiveEntry to convert into a ColorTable.</param>
    /// <returns>A ColorTable object created from the provided DataArchiveEntry.</returns>
    public static ColorTable FromEntry(DataArchiveEntry entry)
    {
        using var segment = entry.ToStreamSegment();

        return new ColorTable(segment);
    }

    /// <summary>
    ///     Loads a ColorTable from the specified path
    /// </summary>
    /// <param name="path">The path to the color table file.</param>
    /// <returns>The color table read from the file.</returns>
    public static ColorTable FromFile(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".tbl"),
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new ColorTable(stream);
    }
    #endregion
}