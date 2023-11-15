using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;
using DALib.Memory;

namespace DALib.Drawing;

public sealed class Tileset : Collection<Tile>
{
    public Tileset(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.Default, true);

        var tileCount = (int)(stream.Length / CONSTANTS.TILE_SIZE);

        for (var i = 0; i < tileCount; i++)
        {
            var tileData = reader.ReadBytes(CONSTANTS.TILE_SIZE);

            Add(
                new Tile
                {
                    Id = i,
                    Data = tileData,
                    Width = CONSTANTS.TILE_WIDTH,
                    Height = CONSTANTS.TILE_HEIGHT
                });
        }
    }

    public Tileset(Span<byte> buffer)
    {
        var reader = new SpanReader(Encoding.Default, buffer);

        var tileCount = buffer.Length / CONSTANTS.TILE_SIZE;

        for (var i = 0; i < tileCount; i++)
        {
            var tileData = reader.ReadBytes(CONSTANTS.TILE_SIZE);

            Add(
                new Tile
                {
                    Id = i,
                    Data = tileData,
                    Width = CONSTANTS.TILE_WIDTH,
                    Height = CONSTANTS.TILE_HEIGHT
                });
        }
    }

    public static Tileset FromArchive(string fileName, DataArchive archive)
    {
        if(!archive.TryGetValue(fileName.WithExtension(".bmp"), out var entry))
            throw new FileNotFoundException($"BMP file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }
    
    public static Tileset FromEntry(DataArchiveEntry entry) => new(entry.ToStreamSegment());

    public static Tileset FromFile(string path)
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

        return new Tileset(stream);
    }
}