using System;
using DALib.Drawing;

namespace DALib.Utility;

public sealed class Palettized<T> : IDisposable
{
    public required T Entity { get; init; }
    public required Palette Palette { get; init; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Entity is IDisposable disposable)
            disposable.Dispose();
    }

    public void Deconstruct(out T entity, out Palette palette)
    {
        entity = Entity;
        palette = Palette;
    }
}