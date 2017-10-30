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

        public Palette(Stream stream)
        {
            Init(stream);
        }

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

        private void Init(Stream stream)
        {
            _colors = new Color[ColorsPerPalette];
            _colorsReadOnly = new ReadOnlyCollection<Color>(_colors);

            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                for (var i = 0; i < ColorsPerPalette; ++i)
                {
                    _colors[i] = Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                }
            }
        }
    }
}
