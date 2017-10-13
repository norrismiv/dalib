using DALib.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace DALib.Drawing
{
    public class ItemPaletteTable
    {
        private List<ItemPaletteTableEntry> _entries;
        private ReadOnlyCollection<ItemPaletteTableEntry> _entriesReadOnly;

        public ItemPaletteTable(Stream stream) => Init(stream);

        public ItemPaletteTable(DataFileEntry entry) : this(entry.Open())
        {
        }

        public ItemPaletteTable(string fileName) : this(File.OpenRead(fileName))
        {
        }

        public ReadOnlyCollection<ItemPaletteTableEntry> Entries => _entriesReadOnly;

        public int GetPaletteNumber(int tileNumber)
        {
            foreach (var entry in _entries)
            {
                if (tileNumber >= entry.MinimumTileNumber && tileNumber <= entry.MaximumTileNumber)
                    return entry.PaletteNumber;
            }
            return 0;
        }

        private void Init(Stream stream)
        {
            _entries = new List<ItemPaletteTableEntry>();
            _entriesReadOnly = new ReadOnlyCollection<ItemPaletteTableEntry>(_entries);

            using (var reader = new StreamReader(stream, Encoding.Default, true, 1024, true))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var vals = line.Split(' ');
                    if (vals.Length == 3 && int.TryParse(vals[0], out int min) && int.TryParse(vals[1], out int max) && int.TryParse(vals[2], out int palNum))
                        _entries.Add(new ItemPaletteTableEntry(min, max, palNum));
                }
            }
        }
    }

    public class ItemPaletteTableEntry
    {
        public ItemPaletteTableEntry(int minimumTileNumber, int maximumTileNumber, int paletteNumber)
        {
            MinimumTileNumber = minimumTileNumber;
            MaximumTileNumber = maximumTileNumber;
            PaletteNumber = paletteNumber;
        }

        public int MinimumTileNumber { get; }

        public int MaximumTileNumber { get; }

        public int PaletteNumber { get; }
    }
}
