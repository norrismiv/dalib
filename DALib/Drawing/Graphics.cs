using System.Text;
using DALib.Data;
using DALib.Definitions;
using DALib.Extensions;
using DALib.Memory;
using DALib.Utility;
using SkiaSharp;

namespace DALib.Drawing;

public class Graphics
{
    public static SKImage RenderImage(EpfFrame frame, Palette palette)
    {
        var fullWidth = frame.Right;
        var fullHeight = frame.Bottom;
        var drawnWidth = frame.Right - frame.Left;
        var drawnHeight = frame.Bottom - frame.Top;

        using var image = new SKBitmap(fullWidth, fullHeight);
        using var canvas = new SKCanvas(image);

        using var drawnPart = SimpleRender(
            drawnWidth,
            drawnHeight,
            frame.Data,
            palette);

        canvas.DrawImage(drawnPart, frame.Left, frame.Top);

        return SKImage.FromBitmap(image);
    }

    public static SKImage RenderImage(MpfFrame frame, Palette palette)
        => SimpleRender(
            frame.Width,
            frame.Height,
            frame.Data,
            palette);

    public static SKImage RenderImage(HpfFile hpf, Palette palette)
        => SimpleRender(
            CONSTANTS.HPF_TILE_WIDTH,
            hpf.Height,
            hpf.Data,
            palette);

    public static SKImage RenderImage(SpfFrame spf, Palette spfPrimaryColorPalette)
    {
        using var bitmap = new SKBitmap(spf.PixelWidth, spf.PixelHeight);

        for (var y = 0; y < spf.PixelHeight; y++)
            for (var x = 0; x < spf.PixelWidth; x++)
            {
                if ((x < spf.PadWidth) || (y < spf.PadHeight))
                {
                    bitmap.SetPixel(x, y, CONSTANTS.Transparent);

                    continue;
                }

                //get the palette index for the current pixel
                //ignore padding on top and left
                var pixelIndex = (spf.PixelWidth - spf.PadWidth) * (y - spf.PadHeight) + (x - spf.PadWidth);
                var paletteIndex = spf.Data[pixelIndex];

                if (paletteIndex == 0)
                {
                    bitmap.SetPixel(x, y, CONSTANTS.Transparent);

                    continue;
                }

                var color = spfPrimaryColorPalette[paletteIndex];
                bitmap.SetPixel(x, y, color);
            }

        return SKImage.FromBitmap(bitmap);
    }

    public static SKImage RenderImage(EfaFrame efa)
    {
        using var bitmap = new SKBitmap(
            efa.ImageWidth,
            efa.ImageHeight,
            SKColorType.Rgba8888,
            SKAlphaType.Premul);

        bitmap.Erase(CONSTANTS.Transparent);

        if (efa.ByteCount == 0)
            return SKImage.FromBitmap(bitmap);

        var reader = new SpanReader(Encoding.Default, efa.Data, Endianness.LittleEndian);

        var dataWidth = efa.ByteWidth / 2;
        var dataHeight = efa.ByteCount / efa.ByteWidth;

        for (var y = 0; y < dataHeight; y++)
            for (var x = 0; x < dataWidth; x++)
            {
                //read the RGB565 color
                var color = reader.ReadRgb565Color(true);

                //ignore extra shit on the right and bottom outside the render dimensions
                if (x >= efa.FrameWidth)
                    continue;

                if (y >= efa.FrameHeight)
                    continue;

                // set alpha based on luminance
                // TODO: may need adjusting
                var adjustedColor = color.WithLuminanceAlpha();

                bitmap.SetPixel(x, y, adjustedColor);
            }

        return SKImage.FromBitmap(bitmap);
    }

    public static SKImage RenderMap(MapFile map, DataArchive seoDat, DataArchive iaDat)
        => RenderMap(
            map,
            Tileset.FromArchive("tilea", seoDat),
            PaletteLookup.FromArchive("mpt", seoDat),
            PaletteLookup.FromArchive("stc", iaDat),
            iaDat);

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

        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var pixelIndex = y * width + x;
                var paletteIndex = data[pixelIndex];

                //apparently palette index 0 is transparent
                //TODO: check if this is also the case for SpfFile
                var color = paletteIndex == 0 ? CONSTANTS.Transparent : palette[paletteIndex];

                bitmap.SetPixel(x, y, color);
            }

        return SKImage.FromBitmap(bitmap);
    }
}