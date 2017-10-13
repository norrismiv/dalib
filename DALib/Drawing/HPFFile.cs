using DALib.Data;
using System;
using System.Drawing;
using System.IO;

namespace DALib.Drawing
{
    public class HPFFile
    {
        private byte[] _data;

        public HPFFile(Stream stream) => Init(stream);

        public HPFFile(DataFileEntry entry)
        {
            using (var stream = entry.Open())
            {
                Init(stream);
            }
        }

        public int Width => 28;

        public int Height => _data.Length / Width;

        public byte[] UnknownBytes { get; private set; }

        public Bitmap Render(Palette palette) => palette.Render(_data, Width, Height);

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
            uint val2 = 0;
            uint val3 = 0;
            uint i = 0;
            uint j = 0;
            uint l = 0;
            uint m = 0;

            var hpfSize = hpfBytes.Length;

            var rawBytes = new byte[hpfSize * 10];

            var int_odd = new uint[256];
            var int_even = new uint[256];
            var byte_pair = new byte[513];

            for (i = 0; i < 256; i++)
            {
                int_odd[i] = (2 * i + 1);
                int_even[i] = (2 * i + 2);

                byte_pair[i * 2 + 1] = (byte)i;
                byte_pair[i * 2 + 2] = (byte)i;
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

                    if ((hpfBytes[4 + l - 1] & (1 << (int)k)) != 0)
                        val = int_even[val];
                    else
                        val = int_odd[val];
                }

                val3 = val;
                val2 = byte_pair[val];

                while (val3 != 0 && val2 != 0)
                {
                    i = byte_pair[val2];
                    j = int_odd[i];

                    if (j == val2)
                    {
                        j = int_even[i];
                        int_even[i] = val3;
                    }
                    else
                        int_odd[i] = val3;

                    if (int_odd[val2] == val3)
                        int_odd[val2] = j;
                    else
                        int_even[val2] = j;

                    byte_pair[val3] = (byte)i;
                    byte_pair[j] = (byte)val2;
                    val3 = i;
                    val2 = byte_pair[val3];
                }

                val += 0xFFFFFF00;

                if (val != 0x100)
                {
                    rawBytes[m] = (byte)val;
                    m++;
                }
            }

            var finalData = new byte[m];
            Buffer.BlockCopy(rawBytes, 0, finalData, 0, (int)m);

            return finalData;
        }
    }
}
