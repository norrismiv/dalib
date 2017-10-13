using DALib.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace DALib.Drawing
{
    public class PaletteTable
    {
        private List<PaletteTableEntry> _entries;
        private ReadOnlyCollection<PaletteTableEntry> _entriesReadOnly;
        private Dictionary<int, int> _singleValueEntries;

        public PaletteTable(Stream stream) => Init(stream);

        public PaletteTable(DataFileEntry entry) : this(entry.Open())
        {
        }

        public PaletteTable(string fileName) : this(File.OpenRead(fileName))
        {
        }

        public ReadOnlyCollection<PaletteTableEntry> Entries => _entriesReadOnly;

        public int GetPaletteNumber(int tileNumber)
        {
            if (_singleValueEntries.TryGetValue(tileNumber, out int paletteNumber))
                return paletteNumber;

            foreach (var entry in _entries)
            {
                if (tileNumber >= entry.MinimumTileNumber && tileNumber <= entry.MaximumTileNumber)
                    return entry.PaletteNumber;
            }

            return 0;
        }

        private void Init(Stream stream)
        {
            _entries = new List<PaletteTableEntry>();
            _entriesReadOnly = new ReadOnlyCollection<PaletteTableEntry>(_entries);
            _singleValueEntries = new Dictionary<int, int>();

            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var vals = line.Split(' ');
                    if (vals.Length < 2 || !int.TryParse(vals[0], out int min) || !int.TryParse(vals[1], out int paletteNumOrMax))
                        continue;
                    if (vals.Length == 2)
                        _singleValueEntries.Add(min, paletteNumOrMax);
                    else if (vals.Length == 3 && int.TryParse(vals[2], out int paletteNumber))
                        _entries.Add(new PaletteTableEntry(min, paletteNumOrMax, paletteNumber));
                }
            }
        }
    }

    public class PaletteTableEntry
    {
        public PaletteTableEntry(int minimumTileNumber, int maximumTileNumber, int paletteNumber)
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
