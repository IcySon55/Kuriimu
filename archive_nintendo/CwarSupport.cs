using System.Runtime.InteropServices;
using Cetera;
using Kuriimu.Contract;

namespace archive_nintendo.CWAR
{
    public class CWARFileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public ByteOrder byteOrder;
        public short headerSize;
        public uint version;
        public uint fileSize;
        public int partitionCount;
        public int infoId;  //0x6800
        public int infoOffset;
        public int infoSize;
        public int dataId;  ////0x6801
        public int dataOffset;
        public uint dataSize;
        public int infxId;  //0x6802
        public uint infxOffset;
        public int infxSize;
        public int zero0;
        public int zero1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PartitionHeader
    {
        public Magic magic;
        public uint size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class InfoEntry
    {
        public int unk0; //const?
        public uint dataOffset;
        public uint dataSize;
    }
}
