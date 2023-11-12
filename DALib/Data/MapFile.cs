using System;
using System.IO;
using System.Text;
using DALib.Extensions;
using DALib.Memory;

namespace DALib.Data;

public class MapFile
{
    public int Height { get; }
    public MapTile[,] Tiles { get; }
    public int Width { get; }

    public MapFile(int width, int height)
    {
        Tiles = new MapTile[width, height];
        Width = width;
        Height = height;
    }

    public MapFile(Stream stream, int width, int height)
        : this(width, height)
    {
        using var reader = new BinaryReader(stream, Encoding.GetEncoding(949), true);

        for (var y = 0; y < Height; ++y)
        {
            for (var x = 0; x < Width; ++x)
            {
                var background = reader.ReadInt16(true);
                var leftForeground = reader.ReadInt16(true);
                var rightForeground = reader.ReadInt16(true);
                Tiles[x, y] = new MapTile(background, leftForeground, rightForeground);
            }
        }
    }

    public MapFile(Span<byte> buffer, int width, int height)
        : this(width, height)
    {
        var reader = new SpanReader(Encoding.GetEncoding(949), buffer);

        for (var y = 0; y < Height; ++y)
        {
            for (var x = 0; x < Width; ++x)
            {
                var background = reader.ReadInt16();
                var leftForeground = reader.ReadInt16();
                var rightForeground = reader.ReadInt16();
                Tiles[x, y] = new MapTile(background, leftForeground, rightForeground);
            }
        }
    }

    public static MapFile FromFile(string path, int width, int height)
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

        return new MapFile(stream, width, height);
    }

    public MapTile this[int x, int y] => Tiles[x, y];
}

public sealed class MapTile
{
    public int Background { get; }

    public int LeftForeground { get; }

    public int RightForeground { get; }

    public MapTile(int background, int leftForeground, int rightForeground)
    {
        Background = background;
        LeftForeground = leftForeground;
        RightForeground = rightForeground;
    }
}