using System;
using System.Drawing;

namespace DALib.Drawing
{
    public class EpfFrame : IRenderable
    {
        private readonly byte[] _data;

        public int Top { get; }
        public int Left { get; }
        public int Bottom { get; }
        public int Right { get; }
        public int Width => Right - Left;
        public int Height => Bottom - Top;
        public byte[] Data => _data;

        public EpfFrame(int top, int left, int bottom, int right, byte[] data)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
            _data = new byte[data.Length];
            Buffer.BlockCopy(data, 0, _data, 0, data.Length);
        }
    }
}
