using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archive_xc2
{
    public class XorStream : Stream
    {
        public byte[] Key { get; set; }

        public int KeySize => Key.Length;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _stream.Length;

        public override long Position { get => _stream.Position; set => Seek(value, SeekOrigin.Begin); }

        private Stream _stream;

        public XorStream(Stream input, string key, Encoding enc) : this(input, enc.GetBytes(key))
        {
        }

        public XorStream(Stream input, byte[] key)
        {
            _stream = input;

            Key = new byte[key.Length];
            Array.Copy(key, Key, key.Length);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new InvalidDataException($"Buffer is too small.");

            var length = (int)Math.Max(0, Math.Min(count, Length - Position));

            var keyPos = Position % KeySize;
            for (int i = 0; i < length; i++)
            {
                buffer[offset + i] = (byte)(Key[keyPos++] ^ _stream.ReadByte());
                if (keyPos >= KeySize)
                    keyPos = 0;
            }

            return length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new InvalidDataException($"Buffer is too small.");

            var keyPos = Position % KeySize;
            for (int i = 0; i < count; i++)
            {
                _stream.WriteByte((byte)(buffer[offset + i] ^ Key[keyPos++]));
                if (keyPos >= KeySize)
                    keyPos = 0;
            }
        }
    }
}
