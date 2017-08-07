using System.Runtime.InteropServices;

namespace image_bnr
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public short version;
        public ushort crc16;
    }
}
