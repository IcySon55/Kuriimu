using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kontract.Interface;
using Komponent.Image.Format;
using Komponent.IO;

namespace image_iobj
{
    public class Support
    {
        public static Dictionary<int, IImageFormat> Format = new Dictionary<int, IImageFormat>()
        {
            [4] = new RGBA(8, 8, 8, 8),
            [5] = new RGBA(4, 4, 4, 4),
            [18] = new ETC1(true)
        };
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
