using DALib.Data;
using System;
using System.Drawing;
using System.IO;

namespace DALib.Drawing
{
    public class HpfFile : IRenderable
    {
        private byte[] _data;

        public int Left => 0;
        public int Top => 0;
        public int Width => 28;
        public int Height => _data.Length / Width;
        public byte[] UnknownBytes { get; private set; }
        public byte[] Data => _data;

        public HpfFile(Stream stream)
        {
            Init(stream);
        }
        public HpfFile(DataFileEntry entry)
        {
            using (var stream = entry.Open())
            {
                Init(stream);
            }
        }
        private void Init(Stream stream)
        {
            byte[] decompressedBytes;

            if (stream.ReadByte() == 0x55)
            {
                var compressedBytes = new byte[stream.Length];
                compressedBytes[0] = 0x55;
                stream.Read(compressedBytes, 1, compressedBytes.Length - 1);
                decompressedBytes = Decompress(compressedBytes);
            }
            else
            {
                decompressedBytes = new byte[stream.Length];
                stream.Position--;
                stream.Read(decompressedBytes, 0, decompressedBytes.Length);
            }

            UnknownBytes = new byte[8];
            _data = new byte[decompressedBytes.Length - 8];

            Buffer.BlockCopy(decompressedBytes, 0, UnknownBytes, 0, 8);
            Buffer.BlockCopy(decompressedBytes, 8, _data, 0, _data.Length);
        }
        private byte[] Decompress(byte[] hpfBytes)
        {
            // method written by Eru/illuvatar

            uint k = 7;
            uint val = 0;
            uint i;
            uint l = 0;
            uint m = 0;

            var hpfSize = hpfBytes.Length;

            var rawBytes = new byte[hpfSize * 10];

            var intOdd = new uint[256];
            var intEven = new uint[256];
            var bytePair = new byte[513];

            for (i = 0; i < 256; i++)
            {
                intOdd[i] = (2 * i + 1);
                intEven[i] = (2 * i + 2);

                bytePair[i * 2 + 1] = (byte)i;
                bytePair[i * 2 + 2] = (byte)i;
            }

            while (val != 0x100)
            {
                val = 0;
                while (val <= 0xFF)
                {
                    if (k == 7)
                    {
                        l++;
                        k = 0;
                    }
                    else
                        k++;

                    val = (hpfBytes[4 + l - 1] & (1 << (int)k)) != 0 ? intEven[val] : intOdd[val];
                }

                var val3 = val;
                uint val2 = bytePair[val];

                while (val3 != 0 && val2 != 0)
                {
                    i = bytePair[val2];
                    var j = intOdd[i];

                    if (j == val2)
                    {
                        j = intEven[i];
                        intEven[i] = val3;
                    }
                    else
                        intOdd[i] = val3;

                    if (intOdd[val2] == val3)
                        intOdd[val2] = j;
                    else
                        intEven[val2] = j;

                    bytePair[val3] = (byte)i;
                    bytePair[j] = (byte)val2;
                    val3 = i;
                    val2 = bytePair[val3];
                }

                val += 0xFFFFFF00;

                if (val == 0x100) continue;
                rawBytes[m] = (byte)val;
                m++;
            }

            var finalData = new byte[m];
            Buffer.BlockCopy(rawBytes, 0, finalData, 0, (int)m);

            return finalData;
        }
    }
}
