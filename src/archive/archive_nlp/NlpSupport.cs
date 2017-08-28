using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_nlp
{
    public class NLPFileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        // 0x800 padding used in archive
        public uint unk1;
        public uint dataBlockOffset;
        public uint unk3;
        public uint unk4;
        public uint entryCount;
        public uint metaInfEndOffset;
        public uint unk5;
        public uint unk6;
        public uint unk7;
        public uint BlockTableEndOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MetaInfEntry
    {
        public Magic magic;
        public uint zero1;
        public uint fileOffsetInPAK;
        public uint decompSize;
        public uint unk4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class BlockOffsetHeader
    {
        public uint zero1;
        public uint entryCount;
        public uint unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class BlockOffsetEntry
    {
        public uint id;
        public uint blockOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PACKHeader
    {
        //offsets are relative to the PACK section
        public Magic magic;
        public ushort unk1;
        public ushort packFileCount;
        public uint stringSizeOffset;
        public uint stringOffset;
        public uint fileOffset;
        public uint decompSize;
        public uint compSize;
        public uint zero1;
    }
}
