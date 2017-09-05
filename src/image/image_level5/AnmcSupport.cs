using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_xi.ANMC
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic;
        public uint const1;
        public ushort tmpOff;
        public ushort unk2;
        public uint unk3;
        public uint unk4;

        public uint stringOffset => (uint)tmpOff << 2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public ushort offTmp;
        public ushort entryCount;
        public ushort unk1;
        public ushort entryLength;

        public ushort offset => (ushort)(offTmp << 2);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileMeta
    {
        public uint nameHash;
        public uint nameOffset;
        public uint unk1;
        public uint unk2;
        public uint unk3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SubPart
    {
        public uint nameHash;
        public uint nameOffset;
        public uint unk1;
        public uint const1;
        public uint refHash;    //Hash of filename; e.g. FileMeta

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x33)]
        public float[] Floats;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InfoMeta1
    {
        public uint subPartHash;    //Hash of name of subPart
        public uint nameOffset;
        public uint unk1;
        public uint subPartHash2;   //again
        public uint unkHash;    //Hash of name in previous yet unkown table; e.g. tables[3]

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0xa)]
        public float[] Floats;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InfoMeta2
    {
        public uint hash;
        public ushort nameOffset;
        public ushort unk1;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x6)]
        public float[] Floats;
    }
}
