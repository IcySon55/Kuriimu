using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Kontract;

namespace image_nintendo.NCGLR
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GenericHeader
    {
        public Magic magic;
        public ushort byteOrder;
        public ushort unk1;
        public uint secSize;
        public ushort headerSize;
        public ushort subSecCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TTLPHeader
    {
        public Magic magic;
        public uint secSize;
        public uint bitDepth;   //3 - 4bit, 4 - 8bit
        public uint zero1;
        public uint dataSize;
        public uint cpp;    //Colours per Palette
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PMCPHeader
    {
        public Magic magic;
        public uint secSize;
        public ushort palCount;
        public uint unk1;
        public ushort unk2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CHARHeader
    {
        public Magic magic;
        public uint secSize;
        public ushort tileCount;
        public ushort tileSize;
        public uint bitDepth;   //3 - 4bit, 4 - 8bit
        public long zero1;
        public uint dataSize;
        public uint unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SOPCHeader
    {
        public Magic magic;
        public uint secSize;
        public uint zero1;
        public ushort tileSize;
        public ushort tileCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class NRCSHeader
    {
        public Magic magic;
        public uint secSize;
        public ushort width;
        public ushort height;
        public uint zero1;
        public uint dataSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CEBKHeader
    {
        public Magic magic;
        public uint secSize;
        public uint imgCount;
        public uint unk1;
        public uint boundSize;  //multiplied by 64
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CEBKEntry
    {
        public ushort cellCount;
        public ushort unk1;
        public uint cellOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TileEntry
    {
        public byte rowData;
        public byte cellWidth;
        public byte colData;
        public byte cellHeight;
        public ushort tileOffset;
    }

    public class KBECImage
    {
        public CEBKEntry entry;
        public List<TileEntry> tileEntry;
        public List<byte[]> tiles;

    }

    public class NCLR
    {
        public GenericHeader header;
        public TTLPHeader ttlpHeader;
        public PMCPHeader pmcpHeader;
        public List<ushort> palID;
    }

    public class NCGR
    {
        public GenericHeader header;
        public CHARHeader charHeader;
        public SOPCHeader sopcHeader;
    }

    public class NRCS
    {
        public GenericHeader header;
        public NRCSHeader nrcsHeader;
    }

    public class NCER
    {
        public GenericHeader header;
        public NRCSHeader ncerHeader;
    }

    public enum MapType : byte
    {
        None, SCR, CER
    }
}
