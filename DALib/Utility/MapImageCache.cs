using System;

namespace DALib.Utility;

/// <summary>
/// A collection of <see cref="SKImageCache{TKey}"/> caches that can be used as a shared cache for rendering multiple maps.
/// </summary>
/// <param name="bgCache">The background cache</param>
/// <param name="lfgCache">The left foreground cache</param>
/// <param name="rfgCache">The right foreground cache</param>
public class MapImageCache(SKImageCache<int> bgCache, SKImageCache<int> lfgCache, SKImageCache<int> rfgCache): IDisposable
{
    /// <summary>
    /// The background cache
    /// </summary>
    public SKImageCache<int> BackgroundCache { get; } = bgCache;
    /// <summary>
    /// The left foreground cache
    /// </summary>
    public SKImageCache<int> LeftForegroundCache { get; } = lfgCache;
    /// <summary>
    /// The right foreground cache
    /// </summary>
    public SKImageCache<int> RightForegroundCache { get; } = rfgCache;

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        BackgroundCache.Dispose();
        LeftForegroundCache.Dispose();
        RightForegroundCache.Dispose();
        GC.SuppressFinalize(this);
    }
}

