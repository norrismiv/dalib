using System;
using System.Collections.Generic;
using System.Text;

namespace DALib.Drawing
{
    public interface IRenderable
    {
        int Width { get; }
        int Height { get; }
        int Top { get; }
        int Left { get; }
        byte[] Data { get; }
    }
}
