using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract;

namespace archive_ChibiQp
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public uint hash;
        public int entryDataOffset;
        public int entryDataSize;
        public int unkOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public int tmp1;
        public int fileOffset;
        public int fileSize;

        public byte dirMark => (byte)(tmp1 >> 24);
        public int relNameOffset => tmp1 & 0xFFFFFF;
    }
}
