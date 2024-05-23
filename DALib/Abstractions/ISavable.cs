using System.IO;

namespace DALib.Abstractions;

/// <summary>
///     Defines the pattern for an object that can be saved to a file or stream
/// </summary>
public interface ISavable
{
    /// <summary>
    ///     Saves the object to the specified path
    /// </summary>
    /// <param name="path">
    ///     The path to save the object to
    /// </param>
    void Save(string path);

    /// <summary>
    ///     Writes the object to the specified stream
    /// </summary>
    /// <param name="stream">
    ///     The stream to write the object to
    /// </param>
    void Save(Stream stream);
}