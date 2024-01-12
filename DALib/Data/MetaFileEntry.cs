using System.Collections.Generic;
using System.Linq;

namespace DALib.Data;

/// <summary>
///     Represents a meta file entry with a key and optional properties.
/// </summary>
public sealed class MetaFileEntry(string key, IEnumerable<string>? properties = null)
{
    /// <summary>
    ///     The key of the entry
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    ///     A collection of properties associated with the key
    /// </summary>
    public List<string> Properties { get; } = properties?.ToList() ?? [];
}