using System.Collections.Generic;
using System.Linq;
using DALib.Drawing;

namespace DALib.Extensions;

public static class DictionaryExtensions
{
    public static int GetNextPaletteId(this Dictionary<int, Palette> palettes) => palettes.Keys.Max() + 1;
}