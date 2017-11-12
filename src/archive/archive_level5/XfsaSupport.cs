using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace archive_level5.XFSA
{
    public class XFSAFileInfo : ArchiveFileInfo
    {
        public FileEntry entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint offset1;
        public uint offset2;
        public uint fileEntryTableOffset;
        public uint nameTableOffset;
        public uint dataOffset;
        public ushort table1EntryCount;
        public ushort table2EntryCount;
        public uint fileEntryCount;
        public uint unk4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Table1Entry
    {
        public uint hash;
        public uint unk1;
        public uint unk2;
        public uint unk3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Table2Entry
    {
        public uint hash;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public uint crc32;  //lower case filename hash
        public uint comb1; //offset combined with an unknown value, offset is last 24 bits with 4bit left-shift
        public uint comb2;   //size combined with an unknown value, size is last 20 bits
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class NameEntry
    {
        public uint offset; //relative to nameTableOffset
        public uint name;
    }
}
