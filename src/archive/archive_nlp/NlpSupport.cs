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
        public uint unk2;
        public uint unk3;
        public uint unk4;
        public uint fileCount;
        public uint entryTable1Size;
        public uint unk5;
        public uint unk6;
        public uint unk7;
        public uint entryTable2Size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public Magic magic;
        public uint unk1;
        public uint unk2;
        public uint unk3;
        public uint unk4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry2Header
    {
        public uint unk1;
        public uint entryCount;
        public uint unk2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry2
    {
        public uint unk1;
        public uint id;
    }
}
