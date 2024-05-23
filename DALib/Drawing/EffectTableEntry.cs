using System.Collections;
using System.Collections.Generic;

namespace DALib.Drawing;

/// <summary>
///     Represents an entry in the effect table, containing a sequence of frame indexes that make up an animation.
/// </summary>
public sealed class EffectTableEntry : IEnumerable<int>
{
    /// <summary>
    ///     A sequence of frame indexes that make up the animation.
    /// </summary>
    public List<int> FrameSequence { get; set; } = [];

    /// <inheritdoc />
    public IEnumerator<int> GetEnumerator() => FrameSequence.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     Retrieves the frame index of the next frame in the sequuence
    /// </summary>
    /// <param name="currentAnimationIndex">
    ///     The ID of the current tile
    /// </param>
    public int GetNextFrameIndex(int currentAnimationIndex)
    {
        if (currentAnimationIndex >= FrameSequence.Count)
            currentAnimationIndex = 0;

        return FrameSequence[currentAnimationIndex];
    }
}