using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Kontract;
using Kontract.Interface;
using Kontract.IO;

namespace archive_sony
{
    public class PsarcFileInfo : ArchiveFileInfo
    {
        public int ID;
        public Compression Compression;
        public Entry Entry { get; set; }

        public override Stream FileData
        {
            // TODO: Decompress the SubStream by block
            get { return null; }
        }

        public List<long> Blocks { get; }

        public override long? FileSize => Entry.Size;

        public PsarcFileInfo()
        {
            Blocks = new List<long>();
        }

        public byte[] DecompressZLib(Stream inData)
        {
            using (var br = new BinaryReaderX(inData, true))
            {
                var ms = new MemoryStream();
                br.BaseStream.Position = 2;
                using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes((int)br.BaseStream.Length - 6)), CompressionMode.Decompress))
                    ds.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic Magic;
        public ushort Major;
        public ushort Minor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string Compression;
        public int TocSize; // zSize
        public int TocEntrySize;
        public int TocEntryCount;
        public int BlockSize;
        public uint ArchiveFlags;

        public string Version => $"v{Major}.{Minor}";
    }

    public sealed class Entry
    {
        public byte[] MD5Hash;
        public uint Index;
        public long Size; // 40 bit (5 bytes)
        public long Offset; // 40 bit (5 bytes)
    }

    public enum Compression
    {
        ZLib,
        LZMA,
        Sdat
    }
}
