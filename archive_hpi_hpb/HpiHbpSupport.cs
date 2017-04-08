using System;
using System.IO;
using System.Runtime.InteropServices;
using Cetera.Compression;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_hpi_hpb
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HpiHeader
    {
        public Magic magic;
        public int zero0;
        public int headerSize;  //without magic
        public int zero1;
        public short zero2;
        public short infoCount;
        public int entryCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HashEntry
    {
        public short entryOffset;
        public ushort entryCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public uint stringOffset;
        public uint fileOffset;
        public uint fileSize;
        public uint uncompressedSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmpHeader
    {
        public Magic magic;
        public int compressedSize;
        public int headerSize;
        public int zero;
        public uint uncompressedSize;
        public int padding0, padding1, padding2; // equal to 0x01234567
    }

    public class HashSort : IComparable<HashSort>
    {
        public Entry Entry;
        public string FileName;
        public uint Hash;

        public int CompareTo(HashSort rhs) => Hash.CompareTo(rhs.Hash);
    }

    public class HpiHpbAfi : ArchiveFileInfo
    {
        public Entry Entry;

        public override Stream FileData => base.FileData ?? GetUncompressedStream();

        public Stream GetUncompressedStream()
        {
            FileData.Position = 0;
            if (Entry.uncompressedSize == 0) return FileData;
            using (var br = new BinaryReaderX(FileData, true))
            {
                var header = br.ReadStruct<AcmpHeader>();
                return new MemoryStream(RevLZ77.Decompress(br.ReadBytes(header.compressedSize), header.uncompressedSize));
            }
        }

        public Stream GetCompressedStream(Stream fileData)
        {
            fileData.Position = 0;
            if (Entry.uncompressedSize == 0) return fileData;
            using (var br = new BinaryReaderX(fileData))
            {
                Stream compData = new MemoryStream(RevLZ77.Compress(br.ReadBytes((int)fileData.Length)));
                return compData;
            }
        }
    }
}