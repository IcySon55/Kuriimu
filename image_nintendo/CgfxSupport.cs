using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Contract;

namespace image_nintendo.CGFX
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CgfxHeader
    {
        public Magic magic;
        public ushort byteOrder;
        public ushort headerSize;
        public uint version;
        public uint fileSize;
        public uint entries;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DataEntry
    {
        public uint entryCount;
        public uint offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DictHeader
    {
        public Magic magic;
        public uint dictSize;
        public uint entryCount;
        public uint unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DictEntry
    {
        public uint unk1;
        public ushort unk2;
        public ushort unk3;
        public uint symbolOffset;
        public uint objectOffset;
    }
}
