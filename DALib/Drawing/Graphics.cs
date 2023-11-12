using SkiaSharp;

namespace DALib.Drawing;

public class Graphics
{
    private static SKImage RenderTile(
        Tile tile,
        Palette palette
    )
    {
        using var bitmap = new SKBitmap(tile.Width, tile.Height);
        var data = tile.Data;

        for (var y = 0; y < tile.Height; y++)
        {
            for (var x = 0; x < tile.Width; x++)
            {
                var pixelIndex = y * tile.Width + x;
                var paletteIndex = data[pixelIndex];
                var color = palette[paletteIndex];
                
                bitmap.SetPixel(x, y, color);
            }
        }
        
        return SKImage.FromBitmap(bitmap);
    }
}