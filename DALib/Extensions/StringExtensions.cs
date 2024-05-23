using System;
using System.IO;

namespace DALib.Extensions;

/// <summary>
///     Provides extension methods for strings
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Adds or replaces the file extension of a given string with a new extension.
    /// </summary>
    /// <param name="str">
    ///     The string to modify.
    /// </param>
    /// <param name="extension">
    ///     The new extension to add or replace.
    /// </param>
    /// <returns>
    ///     The modified string with the new extension. If the original string does not have an extension, the new extension
    ///     will be appended to it. If the original string already has an extension, it will be replaced with the new
    ///     extension.
    /// </returns>
    /// <remarks>
    ///     The provided extension can omit or include the leading dot
    /// </remarks>
    public static string WithExtension(this string str, string extension)
    {
        var existingExt = Path.GetExtension(str);
        var newExt = extension;

        if (!newExt.StartsWith(".", StringComparison.OrdinalIgnoreCase))
            newExt = $".{newExt}";

        return string.IsNullOrEmpty(existingExt) ? $"{str}{newExt}" : str.Replace(existingExt, $"{newExt}");
    }
}