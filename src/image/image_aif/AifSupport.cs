using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace image_aif
{
    public enum Format : byte
    {
        L8 = 2,
        RGBA4444 = 6, LA88, RGBA8888,
        LA44 = 0x22,
        RGB888 = 0x24, ETC1, ETC1A4
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TexInfo
    {
        public ushort width;
        public ushort height;
        public Format format;
    }
}
