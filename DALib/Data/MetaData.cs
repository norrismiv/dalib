using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DALib.Data
{
    public partial class MetaData
    {
        private List<MetaDataEntry> _entries;
        private ReadOnlyCollection<MetaDataEntry> _entriesReadOnly;
        private Dictionary<string, MetaDataEntry> _entriesDictionary;

        public MetaData(Stream stream) => Init(stream);

        public ReadOnlyCollection<MetaDataEntry> Entries => _entriesReadOnly;

        public MetaDataEntry this[int index] => _entries[index];

        public MetaDataEntry this[string key]
        {
            get
            {
                TryGetEntry(key, out MetaDataEntry entry);
                return entry;
            }
        }

        public static MetaData FromFile(string fileName) => FromFile(fileName, false);

        public static MetaData FromFile(string fileName, bool isCompressed)
        {
            using (var metaDataStream = File.OpenRead(fileName))
            {
                if (!isCompressed)
                    return new MetaData(metaDataStream);

                using (var decompressedMetaDataStream = new MemoryStream())
                using (var decompressionStream = new DeflateStream(metaDataStream, CompressionMode.Decompress))
                {
                    metaDataStream.Seek(2, SeekOrigin.Begin);
                    decompressionStream.CopyTo(decompressedMetaDataStream);
                    decompressedMetaDataStream.Seek(0, SeekOrigin.Begin);
                    return new MetaData(decompressedMetaDataStream);
                }
            }
        }

        public bool ContainsKey(string key) => _entriesDictionary.ContainsKey(key);

        public bool TryGetEntry(string key, out MetaDataEntry entry) => _entriesDictionary.TryGetValue(key, out entry);

        private void Init(Stream stream)
        {
            var encoding = Encoding.GetEncoding(949);

            using (var reader = new BinaryReader(stream, encoding, true))
            {
                var expectedNumberOfEntries = reader.ReadByte() << 8 | reader.ReadByte();

                _entries = new List<MetaDataEntry>();
                _entriesReadOnly = new ReadOnlyCollection<MetaDataEntry>(_entries);
                _entriesDictionary = new Dictionary<string, MetaDataEntry>(StringComparer.CurrentCultureIgnoreCase);

                for (var i = 0; i < expectedNumberOfEntries; ++i)
                {
                    var entryKeyLength = reader.ReadByte();
                    var entryKeyBytes = reader.ReadBytes(entryKeyLength);
                    var entryKey = encoding.GetString(entryKeyBytes);

                    var expectedNumberOfValues = reader.ReadByte() << 8 | reader.ReadByte();

                    var entryValues = new List<string>();

                    for (var j = 0; j < expectedNumberOfValues; ++j)
                    {
                        var entryValueLength = reader.ReadByte() << 8 | reader.ReadByte();
                        var entryValueBytes = reader.ReadBytes(entryValueLength);
                        var entryValue = encoding.GetString(entryValueBytes);
                        entryValues.Add(entryValue);
                    }

                    var entry = new MetaDataEntry(entryKey, entryValues);

                    _entries.Add(entry);
                    _entriesDictionary[entryKey] = entry; // DO NOT USE .Add HERE! Keys are NOT guaranteed to be unique!
                }
            }
        }
    }

    public partial class MetaData : IEnumerable<MetaDataEntry>
    {
        public IEnumerator<MetaDataEntry> GetEnumerator() => ((IEnumerable<MetaDataEntry>)_entries).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<MetaDataEntry>)_entries).GetEnumerator();
    }
}
