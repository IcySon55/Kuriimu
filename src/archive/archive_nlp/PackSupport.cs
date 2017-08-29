using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using System.IO;
using Kuriimu.Compression;
using System;
using Kuriimu.IO;

namespace archive_nlp.PACK
{
    public class PACKFileInfo : ArchiveFileInfo
    {
        public FileEntry Entry;

        public override Stream FileData
        {
            get
            {
                if (State != ArchiveFileState.Archived) return base.FileData;
                return new MemoryStream(ZLib.Decompress(base.FileData));
            }
        }

        public override long? FileSize => Entry.entry.decompSize;

        public Tuple<uint, uint> Write(Stream input, uint compOffset, uint uncompOffset)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                if (base.FileData.Length > 0)
                {
                    Entry.entry.compOffset = compOffset;
                    Entry.entry.decompOffset = uncompOffset;
                }

                if (State == ArchiveFileState.Archived)
                {
                    base.FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment();
                }
                else
                {
                    if (base.FileData.Length > 0)
                    {
                        var comp = ZLib.Compress(base.FileData);
                        bw.Write(comp);
                        bw.WriteAlignment();

                        Entry.entry.compSize = (uint)comp.Length;
                        Entry.entry.decompSize = (uint)base.FileData.Length;
                    }
                }

                return new Tuple<uint, uint>(compOffset + Entry.entry.compSize, uncompOffset + Entry.entry.decompSize);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PACKHeader
    {
        public Magic magic;
        public ushort unk1;
        public ushort packFileCount;
        public uint stringOffOffset;
        public uint stringOffset;
        public uint fileOffset;
        public uint decompSize;
        public uint compSize;
        public uint zero1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PACKEntry
    {
        public Magic magic;
        public uint zero1;
        public uint decompSize;
        public uint decompOffset;
        public uint zero2;
        public uint unk1;
        public uint compSize;
        public uint compOffset;
    }

    public class FileEntry
    {
        public PACKEntry entry;
        public int nameOffset;
    }
}
