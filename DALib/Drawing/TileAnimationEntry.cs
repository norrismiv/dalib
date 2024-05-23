using System.Collections;
using System.Collections.Generic;

namespace DALib.Drawing;

/// <summary>
///     Represents an entry in a tile animation sequence.
/// </summary>
public sealed class TileAnimationEntry : IEnumerable<ushort>
{
    /// <summary>
    ///     The number of milliseconds to wait between each frame of the animation.
    /// </summary>
    /// <remarks>
    ///     This property determines the delay between each frame in the animation. It is measured in milliseconds. The default
    ///     value is 500 milliseconds. The actual duration of the interval is calculated by multiplying this value by 100. This
    ///     is necessary because the value is stored in a text file as a hundredth of the desired interval time.
    /// </remarks>
    public int AnimationIntervalMs { get; set; } = 500;

    /// <summary>
    ///     A sequence of tile IDs that make up the animation.
    /// </summary>
    public List<ushort> TileSequence { get; set; } = [];

    /// <inheritdoc />
    public IEnumerator<ushort> GetEnumerator() => TileSequence.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     Retrieves the ID of the next tile in the sequence, based on the given current tile ID.
    /// </summary>
    /// <param name="currentTileId">
    ///     The ID of the current tile
    /// </param>
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