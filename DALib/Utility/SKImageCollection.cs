using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkiaSharp;

namespace DALib.Utility;

/// <summary>
///     Represents a disposable collection of SKImages.
/// </summary>
public class SKImageCollection(IEnumerable<SKImage> images) : Collection<SKImage>(images.ToList()), IDisposable
{
    /// <inheritdoc />
    public virtual void Dispose()
    {
        foreach (var image in Items)
            image.Dispose();

        GC.SuppressFinalize(this);
    }
}