using System;
using System.IO;

namespace DALib.Extensions;

public static class StringExtensions
{
    public static string WithExtension(this string str, string extension)
    {
        var existingExt = Path.GetExtension(str);
        var newExt = extension;
        
        if(!newExt.StartsWith(".", StringComparison.OrdinalIgnoreCase))
            newExt = $".{newExt}";

        return string.IsNullOrEmpty(existingExt)
            ? $"{str}{newExt}"
            : str.Replace(existingExt, $"{newExt}");
    }
}