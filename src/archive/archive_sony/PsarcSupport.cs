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
        public List<Block> Blocks { get; }

        public override Stream FileData
        {
            get
            {
                if (State != ArchiveFileState.Archived) return base.FileData;

                var ms = new MemoryStream();
                using (var br = new BinaryReaderX(base.FileData, true, ByteOrder.BigEndian))
                {
                    foreach (var block in Blocks)
                    {
                        switch (block.Compression)
                        {
                            case Compression.None:
                                ms.Write(br.ReadBytes((int)block.Size), (int)ms.Position, (int)block.Size);
                                break;
                            case Compression.ZLib:
                                br.BaseStream.Position += 2;
                                using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes((int)block.Size - 2)), CompressionMode.Decompress))
                                    ds.CopyTo(ms);
                                break;
                        }
                    }
                }
                ms.Position = 0;

                return ms;
            }
        }

        public override long? FileSize => Entry.Size;

        public PsarcFileInfo()
        {
            Blocks = new List<Block>();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic Magic;
        public ushort Major;
        public ushort Minor;
        public Magic Compression;
        public int TocSize; // zSize
        public int TocEntrySize;
        public int TocEntryCount;
        public int BlockSize;
        public int ArchiveFlags;

        public string Version => $"v{Major}.{Minor}";
    }

    public sealed class Entry
    {
        public byte[] MD5Hash;
        public uint Index;
        public long Size; // 40 bit (5 bytes)
        public long Offset; // 40 bit (5 bytes)
    }

    public sealed class Block
    {
        public long Size = 0;
        public Compression Compression = Compression.None;
    }

    public enum Compression
    {
        None,
        ZLib,
        Lzma,
        Sdat
    }
}
