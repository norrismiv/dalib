using System.IO;

namespace DALib.Extensions;

public static class StringExtensions
{
    public static string WithExtension(this string str, string extension)
    {
        var ext = Path.GetExtension(str);

        return string.IsNullOrEmpty(ext)
            ? $"{str}.{extension}"
            : str.Replace(ext, $".{extension}");
    }
}