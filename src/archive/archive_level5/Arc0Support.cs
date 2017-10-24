using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace archive_level5.ARC0
{
    public class ARC0FileInfo : ArchiveFileInfo
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
        public short tble2Count;
        public int fileEntriesCount;
        public uint unk1;
        public int zero1;

        //Hashes?
        public uint unk2;
        public uint unk3;
        public uint unk4;
        public uint unk5;

        public uint unk6;
        public int fileCount;
        public uint unk7;
        public int zero2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public uint crc32; //only filename.ToLower()
        public uint nameOffsetInFolder;
        public uint fileOffset;
        public uint fileSize;
    }
}
