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
                if (State != ArchiveFileState.Archived || Entry.entry.compSize == 0 || Entry.entry.compSize == Entry.entry.decompSize) return base.FileData;
                return new MemoryStream(ZLib.Decompress(base.FileData));
            }
        }

        public override long? FileSize => Entry.entry.decompSize;

        public Tuple<uint, uint> Write(Stream input, uint compOffset, uint decompOffset)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                if (base.FileData.Length > 0)
                {
                    if (Entry.entry.compOffset != 0) Entry.entry.compOffset = compOffset;
                    Entry.entry.decompOffset = decompOffset;
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
                        byte[] comp;
                        if (Entry.entry.compSize != 0 && Entry.entry.compSize != Entry.entry.decompSize)
                            comp = ZLib.Compress(base.FileData);
                        else
                            comp = new BinaryReaderX(base.FileData, true).ReadAllBytes();
                        bw.Write(comp);
                        bw.WriteAlignment();

                        if (Entry.entry.compSize != 0) Entry.entry.compSize = (uint)comp.Length;
                        Entry.entry.decompSize = (uint)base.FileData.Length;
                    }
                }

                return new Tuple<uint, uint>(
                    (Entry.entry.compOffset == 0) ?
                        Entry.entry.decompOffset + Entry.entry.decompSize :
                        Entry.entry.compOffset + Entry.entry.compSize,
                    decompOffset + Entry.entry.decompSize);
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
