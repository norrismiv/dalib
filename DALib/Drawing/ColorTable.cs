using DALib.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Text;

namespace DALib.Drawing
{
    public class ColorTable
    {
        private int _colorsPerEntry;
        private Dictionary<int, ColorTableEntry> _entries;
        private ReadOnlyDictionary<int, ColorTableEntry> _entriesReadOnly;

        public ColorTable(Stream stream) => Init(stream);

        public ColorTable(DataFileEntry dataFileEntry) : this(dataFileEntry.Open())
        {
        }

        public ColorTable(string fileName) : this(File.OpenRead(fileName))
        {
        }

        public static int PaletteStartIndex => 98;

        public int ColorsPerEntry => _colorsPerEntry;

        public ReadOnlyDictionary<int, ColorTableEntry> Entries => _entriesReadOnly;

        public ColorTableEntry this[int index] => _entries[index];

        public bool ContainsColor(int colorNumber) => _entries.ContainsKey(colorNumber);

        public bool TryGetEntry(int colorNumber, out ColorTableEntry entry) => _entries.TryGetValue(colorNumber, out entry);

        private void Init(Stream stream)
        {
            _entries = new Dictionary<int, ColorTableEntry>();
            _entriesReadOnly = new ReadOnlyDictionary<int, ColorTableEntry>(_entries);

            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                if (!int.TryParse(reader.ReadLine(), out _colorsPerEntry))
                    return;

                while (!reader.EndOfStream && int.TryParse(reader.ReadLine(), out int colorNumber))
                {
                    var colors = new Color[_colorsPerEntry];
                    for (var i = 0; i < _colorsPerEntry && !reader.EndOfStream; ++i)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        if (values.Length != 3 || !int.TryParse(values[0], out int r) || !int.TryParse(values[1], out int g) || !int.TryParse(values[2], out int b))
                            return;
                        colors[i] = Color.FromArgb(r % 256, g % 256, b % 256);
                    }
                    _entries[colorNumber] = new ColorTableEntry(colors);
                }
            }
        }
    }
}
