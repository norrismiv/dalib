using System.Collections.Generic;
using System.IO;
using System.Linq;
using DALib.Extensions;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.SkiaSharp;
using SkiaSharp;

namespace DALib.Utility;

public static class ImageProcessor
{
    public static SKImage CreateMosaic(SKColorType colorType, int padding = 1, params SKImage[] images)
    {
        var width = images.Sum(img => img.Width) + (images.Length - 1) * padding;
        var height = images.Max(img => img.Height);

        using var bitmap = new SKBitmap(
            width,
            height,
            colorType,
            SKAlphaType.Premul);

        using (var canvas = new SKCanvas(bitmap))
        {
            var x = 0;

            foreach (var image in images)
            {
                canvas.DrawImage(image, x, 0);

                x += image.Width + padding;
            }
        }

        return SKImage.FromBitmap(bitmap);
    }

    public static Palettized<SKImageCollection> QuantizeMultiple(SKColorType colorType, params SKImage[] images)
    {
        const int PADDING = 1;

        //create a mosaic of all of the individual images
        using var mosaic = CreateMosaic(colorType, PADDING, images);
        using var bitmap = SKBitmap.FromImage(mosaic);

        //quantize the mosaic
        IQuantizer quantizer = OptimizedPaletteQuantizer.Wu(alphaThreshold: 0);
        using var session = quantizer.Initialize(bitmap.GetReadableBitmapData());

        var quantizedImages = new List<SKImage>();
        var x = 0;

        for (var i = 0; i < images.Length; i++)
        {
            var originalImage = images[i];
            using var quantizedBitmap = new SKBitmap(originalImage.Info.WithColorType(colorType));

            //extract the quantized parts out of the mosaic
            bitmap.ExtractSubset(
                quantizedBitmap,
                new SKRectI(
                    x,
                    0,
                    x + originalImage.Width,
                    originalImage.Height));

            x += quantizedBitmap.Width + PADDING;

            var image = SKImage.FromBitmap(quantizedBitmap);
            using var outStream = File.Create($@"output\test{i}.png");
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(outStream);

            quantizedImages.Add(image);
        }

        return new Palettized<SKImageCollection>
        {
            Entity = new SKImageCollection(quantizedImages),
            Palette = session.Palette!.ToDALibPalette()
        };
    }
}