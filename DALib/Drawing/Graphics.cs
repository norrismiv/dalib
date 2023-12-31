using System;
using System.Text;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;
using DALib.Memory;
using DALib.Utility;
using SkiaSharp;

namespace DALib.Drawing;

/// <summary>
///     Graphics class provides various methods for rendering images
/// </summary>
public static class Graphics
{
    private static SKImage PaddedRender(
        int top,
        int left,
        int bottom,
        int right,
        byte[] data,
        Palette palette)
    {
        var drawnWidth = right - left;
        var drawnHeight = bottom - top;

        var hasTransparency = palette[0]
                                  .Equals(SKColors.Black)
                              || palette[0]
                                  .Equals(CONSTANTS.Transparent);

        using var bitmap = new SKBitmap(left, bottom);

        bitmap.Erase(CONSTANTS.Transparent);

        for (var y = 0; y < drawnWidth; y++)
            for (var x = 0; x < drawnHeight; x++)
            {
                var xActual = x + left;
                var yActual = y + top;

                var pixelIndex = y * drawnWidth + x;
                var paletteIndex = data[pixelIndex];

                //if the paletteIndex is 0, and that color is pure black or transparent black
                var color = (paletteIndex == 0) && hasTransparency ? CONSTANTS.Transparent : palette[paletteIndex];

                bitmap.SetPixel(xActual, yActual, color);
            }

        return SKImage.FromBitmap(bitmap);
    }

    private static SKImage PaddedRender(
        int top,
        int left,
        int bottom,
        int right,
        SKColor[] data)
    {
        var drawnWidth = right - left;
        var drawnHeight = bottom - top;

        using var bitmap = new SKBitmap(drawnWidth, drawnHeight);

        bitmap.Erase(CONSTANTS.Transparent);

        for (var y = 0; y < drawnHeight; y++)
            for (var x = 0; x < drawnWidth; x++)
            {
                var xActual = x + left;
                var yActual = y + top;

                var pixelIndex = y * drawnWidth + x;
                var color = data[pixelIndex];

                if ((color == CONSTANTS.Transparent) || (color == SKColors.Black))
                    continue;

                bitmap.SetPixel(xActual, yActual, color);
            }

        return SKImage.FromBitmap(bitmap);
    }

    /// <summary>
    ///     Renders an EpfFrame
    /// </summary>
    /// <param name="frame">The frame to render</param>
    /// <param name="palette">A palette containing colors used by the frame</param>
    public static SKImage RenderImage(EpfFrame frame, Palette palette)
        => PaddedRender(
            frame.Top,
            frame.Left,
            frame.Bottom,
            frame.Right,
            frame.Data,
            palette);

    /// <summary>
    ///     Renders an MpfFrame
    /// </summary>
    /// <param name="frame">The frame to render</param>
    /// <param name="palette">A palette containing colors used by the frame</param>
    public static SKImage RenderImage(MpfFrame frame, Palette palette)
        => PaddedRender(
            frame.Top,
            frame.Left,
            frame.Bottom,
            frame.Right,
            frame.Data,
            palette);

    /// <summary>
    ///     Renders an HpfFile
    /// </summary>
    /// <param name="hpf">The file to render</param>
    /// <param name="palette">A palette containing colors used by the frame</param>
    public static SKImage RenderImage(HpfFile hpf, Palette palette)
        => SimpleRender(
            CONSTANTS.HPF_TILE_WIDTH,
            hpf.PixelHeight,
            hpf.Data,
            palette);

    /// <summary>
    ///     Renders a palettized SPF frame
    /// </summary>
    /// <param name="spf">The frame to render. Must be a palettized SpfFrame</param>
    /// <param name="spfPrimaryColorPalette">The primary color palette of the SpfFile. (see SpfFile.Format)</param>
    public static SKImage RenderImage(SpfFrame spf, Palette spfPrimaryColorPalette)
        => PaddedRender(
            spf.Top,
            spf.Left,
            spf.Top + spf.PixelHeight,
            spf.Left + spf.PixelWidth,
            spf.Data!,
            spfPrimaryColorPalette);

    /// <summary>
    ///     Renders a colorized SpfFrame
    /// </summary>
    /// <param name="spf">The frame to render. Must be a colorized SpfFrame. (see SpfFile.Format)</param>
    public static SKImage RenderImage(SpfFrame spf)
        => PaddedRender(
            spf.Top,
            spf.Left,
            spf.Top + spf.PixelHeight,
            spf.Left + spf.PixelWidth,
            spf.ColorData!);

    /// <summary>
    ///     Renders an EfaFrame
    /// </summary>
    /// <param name="efa">The frame to render</param>
    /// <param name="efaBlendingType">The alpha blending type to use</param>
    public static SKImage RenderImage(EfaFrame efa, EfaBlendingType efaBlendingType = EfaBlendingType.Luminance)
    {
        using var bitmap = new SKBitmap(
            efa.ImagePixelWidth,
            efa.ImagePixelHeight,
            SKColorType.Rgba8888,
            SKAlphaType.Premul);

        bitmap.Erase(CONSTANTS.Transparent);

        if (efa.ByteCount == 0)
            return SKImage.FromBitmap(bitmap);

        var reader = new SpanReader(Encoding.Default, efa.Data, Endianness.LittleEndian);

        //we will iterate over the data to render the image
        var dataWidth = efa.ByteWidth / 2;
        var dataHeight = efa.ByteCount / efa.ByteWidth;

        for (var y = 0; y < dataHeight; y++)
            for (var x = 0; x < dataWidth; x++)
            {
                //left and top are padding, add them to the x/y
                var xActual = x + efa.Left;
                var yActual = y + efa.Top;

                //read the RGB565 color
                var color = reader.ReadRgb565Color();

                //for some reason these images can have extra trash data on the right and bottom
                //we avoid it by obeying the frame pixel width/height vs padded x/y
                if (xActual >= efa.FramePixelWidth)
                    continue;

                if (yActual >= efa.FramePixelHeight)
                    continue;

                // set alpha based on luminance
                // this may be slightly off, but it's close enough for now
                var coefficient = efaBlendingType switch
                {
                    EfaBlendingType.Luminance     => 1f,
                    EfaBlendingType.LessLuminance => 1.25f, //more alpha is less transparent
                    EfaBlendingType.NotSure       => -1f,
                    _                             => throw new ArgumentOutOfRangeException(nameof(efaBlendingType), efaBlendingType, null)
                };

                //if the coefficient is positive, add luminance alpha to the color
                if (coefficient > 0)
                    color = color.WithLuminanceAlpha(coefficient);

                bitmap.SetPixel(xActual, yActual, color);
            }

        return SKImage.FromBitmap(bitmap);
    }

    /// <summary>
    ///     Renders a MapFile, given the archives that contain required data
    /// </summary>
    /// <param name="map">The map file to render.</param>
    /// <param name="seoDat">The SEO archive.</param>
    /// <param name="iaDat">The IA archive.</param>
    public static SKImage RenderMap(MapFile map, DataArchive seoDat, DataArchive iaDat)
        => RenderMap(
            map,
            Tileset.FromArchive("tilea", seoDat),
            PaletteLookup.FromArchive("mpt", seoDat)
                         .Freeze(),
            PaletteLookup.FromArchive("stc", iaDat)
                         .Freeze(),
            iaDat);

    /// <summary>
    ///     Renders a MapFile, given already extracted information
    /// </summary>
    /// <param name="map">The MapFile to render</param>
    /// <param name="tiles">A collection of background tiles</param>
    /// <param name="bgPaletteLookup">PaletteLookup for background tiles</param>
    /// <param name="fgPaletteLookup">PaletteLookup for foreground tiles</param>
    /// <param name="iaDat">IA archive for reading foreground tile files</param>
    public static SKImage RenderMap(
        MapFile map,
        Tileset tiles,
        PaletteLookup bgPaletteLookup,
        PaletteLookup fgPaletteLookup,
        DataArchive iaDat)
    {
        const int FOREGROUND_PADDING = 512;

        //create lookups so we only render each tile piece once
        using var bgCache = new SKImageCache<int>();
        using var lfgCache = new SKImageCache<int>();
        using var rfgCache = new SKImageCache<int>();

        //calculate width and height based on isometric view
        var width = map.Width * CONSTANTS.TILE_WIDTH;
        var height = map.Height * (CONSTANTS.TILE_HEIGHT + 1) + FOREGROUND_PADDING;
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        //the first tile drawn is the center tile at the top (0, 0)
        var bgInitialDrawX = width / 2 - CONSTANTS.HALF_TILE_WIDTH;
        var bgInitialDrawY = FOREGROUND_PADDING;

        //render background tiles and draw them to the canvas
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var bgIndex = map.Tiles[x, y].Background;

                if (bgIndex > 0)
                    --bgIndex;

                var bgImage = bgCache.GetOrCreate(
                    bgIndex,
                    index =>
                    {
                        var palette = bgPaletteLookup.GetPaletteForId(index + 2);

                        return RenderTile(tiles[index], palette);
                    });

                //for each X axis iteration, we want to move the draw position half a tile to the right and down from the initial draw position
                var drawX = bgInitialDrawX + x * CONSTANTS.HALF_TILE_WIDTH;
                var drawY = bgInitialDrawY + x * CONSTANTS.HALF_TILE_HEIGHT;

                canvas.DrawImage(bgImage, drawX, drawY);
            }

            //for each Y axis iteration, we want to move the draw position half a tile to the left and down from the initial draw position
            bgInitialDrawX -= CONSTANTS.HALF_TILE_WIDTH;
            bgInitialDrawY += CONSTANTS.HALF_TILE_HEIGHT;
        }

        //render left and right foreground tiles and draw them to the canvas
        var fgInitialDrawX = width / 2 - CONSTANTS.HALF_TILE_WIDTH;
        var fgInitialDrawY = FOREGROUND_PADDING;

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var tile = map.Tiles[x, y];
                var lfgIndex = tile.LeftForeground;
                var rfgIndex = tile.RightForeground;

                //render left foreground
                var lfgImage = lfgCache.GetOrCreate(
                    lfgIndex,
                    index =>
                    {
                        var hpf = HpfFile.FromArchive($"stc{index:D5}.hpf", iaDat);
                        var palette = fgPaletteLookup.GetPaletteForId(index + 1);

                        return RenderImage(hpf, palette);
                    });

                //for each X axis iteration, we want to move the draw position half a tile to the right and down from the initial draw position
                var lfgDrawX = fgInitialDrawX + x * CONSTANTS.HALF_TILE_WIDTH;

                var lfgDrawY = fgInitialDrawY + (x + 1) * CONSTANTS.HALF_TILE_HEIGHT - lfgImage.Height + CONSTANTS.HALF_TILE_HEIGHT;

                if ((lfgIndex % 10000) > 1)
                    canvas.DrawImage(lfgImage, lfgDrawX, lfgDrawY);

                //render right foreground
                var rfgImage = rfgCache.GetOrCreate(
                    rfgIndex,
                    index =>
                    {
                        var hpf = HpfFile.FromArchive($"stc{index:D5}.hpf", iaDat);
                        var palette = fgPaletteLookup.GetPaletteForId(index + 1);

                        return RenderImage(hpf, palette);
                    });

                //for each X axis iteration, we want to move the draw position half a tile to the right and down from the initial draw position
                var rfgDrawX = fgInitialDrawX + (x + 1) * CONSTANTS.HALF_TILE_WIDTH;

                var rfgDrawY = fgInitialDrawY + (x + 1) * CONSTANTS.HALF_TILE_HEIGHT - rfgImage.Height + CONSTANTS.HALF_TILE_HEIGHT;

                if ((rfgIndex % 10000) > 1)
                    canvas.DrawImage(rfgImage, rfgDrawX, rfgDrawY);
            }

            //for each Y axis iteration, we want to move the draw position half a tile to the left and down from the initial draw position
            fgInitialDrawX -= CONSTANTS.HALF_TILE_WIDTH;
            fgInitialDrawY += CONSTANTS.HALF_TILE_HEIGHT;
        }

        return SKImage.FromBitmap(bitmap);
    }

    /// <summary>
    ///     Renders a Tile
    /// </summary>
    /// <param name="tile">The tile to render</param>
    /// <param name="palette">A palette containing colors used by the tile</param>
    public static SKImage RenderTile(Tile tile, Palette palette)
        => SimpleRender(
            CONSTANTS.TILE_WIDTH,
            CONSTANTS.TILE_HEIGHT,
            tile.Data,
            palette);

    private static SKImage SimpleRender(
        int width,
        int height,
        byte[] data,
        Palette palette)
    {
        using var bitmap = new SKBitmap(width, height);

        var hasTransparency = palette[0]
                                  .Equals(SKColors.Black)
                              || palette[0]
                                  .Equals(CONSTANTS.Transparent);

        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var pixelIndex = y * width + x;
                var paletteIndex = data[pixelIndex];

                //if the paletteIndex is 0, and that color is pure black or transparent black
                var color = (paletteIndex == 0) && hasTransparency ? CONSTANTS.Transparent : palette[paletteIndex];

                bitmap.SetPixel(x, y, color);
            }

        return SKImage.FromBitmap(bitmap);
    }
}