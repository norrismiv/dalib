using DALib.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Text;

namespace DALib.Drawing
{
    public partial class MpfFile
    {
        private List<MpfFrame> _frames;
        private ReadOnlyCollection<MpfFrame> _framesReadOnly;

        public MpfFile(Stream stream)
        {
            Init(stream);
        }

        public MpfFile(DataFileEntry entry) : this(entry.Open())
        {
        }

        public MpfFile(string fileName) : this(File.OpenRead(fileName))
        {
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Size Size => new Size(Width, Height);
        public byte[] UnknownBytes { get; private set; }
        public int StopFrameIndex { get; private set; }
        public int StopFrameCount { get; private set; }
        public int WalkFrameIndex { get; private set; }
        public int WalkFrameCount { get; private set; }
        public int AttackFrameIndex { get; private set; }
        public int AttackFrameCount { get; private set; }
        public int StopMotionFrameCount { get; private set; }
        public int StopMotionProbability { get; private set; }
        public int Attack2StartIndex { get; private set; }
        public int Attack2FrameCount { get; private set; }
        public int Attack3StartIndex { get; private set; }
        public int Attack3FrameCount { get; private set; }
        public int PaletteNumber { get; private set; }

        public ReadOnlyCollection<MpfFrame> Frames => _framesReadOnly;

        public MpfFrame this[int index] => _frames[index];

        private void Init(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                if (reader.ReadInt32() == -1)
                {
                    var unknown = reader.ReadInt32();
                    UnknownBytes = unknown == 4 ? reader.ReadBytes(8) : BitConverter.GetBytes(unknown);
                }
                else
                {
                    UnknownBytes = new byte[0];
                    stream.Seek(-4, SeekOrigin.Current);
                }

                var expectedNumberOfFrames = reader.ReadByte();

                _frames = new List<MpfFrame>();
                _framesReadOnly = new ReadOnlyCollection<MpfFrame>(_frames);

                Width = reader.ReadInt16();
                Height = reader.ReadInt16();

                var expectedDataSize = reader.ReadInt32();

                WalkFrameIndex = reader.ReadByte();
                WalkFrameCount = reader.ReadByte();

                if (reader.ReadInt16() == -1)
                {
                    StopFrameIndex = reader.ReadByte();
                    StopFrameCount = reader.ReadByte();
                    StopMotionFrameCount = reader.ReadByte();
                    StopMotionProbability = reader.ReadByte();
                    AttackFrameIndex = reader.ReadByte();
                    AttackFrameCount = reader.ReadByte();
                    Attack2StartIndex = reader.ReadByte();
                    Attack2FrameCount = reader.ReadByte();
                    Attack3StartIndex = reader.ReadByte();
                    Attack3FrameCount = reader.ReadByte();
                }
                else
                {
                    stream.Seek(-2, SeekOrigin.Current);
                    AttackFrameIndex = reader.ReadByte();
                    AttackFrameCount = reader.ReadByte();
                    StopFrameIndex = reader.ReadByte();
                    StopFrameCount = reader.ReadByte();
                    StopMotionFrameCount = reader.ReadByte();
                    StopMotionProbability = reader.ReadByte();
                }

                var dataStart = stream.Length - expectedDataSize;

                for (var i = 0; i < expectedNumberOfFrames; ++i)
                {
                    var left = reader.ReadInt16();
                    var top = reader.ReadInt16();
                    var right = reader.ReadInt16();
                    var bottom = reader.ReadInt16();
                    var xOffset = reader.ReadInt16(true);
                    var yOffset = reader.ReadInt16(true);
                    var startAddress = reader.ReadInt32();

                    if (left == -1 && top == -1)
                    {
                        PaletteNumber = startAddress;
                        --expectedNumberOfFrames;
                        continue;
                    }

                    var frameWidth = right - left;
                    var frameHeight = bottom - top;

                    byte[] data;

                    if (frameWidth > 0 && frameHeight > 0)
                    {
                        var position = stream.Position;
                        stream.Seek(dataStart + startAddress, SeekOrigin.Begin);
                        data = reader.ReadBytes(frameWidth * frameHeight);
                        stream.Seek(position, SeekOrigin.Begin);
                    }
                    else
                        data = new byte[0];

                    _frames.Add(new MpfFrame(top, left, bottom, right, xOffset, yOffset, data));
                }
            }
        }
    }

    public partial class MpfFile : IEnumerable<MpfFrame>
    {
        public IEnumerator<MpfFrame> GetEnumerator() => ((IEnumerable<MpfFrame>)_frames).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<MpfFrame>)_frames).GetEnumerator();
    }
}
