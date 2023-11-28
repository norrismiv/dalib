using System.IO;

namespace DALib.Abstractions;

public interface ISavable
{
    void Save(string path);

    void Save(Stream stream);
}