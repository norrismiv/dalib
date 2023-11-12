using System;

namespace DALib.IO;

public class Compression
{
    public static void DecompressHpf(ref Span<byte> buffer)
    {
        // method written by Eru/illuvatar

        uint k = 7;
        uint val = 0;
        uint i;
        uint l = 0;
        var m = 0;

        var hpfSize = buffer.Length;
        var intermediaryBuffer = new byte[hpfSize * 10];
        Span<byte> rawBytes = intermediaryBuffer;

        var intOdd = new uint[256];
        var intEven = new uint[256];
        var bytePair = new byte[513];

        for (i = 0; i < 256; i++)
        {
            intOdd[i] = 2 * i + 1;
            intEven[i] = 2 * i + 2;

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
                } else
                    k++;

                val = (buffer[4 + (int)l - 1] & (1 << (int)k)) != 0 ? intEven[val] : intOdd[val];
            }

            var val3 = val;
            uint val2 = bytePair[val];

            while ((val3 != 0) && (val2 != 0))
            {
                i = bytePair[val2];
                var j = intOdd[i];

                if (j == val2)
                {
                    j = intEven[i];
                    intEven[i] = val3;
                } else
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

            if (val == 0x100)
                continue;

            rawBytes[m] = (byte)val;
            m++;
        }

        buffer = rawBytes[..m];
    }
}