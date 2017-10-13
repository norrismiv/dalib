using System;
using System.Security.Cryptography;
using System.Text;

namespace DALib.Cryptography
{
    public class PacketCryptoProvider
    {
        private const int MinimumSeed = 0;
        private const int MaximumSeed = 9;
        private const int DefaultSeed = 0;
        private const int SaltLength = 256;
        private const int KeystreamLength = 9;
        private const string DefaultKeystream = "UrkcnItnI";
        private const int Keystream2TableLength = 1024;
        private const int BufferLength = 65532;

        private byte _seed;
        private byte[] _salt;
        private byte[] _keystream1;
        private byte[] _keystream2;
        private byte[] _keystream2Table;

        private MD5 _md5;
        private uint _randState;
        private byte[] _buffer;

        public PacketCryptoProvider() : this(DefaultSeed, DefaultKeystream)
        {
            _keystream1[3] = 0xE5; // The default keystream is supposed to be UrkcnItnI,
            _keystream1[7] = 0xA3; // however Kru somehow managed to fuck that up. :-)
        }

        public PacketCryptoProvider(byte seed, string keystream)
        {
            if (seed < MinimumSeed || seed > MaximumSeed)
                throw new ArgumentOutOfRangeException(nameof(seed));

            if (keystream == null)
                throw new ArgumentNullException(nameof(keystream));

            var keystreamBytes = Encoding.ASCII.GetBytes(keystream);

            if (keystreamBytes.Length != KeystreamLength)
                throw new ArgumentOutOfRangeException(nameof(keystream));

            _seed = seed;
            _keystream1 = keystreamBytes;
            _keystream2 = new byte[KeystreamLength];
            _keystream2Table = new byte[Keystream2TableLength];
            GenerateSalt(seed);

            _md5 = MD5.Create();
            _randState = 1;
            _buffer = new byte[BufferLength];
        }

        public byte Seed
        {
            get => _seed;

            set
            {
                if (_seed != value)
                {
                    _seed = value;
                    GenerateSalt(value);
                }
            }
        }

        public string Keystream
        {
            get => Encoding.ASCII.GetString(_keystream1);

            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                var bytes = Encoding.ASCII.GetBytes(value);

                if (bytes.Length != KeystreamLength)
                    throw new Exception($"Keystream must be {KeystreamLength} characters long");

                _keystream1 = bytes;
            }
        }

        public void GenerateKeystream2Table(string name)
        {
            var table = GetMD5String(GetMD5String(name));
            for (var i = 0; i < 31; ++i)
            {
                table += GetMD5String(table);
            }
            _keystream2Table = Encoding.ASCII.GetBytes(table);
        }

        public byte[] EncryptClientData(byte[] data, int offset, int count, byte sequence, bool useKeystream2)
        {
            if (offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset + count > data.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var resultLength = 0;

            _buffer[resultLength++] = data[offset];
            _buffer[resultLength++] = sequence;
            Buffer.BlockCopy(data, offset + 1, _buffer, 2, count - 1);
            resultLength += count - 1;

            _buffer[resultLength++] = 0;

            if (useKeystream2)
                _buffer[resultLength++] = data[offset];

            var keystream2Seed = NextRandState(ref _randState);
            var a = (ushort)((ushort)keystream2Seed % 65277 + 256);
            var b = (byte)(((keystream2Seed & 0xFF0000) >> 16) % 155 + 100);

            GenerateKeystream2(a, b);

            Transform(_buffer, 2, count - 1, useKeystream2 ? _keystream2 : _keystream1, sequence);

            var hash = _md5.ComputeHash(_buffer, 0, resultLength);
            _buffer[resultLength++] = hash[13];
            _buffer[resultLength++] = hash[3];
            _buffer[resultLength++] = hash[11];
            _buffer[resultLength++] = hash[7];

            a ^= 0x7470;
            b ^= 0x23;

            _buffer[resultLength++] = (byte)a;
            _buffer[resultLength++] = b;
            _buffer[resultLength++] = (byte)(a >> 8);

            var result = new byte[resultLength];
            Buffer.BlockCopy(_buffer, 0, result, 0, resultLength);
            return result;
        }

        public byte[] EncryptServerData(byte[] data, int offset, int count, byte sequence, bool useKeystream2)
        {
            if (offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset + count > data.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var resultLength = 0;

            _buffer[resultLength++] = data[offset];
            _buffer[resultLength++] = sequence;
            Buffer.BlockCopy(data, offset + 1, _buffer, 2, count - 1);
            resultLength += count - 1;

            var keystream2Seed = NextRandState(ref _randState);
            var a = (ushort)((ushort)keystream2Seed % 65277 + 256);
            var b = (byte)(((keystream2Seed & 0xFF0000) >> 16) % 155 + 100);

            GenerateKeystream2(a, b);

            Transform(_buffer, 2, count - 1, useKeystream2 ? _keystream2 : _keystream1, sequence);

            a ^= 0x6474;
            b ^= 0x24;

            _buffer[resultLength++] = (byte)a;
            _buffer[resultLength++] = b;
            _buffer[resultLength++] = (byte)(a >> 8);

            var result = new byte[resultLength];
            Buffer.BlockCopy(_buffer, 0, result, 0, resultLength);
            return result;
        }

        public byte[] DecryptClientData(byte[] data, int offset, int count, bool useKeystream2)
        {
            if (offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset + count > data.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            _buffer[0] = data[0];
            Buffer.BlockCopy(data, offset + 2, _buffer, 1, count - 9);

            var resultLength = offset + count - 3;

            var a = (ushort)(data[resultLength + 2] << 8 | data[resultLength]);
            var b = data[resultLength + 1];

            a ^= 0x7470;
            b ^= 0x23;

            resultLength -= 4; // hash bytes

            GenerateKeystream2(a, b);

            Transform(_buffer, 1, count - 9, useKeystream2 ? _keystream2 : _keystream1, data[offset + 1]);

            if (useKeystream2)
                --resultLength; // opcode

            --resultLength; // trailing 0

            var result = new byte[resultLength];
            Buffer.BlockCopy(_buffer, 0, result, 0, resultLength);
            return result;
        }

        public byte[] DecryptServerData(byte[] data, int offset, int count, bool useKeystream2)
        {
            if (offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset + count > data.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var a = (ushort)(data[count - 1] << 8 | data[count - 3]);
            var b = data[count - 2];

            a ^= 0x6474;
            b ^= 0x24;

            _buffer[0] = data[offset];
            Buffer.BlockCopy(data, offset + 2, _buffer, 1, count - 5);

            GenerateKeystream2(a, b);

            Transform(_buffer, 1, count - 5, useKeystream2 ? _keystream2 : _keystream1, data[offset + 1]);

            var result = new byte[count - 4];
            Buffer.BlockCopy(_buffer, 0, result, 0, count - 4);
            return result;
        }

        private void GenerateSalt(byte seed)
        {
            _salt = new byte[SaltLength];

            var saltByte = 0;

            for (var i = 0; i < SaltLength; ++i)
            {
                switch (seed)
                {
                    case 0:
                        saltByte = i;
                        break;
                    case 1:
                        saltByte = (i % 2 != 0 ? -1 : 1) * ((i + 1) / 2) + 128;
                        break;
                    case 2:
                        saltByte = 255 - i;
                        break;
                    case 3:
                        saltByte = (i % 2 != 0 ? -1 : 1) * ((255 - i) / 2) + 128;
                        break;
                    case 4:
                        saltByte = i / 16 * (i / 16);
                        break;
                    case 5:
                        saltByte = 2 * i % 256;
                        break;
                    case 6:
                        saltByte = 255 - 2 * i % 256;
                        break;
                    case 7:
                        if (i > 127)
                            saltByte = 2 * i - 256;
                        else
                            saltByte = 255 - 2 * i;
                        break;
                    case 8:
                        if (i > 127)
                            saltByte = 511 - 2 * i;
                        else
                            saltByte = 2 * i;
                        break;
                    case 9:
                        saltByte = 255 - (i - 128) / 8 * ((i - 128) / 8) % 256;
                        break;
                }

                saltByte |= (saltByte << 8) | ((saltByte | (saltByte << 8)) << 16);
                _salt[i] = (byte)saltByte;
            }
        }

        private void GenerateKeystream2(ushort a, byte b)
        {
            for (var i = 0; i < KeystreamLength; ++i)
            {
                _keystream2[i] = _keystream2Table[(i * (KeystreamLength * i + b * b) + a) % Keystream2TableLength];
            }
        }

        private uint NextRandState(ref uint state)
        {
            state = state * 0x343FD + 0x269EC3;
            return (state >> 0x10) & 0x7FFF;
        }

        private string GetMD5String(string value)
        {
            var valueBytes = Encoding.ASCII.GetBytes(value);
            var hashBytes = _md5.ComputeHash(valueBytes);
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
        }

        private void Transform(byte[] buffer, int offset, int count, byte[] keystream, byte sequence)
        {
            if (offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (var i = 0; i < count; ++i)
            {
                buffer[i + offset] ^= _salt[sequence];
                buffer[i + offset] ^= keystream[i % KeystreamLength];

                var saltIndex = (i / KeystreamLength) % SaltLength;

                if (saltIndex != sequence)
                    buffer[i + offset] ^= _salt[saltIndex];
            }
        }
    }
}
