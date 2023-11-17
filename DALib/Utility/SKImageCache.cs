using System;
using System.Collections.Generic;
using SkiaSharp;

namespace DALib.Utility;

public class SKImageCache<TKey>(IEqualityComparer<TKey>? comparer = null) : IDisposable where TKey: IEquatable<TKey>
{
    private readonly Dictionary<TKey, SKImage> Cache = new(comparer);

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var image in Cache.Values)
            image.Dispose();

        GC.SuppressFinalize(this);
    }

    public SKImage GetOrCreate(TKey key, Func<TKey, SKImage> create)
    {
        if (Cache.TryGetValue(key, out var image))
            return image;

        image = create(key);

        Cache.Add(key, image);

        return image;
    }
}