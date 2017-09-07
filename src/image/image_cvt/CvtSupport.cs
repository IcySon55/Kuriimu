using System.Runtime.InteropServices;

namespace image_cvt
{
    public enum Format : short
    {
        ETC1A4 = 0x1006
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public ushort magic;
        public ushort width;
        public ushort height;
        public Format format;
        public uint unkn0;
        public ulong strAsc0;
        public ulong strAsc1;
        public ulong strAsc3;
        public ulong strAsc4;
        public ulong unkn1;
        public ulong unkn2;
        public ulong unkn3;
        public ulong unkn4;
        public uint unkn5;
        

    }
}
