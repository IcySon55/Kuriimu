using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace image_iobj
{
    public enum Format : int
    {
        RGBA4444 = 1,
        ETC1A4 = 0x20
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int table1Offset;
        public int PtgtOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PTGTHeader
    {
        public Magic magic;
        public int unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PTGTOffEntry
    {
        public int offset;
        public short unk1;
        public short unk2;
    }

    public class PTGTEntry
    {
        public PTGTOffEntry offsetEntry;
        public float[] floats;
    }
}
