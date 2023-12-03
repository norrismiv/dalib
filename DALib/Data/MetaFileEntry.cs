using System.Collections.Generic;
using System.Linq;

namespace DALib.Data;

public sealed class MetaFileEntry(string key, IEnumerable<string>? properties = null)
{
    public string Key { get; } = key;
    public List<string> Properties { get; } = properties?.ToList() ?? new List<string>();
}