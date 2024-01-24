using System.Collections.Generic;
using System.Linq;
using DALib.Drawing;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for Dictionary
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    ///     Get the next available palette ID based on the keys of the given dictionary.
    /// </summary>
    /// <param name="palettes">
    ///     The dictionary containing the palettes
    /// </param>
    public static int GetNextPaletteId(this Dictionary<int, Palette> palettes) => palettes.Keys.Max() + 1;
}