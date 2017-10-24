using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract;
using Kontract.IO;

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

    #region Positioning
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PositionHeader
    {
        public uint hash;
        public ushort unk1;
        public ushort unk2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PositionEntry
    {
        public float x1;
        public float y1;
        public float z1;

        public float x2;
        public float y2;
        public float z2;
    }

    public class Position
    {
        public PositionHeader infoMeta;
        public PositionEntry values;

        public int width => (int)(values.x2 + -1 * values.x1);
        public int height => (int)(values.y2 + -1 * values.y1);
    }
    #endregion

    #region Center
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CenterHeader
    {
        public uint hash;
        public ushort unk1;
        public ushort unk2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CenterEntry
    {
        public float x;
        public float y;
        public float z;
    }

    public class Center
    {
        public CenterHeader infoMeta;
        public CenterEntry values;
    }
    #endregion

    #region MetaInf4
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MetaInf4T
    {
        public uint hash;
        public ushort unk1;
        public ushort unk2;
    }

    public class MetaInf4
    {
        public MetaInf4T infoMeta;
        public float[] values;
    }
    #endregion
}
