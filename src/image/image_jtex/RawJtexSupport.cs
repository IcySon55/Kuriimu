using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Cetera.Image;

namespace image_rawJtex
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RawHeader
    {
        public uint dataStart;
        public Format format;
        public int virWidth;
        public int virHeight;
        public int width;
        public int height;
    }

    public enum Format : uint
    {
        RGBA8888 = 2, RGB888, RGBA4444
    }
}
