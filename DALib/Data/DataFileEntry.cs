using System.IO;

namespace DALib.Data
{
    public class DataFileEntry
    {
        private DataFile _dataFile;

        public DataFileEntry(DataFile dataFile, string entryName, int address, int fileSize)
        {
            _dataFile = dataFile;
            EntryName = entryName;
            Address = address;
            FileSize = fileSize;
        }
        
        public string EntryName { get; }

        public int Address { get; }

        public int FileSize { get; }

        public Stream Open()
        {
            _dataFile.DataFileStream.Seek(Address, SeekOrigin.Begin);
            return new MemoryStream(_dataFile.DataFileReader.ReadBytes(FileSize));
        }
    }
}
