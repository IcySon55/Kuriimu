using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace archive_arc0
{
    public class ARC0FileInfo : ArchiveFileInfo
    {
        public uint crc32;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ARC0Header
    {
        public Magic magic;
        public int offset0;
        public int offset1;
        public int offset2;
        public int nameOffset;
        public int dataOffset;
        public short unk0;
        public short folderCount;
        public int fileCount;
        public uint unk1;
        public int zero0;

        public uint unk2;
        public uint unk3;
        public uint unk4;
        public uint unk5;

        public uint unk6;
        public int fileCount2;
        public uint unk7;
        public int zero1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ARC0Entry
    {
        public uint crc32; //only filename.ToLower()
        public uint nameOffsetInFolder;
        public uint fileOffset;
        public uint fileSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ARC0NameEntry
    {
        public uint crc32;
        public string name;
        public uint size;
    }
}
