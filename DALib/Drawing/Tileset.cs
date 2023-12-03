using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using DALib.Abstractions;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;
using DALib.Utility;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class Tileset : Collection<Tile>, ISavable
{
    public Tileset() { }

    private Tileset(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.Default, true);

        var tileCount = (int)(stream.Length / CONSTANTS.TILE_SIZE);

        for (var i = 0; i < tileCount; i++)
        {
            var tileData = reader.ReadBytes(CONSTANTS.TILE_SIZE);

            Add(
                new Tile
                {
                    Data = tileData
                });
        }
    }

    #region SaveTo
    public void Save(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".bmp"),
            new FileStreamOptions
            {
                Access = FileAccess.Write,
                Mode = FileMode.Create,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        Save(stream);
    }

    public void Save(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.Default, true);

        var tileCount = (int)(stream.Length / CONSTANTS.TILE_SIZE);

        for (var i = 0; i < tileCount; i++)
        {
            var tile = this[i];

            if (tile.Data.Length != CONSTANTS.TILE_SIZE)
                throw new InvalidDataException($"Tile {i} has an invalid size of {tile.Data.Length} bytes");

            writer.Write(tile.Data);
        }
    }
    #endregion

    #region LoadFrom
    public static Palettized<Tileset> FromImages(IEnumerable<SKImage> orderedFrames) => FromImages(orderedFrames.ToArray());

    public static Palettized<Tileset> FromImages(params SKImage[] images)
    {
        if (images.Any(img => (img.Height * img.Width) != CONSTANTS.TILE_SIZE))
            throw new InvalidDataException("All images must be 56x27");

        using var quantized = ImageProcessor.QuantizeMultiple(QuantizerOptions.Default, images);
        (var quantizedImages, var palette) = quantized;

        var tileset = new Tileset();

        foreach (var image in quantizedImages)
            tileset.Add(
                new Tile
                {
                    Data = image.GetPalettizedPixelData(palette)
                });

        return new Palettized<Tileset>
        {
            Entity = tileset,
            Palette = palette
        };
    }

    public static Tileset FromArchive(string fileName, DataArchive archive)
    {
        if (!archive.TryGetValue(fileName.WithExtension(".bmp"), out var entry))
            throw new FileNotFoundException($"BMP file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }

    public static Tileset FromEntry(DataArchiveEntry entry)
    {
        using var segment = entry.ToStreamSegment();

        return new Tileset(segment);
    }

    public static Tileset FromFile(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".bmp"),
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new Tileset(stream);
    }
    #endregion
}