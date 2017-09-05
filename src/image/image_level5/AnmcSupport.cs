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
    public struct SubPartT
    {
        public uint nameHash;
        public uint nameOffset;
        public uint unk1;
        public uint const1;
        public uint refHash;    //Hash of filename; e.g. FileMeta
    }

    public class SubPart
    {
        public SubPartT subPart;
        public float[] floats;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InfoMeta1T
    {
        public uint subPartHash;    //Hash of name of subPart
        public uint nameOffset;
        public uint unk1;
        public uint subPartHash2;   //again
        public uint unkHash;    //Hash of name in previous yet unkown table; e.g. tables[3]
    }

    public class InfoMeta1
    {
        public InfoMeta1T infoMeta;
        public float[] floats;
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
