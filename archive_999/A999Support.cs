using System;
using System.Runtime.InteropServices;
using System.IO;
using Kuriimu.Contract;

namespace archive_999
{
    public class A999FileInfo : ArchiveFileInfo
    {
        public Entry Entry;
    }

    internal class XorStream : Stream
    {
        Stream baseStream;
        byte[] xorBytes;

        public XorStream(Stream input, uint xor)
        {
            baseStream = input ?? throw new ArgumentNullException(nameof(input));
            xorBytes = BitConverter.GetBytes(xor);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var orig = (byte)Position;
            int read = baseStream.Read(buffer, offset, count);
            for (int i = orig; i < orig + read; i++)
                buffer[offset + i - orig] ^= (byte)(i ^ xorBytes[i % 4]);
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override bool CanRead => baseStream.CanRead;
        public override bool CanSeek => baseStream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => baseStream.Length;
        public override long Position { get => baseStream.Position; set => baseStream.Position = value; }
        public override void Flush() => baseStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => baseStream.Seek(offset, origin);
        public override void SetLength(long value) => baseStream.SetLength(value);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic;
        public int hashTableOffset;
        public int fileEntryOffset;
        public long dataOffset;
        public long infoSecSize;
        public int hold0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TableHeader
    {
        public int tableSize;
        public int entryCount;
        public long hold0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DirectoryEntry
    {
        public uint directoryHash;
        public int fileCount;
        public int unk1;
        public uint hold0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public long fileOffset;
        public uint XORpad;
        public long fileSize;
        public uint XORID;
        public short directoryHashID;
        public short const0;
        public uint hold0;
    }
}
