using DALib.Data;
using System;
using System.IO;

namespace UnpackDataFile
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: UnpackDataFile.exe <dataFilePath> <destinationPath>");
                return;
            }

            var dataFilePath = args[0];

            if (!File.Exists(dataFilePath))
            {
                Console.WriteLine("Data file does not exist.");
                return;
            }

            DataFile dataFile;

            try
            {
                dataFile = DataFile.Open(dataFilePath);
            }
            catch
            {
                Console.WriteLine("Could not open data file.");
                return;
            }

            var destinationPath = args[1];
            Directory.CreateDirectory(destinationPath);

            Console.WriteLine($"Unpacking {dataFile.Entries.Count} files...");

            foreach (var entry in dataFile)
            {
                var destinationFileName = Path.Combine(destinationPath, entry.EntryName);
                using (var entryStream = entry.Open())
                using (var fileStream = File.OpenWrite(destinationFileName))
                {
                    entryStream.CopyTo(fileStream);
                }
                Console.WriteLine($"    Unpacked {entry.EntryName}");
            }

            Console.WriteLine($"Finished unpacking {dataFile.Entries.Count} files.");
        }
    }
}
