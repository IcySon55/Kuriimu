using System.Runtime.InteropServices;

namespace image_picarg4
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public byte unk1;
        public short unk2;
        public short unk3;
        public short width;
        public short height;
    }
}
