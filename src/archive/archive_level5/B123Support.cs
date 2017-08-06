using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_level5.B123
{
    public class B123FileInfo : ArchiveFileInfo
    {
        public FileEntry entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint offset1;
        public uint offset2;
        public uint fileEntriesOffset;
        public uint nameOffset;
        public uint dataOffset;
        public short table1Count;
        public short table2Count;
        public int fileEntriesCount;
        public uint infoSecSize; //without header 0x48
        public int zero1;

        //Hashes?
        public uint unk1;
        public uint unk2;
        public uint unk3;
        public uint unk4;

        public uint unk5;
        public int fileCount;
        public uint unk6;
        public int zero2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public uint crc32;
        public uint nameOffsetInFolder;
        public uint fileOffset;
        public uint fileSize;
    }
}
