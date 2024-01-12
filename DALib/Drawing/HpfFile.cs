using System;
using System.IO;
using System.Text;
using DALib.Abstractions;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;
using DALib.IO;
using DALib.Memory;
using DALib.Utility;
using SkiaSharp;

namespace DALib.Drawing;

/// <summary>
///     Represents an image file with the ".hpf" extension. This image format support single frame palettized images only.
///     Also supports optional compression. The palettes will be stored in a separate file and will support full RGB888
/// </summary>
public sealed class HpfFile : ISavable
{
    /// <summary>
    ///     The pixel data of the frame encoded as palette indexes
    /// </summary>
    public byte[] Data { get; set; }

    /// <summary>
    ///     8 bytes of header data used to determine if the file is compressed or not. If the first 4 bytes are 0xFF02AA55, the
    ///     image data is compressed
    /// </summary>
    public byte[] HeaderBytes { get; set; }

    /// <summary>
    ///     The pixel height of the image
    /// </summary>
    public int PixelHeight => Data.Length / CONSTANTS.HPF_TILE_WIDTH;

    /// <summary>
    ///     The pixel width of the image
    /// </summary>
    public int PixelWidth => CONSTANTS.HPF_TILE_WIDTH;

    /// <summary>
    ///     Initializes a new instance of the HpfFile class using the specified header bytes and data bytes
    /// </summary>
    /// <param name="headerBytes">The header bytes of the HPF file.</param>
    /// <param name="data">The data bytes of the HPF file.</param>
    public HpfFile(byte[] headerBytes, byte[] data)
    {
        HeaderBytes = headerBytes;
        Data = data;
    }

    private HpfFile(Stream stream)
        : this(stream.ToSpan()) { }

    private HpfFile(Span<byte> buffer)
    {
        var reader = new SpanReader(Encoding.Default, buffer, Endianness.LittleEndian);
        var signature = reader.ReadUInt32();

        if (signature == 0xFF02AA55)
            Compression.DecompressHpf(ref buffer);

        HeaderBytes = buffer[..8]
            .ToArray();

        Data = buffer[8..]
            .ToArray();
    }

    #region SaveTo
    /// <inheritdoc />
    public void Save(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".hpf"),
            new FileStreamOptions
            {
                Access = FileAccess.Write,
                Mode = FileMode.Create,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        Save(stream);
    }

    /// <inheritdoc />
    public void Save(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.Default, true);

        writer.Write(HeaderBytes);
        writer.Write(Data);
    }
    #endregion

    #region LoadFrom
    /// <summary>
    ///     Converts a fully colorized image to an HpfFile
    /// </summary>
    /// <param name="options">
    ///     Options to be used for quantization. EpfFiles can only have a maximum of 256 colors due to being
    ///     a palettized format
    /// </param>
    /// <param name="image">A fully colorized SKImage</param>
    public static Palettized<HpfFile> FromImage(QuantizerOptions options, SKImage image)
    {
        using var quantized = ImageProcessor.Quantize(options, image);

        (var newImage, var palette) = quantized;

        return new Palettized<HpfFile>
        {
            Entity = new HpfFile(new byte[8], newImage.GetPalettizedPixelData(palette)),
            Palette = palette
        };
    }

    /// <summary>
    ///     Loads an HpfFile with the specified fileName from the specified archive
    /// </summary>
    /// <param name="fileName">The name of the HPF file to extract from the archive</param>
    /// <param name="archive">The DataArchive from which to retreive the HPF file</param>
    /// <exception cref="FileNotFoundException">
    ///     Thrown if the HPF file with the specified name is not found in the archive.
    /// </exception>
    public static HpfFile FromArchive(string fileName, DataArchive archive)
    {
        if (!archive.TryGetValue(fileName.WithExtension(".hpf"), out var entry))
            throw new FileNotFoundException($"HPF file with the name \"{fileName}\" was not found in the archive");

        return FromEntry(entry);
    }

    /// <summary>
    ///     Loads an HpfFile from the specified archive entry
    /// </summary>
    /// <param name="entry">The DataArchiveEntry to load the HpfFile from</param>
    public static HpfFile FromEntry(DataArchiveEntry entry) => new(entry.ToSpan());

    /// <summary>
    ///     Loads an HpfFile from the specified path
    /// </summary>
    /// <param name="path">The path of the file to be read.</param>
    public static HpfFile FromFile(string path)
    {
        using var stream = File.Open(
            path.WithExtension(".hpf"),
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.ReadWrite
            });

        return new HpfFile(stream);
    }
    #endregion
}