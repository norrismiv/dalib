using DALib.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace DALib.Drawing
{
    public partial class EPFFile
    {
        private List<EPFFrame> _frames;
        private ReadOnlyCollection<EPFFrame> _framesReadOnly;

        public EPFFile(Stream stream) => Init(stream);

        public EPFFile(DataFileEntry entry)
        {
            using (var stream = entry.Open())
            {
                Init(stream);
            }
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public byte[] UnknownBytes { get; private set; }

        public ReadOnlyCollection<EPFFrame> Frames => _framesReadOnly;

        public EPFFrame this[int index] => _frames[index];

        private void Init(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                stream.Seek(0, SeekOrigin.Begin);

                var expectedNumberOfFrames = reader.ReadInt16();
                Width = reader.ReadInt16();
                Height = reader.ReadInt16();
                UnknownBytes = reader.ReadBytes(2);
                var tocAddress = reader.ReadInt32() + 12;

                _frames = new List<EPFFrame>();
                _framesReadOnly = new ReadOnlyCollection<EPFFrame>(_frames);

                for (var i = 0; i < expectedNumberOfFrames; ++i)
                {
                    stream.Seek(tocAddress + i * 16, SeekOrigin.Begin);

                    var top = reader.ReadInt16();
                    var left = reader.ReadInt16();
                    var bottom = reader.ReadInt16();
                    var right = reader.ReadInt16();

                    var width = right - left;
                    var height = bottom - top;

                    var startAddress = reader.ReadInt32() + 12;
                    var endAddress = reader.ReadInt32() + 12;

                    stream.Seek(startAddress, SeekOrigin.Begin);

                    byte[] data;

                    if (endAddress - startAddress == width * height)
                        data = reader.ReadBytes(endAddress - startAddress);
                    else
                        data = reader.ReadBytes(tocAddress - startAddress);

                    _frames.Add(new EPFFrame(top, left, bottom, right, data));
                }
            }
        }
    }

    public partial class EPFFile : IEnumerable<EPFFrame>
    {
        public IEnumerator<EPFFrame> GetEnumerator() => ((IEnumerable<EPFFrame>)_frames).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<EPFFrame>)_frames).GetEnumerator();
    }
}
