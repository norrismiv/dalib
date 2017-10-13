using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace DALib.Data
{
    public partial class DataFile
    {
        private const int EntryNameLength = 13;

        private List<DataFileEntry> _entries;
        private ReadOnlyCollection<DataFileEntry> _entriesReadOnly;
        private Dictionary<string, DataFileEntry> _entriesDictionary;

        public DataFile(Stream stream) => Init(stream);

        public ReadOnlyCollection<DataFileEntry> Entries
        {
            get
            {
                ThrowIfDisposed();
                return _entriesReadOnly;
            }
        }

        internal Stream DataFileStream { get; private set; }

        internal BinaryReader DataFileReader { get; private set; }

        public DataFileEntry this[string entryName]
        {
            get
            {
                ThrowIfDisposed();
                return _entriesDictionary[entryName];
            }
        }

        public static DataFile Open(string fileName) => new DataFile(File.OpenRead(fileName));

        public DataFileEntry GetEntry(string entryName)
        {
            _entriesDictionary.TryGetValue(entryName, out DataFileEntry result);
            return result;
        }

        private void Init(Stream stream)
        {
            DataFileStream = stream;
            DataFileReader = new BinaryReader(stream);

            _entries = new List<DataFileEntry>();
            _entriesReadOnly = new ReadOnlyCollection<DataFileEntry>(_entries);
            _entriesDictionary = new Dictionary<string, DataFileEntry>(StringComparer.CurrentCultureIgnoreCase);

            stream.Seek(0, SeekOrigin.Begin);

            var expectedNumberOfEntries = DataFileReader.ReadInt32() - 1;

            for (var i = 0; i < expectedNumberOfEntries; ++i)
            {
                var startAddress = DataFileReader.ReadInt32();

                var nameBytes = new byte[EntryNameLength];
                DataFileReader.Read(nameBytes, 0, EntryNameLength);

                var name = Encoding.ASCII.GetString(nameBytes);
                var nullChar = name.IndexOf('\0');
                if (nullChar > -1)
                    name = name.Substring(0, nullChar);

                var endAddress = DataFileReader.ReadInt32();

                stream.Seek(-4, SeekOrigin.Current);

                var entry = new DataFileEntry(this, name, startAddress, endAddress - startAddress);

                _entries.Add(entry);
                _entriesDictionary[name] = entry;
            }
        }
    }

    public partial class DataFile : IDisposable
    {
        private bool _disposed;

        ~DataFile()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (DataFileStream != null)
                {
                    DataFileStream.Dispose();
                    DataFileStream = null;
                }

                if (DataFileReader != null)
                {
                    DataFileReader.Dispose();
                    DataFileReader = null;
                }
            }

            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().ToString());
        }
    }

    public partial class DataFile : IEnumerable<DataFileEntry>
    {
        public IEnumerator<DataFileEntry> GetEnumerator() => ((IEnumerable<DataFileEntry>)_entries).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<DataFileEntry>)_entries).GetEnumerator();
    }
}
