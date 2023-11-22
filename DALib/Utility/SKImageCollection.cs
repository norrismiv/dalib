using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SkiaSharp;

namespace DALib.Utility;

// ReSharper disable once ParameterTypeCanBeEnumerable.Local Collection<T> wont allow this
public class SKImageCollection(IList<SKImage> images) : Collection<SKImage>(images), IDisposable
{
    /// <inheritdoc />
    public virtual void Dispose()
    {
        foreach (var image in Items)
            image.Dispose();

        GC.SuppressFinalize(this);
    }
}