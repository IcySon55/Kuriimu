using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Komponent.IO;
using System.Drawing;

namespace image_spr3
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public uint const1;
        public uint const2;
        public Magic magic;
        public uint headerSize;
        public uint unk1;
        public ushort unk2;
        public ushort entryCount;
        public uint dataValOffset;
        public uint entryOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint zero1;
        public uint entryOffset;
    }

    public class SPR3Entry
    {
        public Entry entry;
        public byte[] data;
    }
}
