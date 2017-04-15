using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Contract;
using Cetera.Image;

namespace archive_fa
{
    public class FAFileInfo : ArchiveFileInfo
    {
        public Entry Entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic;
        public int offset0;
        public int offset1;
        public int entryOffset;
        public uint nameOffset;
        public uint dataOffset;
        public short unk0;
		public short folderCount;
        public int entryCount;
        public uint infoSecSize; //without header 0x48
        public int zero0;

        public uint unk1;
        public uint unk2;
        public uint unk3;
        public uint unk4;

        public uint unk5;
        public int entryCount2;
        public uint unk6;
        public int zero1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public uint crc32;
        public uint nameOffset;
        public uint fileOffset;
        public uint fileSize;
    }
}
