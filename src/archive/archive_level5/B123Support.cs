using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace archive_level5.B123
{
    public class B123FileInfo : ArchiveFileInfo
    {
        public T0Entry dirEntry;
        public int fileCountInDir;
        public FileEntry fileEntry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint table0Offset;
        public uint table1Offset;
        public uint fileEntriesOffset;
        public uint nameOffset;
        public uint dataOffset;
        public short table0Count;
        public short table1Count;
        public int fileEntriesCount;
        public uint infoSecSize; //without header 0x48
        public int zero1;

        //Hashes?
        public uint unk1;
        public uint unk2;
        public uint unk3;
        public uint unk4;

        public uint dirCount;
        public int fileCount;
        public uint unk6;
        public int zero2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class T0Entry
    {
        public uint dirNameHash;
        public short fileCountInDir;
        public short dirCountInDir;
        public uint firstFileNameOffset;
        public uint fileEntryOffset;
        public uint unk1;               //may have a connection to table1, since the value never exceeds table1Count, but is also smaller
        public uint dirNameOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public uint hash;
        public uint nameOffsetInFolder;
        public uint fileOffset;
        public uint fileSize;
    }
}
