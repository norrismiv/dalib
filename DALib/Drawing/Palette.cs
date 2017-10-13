using DALib.Data;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DALib.Drawing
{
    public class Palette
    {
        private const int ColorsPerPalette = 256;

        private Color[] _colors;
        private ReadOnlyCollection<Color> _colorsReadOnly;

        private Palette()
        {
            _colors = new Color[ColorsPerPalette];
            _colorsReadOnly = new ReadOnlyCollection<Color>(_colors);
        }

        public Palette(Stream stream) => Init(stream);

        public Palette(DataFileEntry dataFileEntry) : this(dataFileEntry.Open())
        {
        }

        public Palette(string fileName) : this(File.OpenRead(fileName))
        {
        }

        public ReadOnlyCollection<Color> Colors => _colorsReadOnly;

        public Color this[int index] => _colors[index];

        public Palette Dye(ColorTableEntry colorTableEntry)
        {
            var dyedPalette = new Palette();
            Array.Copy(_colors, dyedPalette._colors, ColorsPerPalette);
            for (var i = 0; i < colorTableEntry.ColorCount; ++i)
            {
                dyedPalette._colors[i + ColorTable.PaletteStartIndex] = colorTableEntry[i];
            }
            return dyedPalette;
        }

        public Bitmap Render(byte[] imageData, int width, int height)
        {
            if (width == 0 || height == 0)
                return new Bitmap(1, 1);

            var bitmap = new Bitmap(width, height);

            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            var dataPointer = bitmapData.Scan0;

            var pixelData = new byte[width * height * 4];

            var imageDataIndex = 0;
            var pixelDataIndex = 0;

            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    var colorIndex = imageData[imageDataIndex++];
                    var color = colorIndex == 0 ? Color.Transparent : _colors[colorIndex];

                    pixelData[pixelDataIndex++] = color.B;
                    pixelData[pixelDataIndex++] = color.G;
                    pixelData[pixelDataIndex++] = color.R;
                    pixelData[pixelDataIndex++] = color.A;
                }
            }

            Marshal.Copy(pixelData, 0, dataPointer, pixelData.Length);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        private void Init(Stream stream)
        {
            _colors = new Color[ColorsPerPalette];
            _colorsReadOnly = new ReadOnlyCollection<Color>(_colors);

            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                stream.Seek(0, SeekOrigin.Begin);

                for (var i = 0; i < ColorsPerPalette; ++i)
                {
                    _colors[i] = Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                }
            }
        }
    }
}
