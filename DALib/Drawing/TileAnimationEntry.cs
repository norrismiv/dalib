using System.Collections;
using System.Collections.Generic;

namespace DALib.Drawing;

public sealed class TileAnimationEntry : IEnumerable<ushort>
{
    /// <summary>
    ///     The number of milliseconds to wait between each frame of the animation.
    /// </summary>
    /// <remarks>
    ///     This number is x100 of the value stored in the text file
    /// </remarks>
    public int AnimationIntervalMs { get; set; } = 500;

    public List<ushort> TileSequence { get; set; } = new();

    /// <inheritdoc />
    public IEnumerator<ushort> GetEnumerator() => TileSequence.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int GetNextTileId(ushort currentTileId)
    {
        var currentIndex = TileSequence.IndexOf(currentTileId);

        if (currentIndex == -1)
            return -1;

        var nextIndex = currentIndex + 1;

        if (nextIndex >= TileSequence.Count)
            nextIndex = 0;

        return TileSequence[nextIndex];
    }
}