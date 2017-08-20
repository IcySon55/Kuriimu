using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Kontract;

namespace image_ctx
{
    public enum Format : uint
    {
        RGBA8888 = 0x6752,
        RGB888 = 0x6754,
        A8 = 0x6756, L8, LA44,
        ETC1 = 0x675A, ETC1A4
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic8 magic;
        public uint width;
        public uint height;
        public uint width2;
        public uint height2;
        public uint unk1;
        public Format format;
        public uint unk2;
        public uint dataSize;
    }
}
