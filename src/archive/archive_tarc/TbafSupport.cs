using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace archive_tarc
{
    public class TBAFFileInfo : ArchiveFileInfo
    {
        public Entry entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint fileSize;
        public uint fileCount;
        public uint unk1;
        public uint unk2;
        public uint entryOffset;
        public uint entrySecSize;
        public uint nameOffset;
        public uint nameSecOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public ushort id;
        public byte unk1;
        public byte unk2;
        public uint nameOffset;
        public uint dataOffset;
        public uint dataSize;
        public uint unk3;
        public uint unk4;
    }
}
