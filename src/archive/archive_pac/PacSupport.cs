using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kontract;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archive_pac
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int unk1;
        public int unk2;
        public int fileCount;
        public int entryOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public Magic magic;
        public int entrySize;
        public int fileSize;
        public int fileOffset;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x80)]
        public string fileName;
    }
}
