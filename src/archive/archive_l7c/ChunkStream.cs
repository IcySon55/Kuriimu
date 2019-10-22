using System;
using System.IO;
using System.Linq;
using Kontract.IO;

namespace archive_l7c
{
    class ChunkStream : Stream
    {
        private Stream _baseStream;

        private ChunkInfo[] _chunks;
        private Stream[] _decodedChunks;

        private long _length;
        private long _position;
        private int _currentChunk;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public ChunkStream(Stream baseStream, int length, params ChunkInfo[] chunks)
        {
            _baseStream = baseStream;

            foreach (var chunk in chunks)
            {
                if (chunk.Offset < 0 || chunk.Offset >= _baseStream.Length)
                    throw new InvalidOperationException("One chunk doesn't fit into the baseStream.");
                if (chunk.Offset + chunk.Length > _baseStream.Length)
                    throw new InvalidOperationException("One chunk doesn't fit into the baseStream.");
            }

            _chunks = chunks;
            _decodedChunks = new Stream[chunks.Length];
            _length = length;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _position = offset;
                    break;
                case SeekOrigin.Current:
                    _position += offset;
                    break;
                case SeekOrigin.End:
                    _position = _length + offset;
                    break;
            }

            if (_position >= _length)
                _currentChunk = -1;
            else
                _currentChunk = GetChunkByPosition(_position);

            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = (int)Math.Min(count, _length - _position);
            if (_currentChunk < 0 || _currentChunk >= _chunks.Length)
                return 0;

            var originalPosition = _baseStream.Position;
            var readBytes = 0;

            DecodeCurrentChunk();

            // Read either complete chunks or until the end of a chunk
            var relativePosition = GetChunkRelativePosition();
            while (_currentChunk < _chunks.Length && relativePosition + count >= _chunks[_currentChunk].Length)
            {
                var length = (int)(_chunks[_currentChunk].Length - relativePosition);
                ReadBufferFromChunk(buffer, offset, length, _currentChunk, relativePosition);

                _position += length;
                count -= length;
                offset += length;
                readBytes += length;
                relativePosition = 0;
                _currentChunk++;

                DecodeCurrentChunk();
            }

            // Read the remaining data not concluding with a chunk
            ReadBufferFromChunk(buffer, offset, count, _currentChunk, relativePosition);

            Seek(_position + count, SeekOrigin.Begin);
            readBytes += count;

            _baseStream.Position = originalPosition;

            return readBytes;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        private void DecodeCurrentChunk()
        {
            if (_currentChunk < 0 || _currentChunk >= _chunks.Length)
                return;

            if (_decodedChunks[_currentChunk] == null && _chunks[_currentChunk].Decoder != null)
            {
                // Decompress chunk when read the first time only
                _decodedChunks[_currentChunk] = new MemoryStream();
                _chunks[_currentChunk].Decoder.Decode(
                    new SubStream(_baseStream, _chunks[_currentChunk].Offset, _chunks[_currentChunk].Length),
                    _decodedChunks[_currentChunk]);

                _chunks[_currentChunk].Length = _decodedChunks[_currentChunk].Length;
            }
        }

        private void ReadBufferFromChunk(byte[] buffer, int offset, int length, int chunk, long relativePosition)
        {
            if (_currentChunk < 0 || _currentChunk >= _chunks.Length)
                return;

            if (_decodedChunks[chunk] != null)
            {
                _decodedChunks[chunk].Position = relativePosition;
                _decodedChunks[chunk].Read(buffer, offset, length);
            }
            else
            {
                _baseStream.Position = GetAbsolutePositionByChunk(chunk);
                _baseStream.Read(buffer, offset, length);
            }
        }

        private int GetChunkByPosition(long position)
        {
            var chunkId = 0;
            while (position >= _chunks[chunkId].Length)
                position -= _chunks[chunkId++].Length;

            return chunkId;
        }

        private long GetAbsolutePositionByChunk(int chunk)
        {
            if (_currentChunk < 0 || _currentChunk >= _chunks.Length)
                return -1;

            var summedLength = 0L;
            for (int i = 0; i < chunk; i++)
                summedLength += _chunks[i].Length;

            return _chunks[chunk].Offset + (_position - summedLength);
        }

        private long GetChunkRelativePosition()
        {
            var summedLength = 0L;
            for (int i = 0; i < _currentChunk; i++)
                summedLength += _chunks[i].Length;

            return _position - summedLength;
        }
    }
}