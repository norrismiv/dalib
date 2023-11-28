using System.Collections.Generic;

namespace DALib.Data;

public sealed class MetaFileEntry
{
    public string Key { get; }
    public List<string> Properties { get; }

    internal MetaFileEntry(string key, IEnumerable<string> properties)
    {
        Key = key;
        Properties = new List<string>(properties);
    }
}