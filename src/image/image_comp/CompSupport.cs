using System.Runtime.InteropServices;
using Cetera.Image;

namespace image_comp
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public uint dataSize;
        public Format format;
        public byte unk1;
        public short width;
        public short height;
    }
}
