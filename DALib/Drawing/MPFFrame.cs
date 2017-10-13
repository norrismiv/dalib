using System;
using System.Drawing;

namespace DALib.Drawing
{
    public class MPFFrame
    {
        private byte[] _data;

        public MPFFrame(int top, int left, int bottom, int right, int xOffset, int yOffset, byte[] data)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
            XOffset = xOffset;
            YOffset = yOffset;
            _data = new byte[data.Length];
            Buffer.BlockCopy(data, 0, _data, 0, data.Length);
        }

        public int Top { get; }

        public int Left { get; }

        public int Bottom { get; }

        public int Right { get; }

        public int XOffset { get; }

        public int YOffset { get; }

        public int Width => Right - Left;

        public int Height => Bottom - Top;

        public byte this[int index] => _data[index];

        public Bitmap Render(Palette palette) => palette.Render(_data, Width, Height);
    }
}
