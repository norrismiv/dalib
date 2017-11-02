using DALib.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace DALib.Drawing
{
    public partial class EpfFile
    {
        private List<EpfFrame> _frames;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] UnknownBytes { get; private set; }
        public ReadOnlyCollection<EpfFrame> Frames { get; private set; }

        public EpfFile(Stream stream)
        {
            Init(stream);
        }

        public EpfFile(DataFileEntry entry)
        {
            using (var stream = entry.Open())
            {
                Init(stream);
            }
        }

        public EpfFrame this[int index] => _frames[index];

        private void Init(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, false))
            {
                var expectedNumberOfFrames = reader.ReadInt16();
                Width = reader.ReadInt16();
                Height = reader.ReadInt16();
                UnknownBytes = reader.ReadBytes(2);
                var tocAddress = reader.ReadInt32() + 12;

                _frames = new List<EpfFrame>();
                Frames = new ReadOnlyCollection<EpfFrame>(_frames);

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

                    var data = endAddress - startAddress == width * height ? reader.ReadBytes(endAddress - startAddress) : reader.ReadBytes(tocAddress - startAddress);

                    _frames.Add(new EpfFrame(top, left, bottom, right, data));
                }
            }
        }
    }

    public partial class EpfFile : IEnumerable<EpfFrame>
    {
        public IEnumerator<EpfFrame> GetEnumerator() => ((IEnumerable<EpfFrame>)_frames).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<EpfFrame>)_frames).GetEnumerator();
    }
}
