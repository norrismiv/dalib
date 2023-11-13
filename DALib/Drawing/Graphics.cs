using System;
using System.Collections.Generic;
using DALib.Data;
using DALib.Definitions;
using SkiaSharp;

namespace DALib.Drawing;

public class Graphics
{
    public static SKImage RenderTile(
        Tile tile,
        Palette palette
    ) => SimpleRender(tile.Width, tile.Height, tile.Data, palette);
    
    public static SKImage RenderImage(
        MpfFrame frame,
        Palette palette
    ) => SimpleRender(frame.Width, frame.Height, frame.Data, palette);
    
    public static SKImage RenderImage(
        EpfFrame frame,
        Palette palette
    ) => SimpleRender(frame.Width, frame.Height, frame.Data, palette);
    
    public static SKImage RenderImage(
        HpfFile hpf,
        Palette palette
    ) => SimpleRender(hpf.Width, hpf.Height, hpf.Data, palette);
    
    public static SKImage RenderMap(
        MapFile map,
        DataArchive seoDat,
        DataArchive iaDat
    )
    {
        const int FOREGROUND_PADDING = 256;
        //load tiles, palettes, palette tables
        var tiles = Tileset.FromArchive("tilea", seoDat);
        var bgPaletteLookup = PaletteLookup.FromArchive("stc", iaDat);
        var fgPaletteLookup = PaletteLookup.FromArchive("stc", iaDat);
        
        //create lookups so we only render each tile once
        var bgLookup = new Dictionary<int, SKImage>();
        var lfgLookup = new Dictionary<int, SKImage>();
        var rfgLookup = new Dictionary<int, SKImage>();
        
        //calculate width and height based on orthagonal view
        var width = CONSTANTS.TILE_WIDTH + (map.Width - 1) * (CONSTANTS.TILE_WIDTH / 2) + FOREGROUND_PADDING;
        var height = CONSTANTS.TILE_HEIGHT + (map.Height - 1) * (CONSTANTS.TILE_HEIGHT / 2) + FOREGROUND_PADDING;
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        //the first tile drawn is the center tile at the top (0, 0)
        var bgInitialDrawX = width / 2 - CONSTANTS.TILE_WIDTH / 2;
        var bgInitialDrawY = FOREGROUND_PADDING;
        
        //render background tiles and draw them to the canvas
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var bgIndex = map.Tiles[x, y].Background;

                if (bgIndex > 0)
                    --bgIndex;

                if (!bgLookup.TryGetValue(bgIndex, out var bgImage))
                {
                    bgImage = RenderTile(tiles[bgIndex], bgPaletteLookup.GetPaletteForId(bgIndex));

                    bgLookup[bgIndex] = bgImage;
                }

                var drawX = bgInitialDrawX + x * (CONSTANTS.TILE_WIDTH / 2);
                var drawY = bgInitialDrawY + x * (CONSTANTS.TILE_HEIGHT / 2);

                canvas.DrawImage(bgImage, drawX, drawY);
            }
            
            bgInitialDrawX -= CONSTANTS.TILE_WIDTH / 2;
            bgInitialDrawY += CONSTANTS.TILE_HEIGHT / 2;
        }
        
        //render left and right foreground tiles and draw them to the canvas
        var fgInitialDrawX = width / 2 - CONSTANTS.TILE_WIDTH / 2;
        var fgInitialDrawY = FOREGROUND_PADDING;
        
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var tile = map.Tiles[x, y];
                var lfgIndex = tile.LeftForeground;
                var rfgIndex = tile.RightForeground;
                
                //render left foreground
                if (!lfgLookup.TryGetValue(lfgIndex, out var lfgImage))
                {
                    var hpf = HpfFile.FromArchive($"stc{lfgIndex:D5}.hpf", iaDat);
                    lfgImage = RenderImage(hpf, fgPaletteLookup.GetPaletteForId(lfgIndex + 1));

                    lfgLookup[lfgIndex] = lfgImage;
                }
                
                //offset the Y value
                var lfgDrawX = fgInitialDrawX + x * (CONSTANTS.TILE_WIDTH / 2);
                var lfgDrawY = fgInitialDrawY + (x + 1) * (CONSTANTS.TILE_HEIGHT / 2) - lfgImage.Height + CONSTANTS.TILE_HEIGHT / 2;

                if (lfgIndex % 10000 > 1)
                    canvas.DrawImage(lfgImage, lfgDrawX, lfgDrawY);

                //render right foreground
                if (!rfgLookup.TryGetValue(rfgIndex, out var rfgImage))
                {
                    var hpf = HpfFile.FromArchive($"stc{rfgIndex:D5}.hpf", iaDat);
                    rfgImage = RenderImage(hpf, fgPaletteLookup.GetPaletteForId(rfgIndex + 1));

                    rfgLookup[rfgIndex] = rfgImage;
                }

                var rfgDrawX = fgInitialDrawX + (x + 1) * (CONSTANTS.TILE_WIDTH / 2);
                var rfgDrawY = fgInitialDrawY + (x + 1) * (CONSTANTS.TILE_HEIGHT / 2) - rfgImage.Height + CONSTANTS.TILE_HEIGHT / 2;

                if (rfgIndex % 10000 > 1)
                    canvas.DrawImage(rfgImage, rfgDrawX, rfgDrawY);
            }
            
            fgInitialDrawX -= CONSTANTS.TILE_WIDTH / 2;
            fgInitialDrawY += CONSTANTS.TILE_HEIGHT / 2;
        }
        
        //canvas.Flush();
        return SKImage.FromBitmap(bitmap);
    }
    
    private static SKImage SimpleRender(
        int width,
        int height,
        byte[] data,
        Palette palette
    )
    {
        using var bitmap = new SKBitmap(width, height);
        
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixelIndex = y * width + x;
                var paletteIndex = data[pixelIndex];
                var color = palette[paletteIndex];
                
                bitmap.SetPixel(x, y, color);
            }
        }
        
        return SKImage.FromBitmap(bitmap);
    }
}