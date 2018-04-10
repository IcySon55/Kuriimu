using System;
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
        public Entry Entry { get; set; }
        public int BlockSize { get; set; }
        public List<int> BlockSizes { get; set; }

        public override Stream FileData
        {
            get
            {
                if (State != ArchiveFileState.Archived) return base.FileData;

                var ms = new MemoryStream();
                using (var br = new BinaryReaderX(base.FileData, true, ByteOrder.BigEndian))
                {
                    br.BaseStream.Position = Entry.Offset;

                    for (var i = Entry.FirstBlockIndex; i < Entry.FirstBlockIndex + Math.Ceiling((double)Entry.UncompressedSize / BlockSize); i++)
                    {
                        if (BlockSizes[i] == 0)
                            ms.Write(br.ReadBytes(BlockSize), 0, BlockSize);
                        else
                        {
                            var compression = br.ReadUInt16();
                            br.BaseStream.Position -= 2;

                            var blockStart = br.BaseStream.Position;
                            if (compression == PSARC.ZLibHeader)
                            {
                                br.BaseStream.Position += 2;
                                using (var ds = new DeflateStream(br.BaseStream, CompressionMode.Decompress, true))
                                    ds.CopyTo(ms);
                                br.BaseStream.Position = blockStart + BlockSizes[i];
                            }
                            // TODO: Add LZMA decompression support
                            else
                                ms.Write(br.ReadBytes(BlockSizes[i]), 0, BlockSizes[i]);
                        }
                    }
                }
                ms.Position = 0;

                return ms;
            }
        }

        public Stream BaseFileData => base.FileData;

        public override long? FileSize => Entry.UncompressedSize;

        public PsarcFileInfo()
        {
            BlockSizes = new List<int>();
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
        public ArchiveFlags ArchiveFlags;

        public string Version => $"v{Major}.{Minor}";
    }

    public sealed class Entry
    {
        public byte[] MD5Hash;
        public int FirstBlockIndex;
        public long UncompressedSize; // 40 bit (5 bytes)
        public long Offset; // 40 bit (5 bytes)
    }

    public enum ArchiveFlags
    {
        RelativePaths = 0,
        IgnoreCasePaths = 1,
        AbsolutePaths = 2
    }
}
