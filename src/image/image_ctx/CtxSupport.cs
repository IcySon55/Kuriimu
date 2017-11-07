using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Kontract;
using Kontract.Interface;
using Kontract.Image.Format;

namespace image_ctx
{
    public class Support
    {
        public static Dictionary<uint, IImageFormat> Format = new Dictionary<uint, IImageFormat>
        {
            [0x6752] = new RGBA(8, 8, 8, 8),
            [0x6754] = new RGBA(8, 8, 8),
            [0x6756] = new LA(0, 8),
            [0x6757] = new LA(8, 0),
            [0x6758] = new LA(4, 4),
            [0x675A] = new ETC1(),
            [0x675B] = new ETC1(true)
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic8 magic;
        public uint width;
        public uint height;
        public uint width2;
        public uint height2;
        public uint unk1;
        public uint format;
        public uint unk2;
        public uint dataSize;
    }
}
