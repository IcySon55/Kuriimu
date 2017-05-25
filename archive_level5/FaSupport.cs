﻿using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace archive_fa
{
    public class FAFileInfo : ArchiveFileInfo
    {
        public uint crc32;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FAHeader
    {
        public Magic magic;
        public int offset0;
        public int offset1;
        public int entryOffset;
        public uint nameOffset;
        public uint dataOffset;
        public short unk0;
        public short folderCount;
        public int fileCount;
        public uint infoSecSize; //without header 0x48
        public int zero0;

        public uint unk1;
        public uint unk2;
        public uint unk3;
        public uint unk4;

        public uint unk5;
        public int fileCount2;
        public uint unk6;
        public int zero1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FAEntry
    {
        public uint crc32;
        public uint nameOffsetInFolder;
        public uint fileOffset;
        public uint fileSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FANameEntry
    {
        public uint crc32;
        public string name;
        public uint size;
    }
}
