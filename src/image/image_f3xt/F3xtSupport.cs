using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kontract.Interface;
using Kontract.Image.Format;
using Kontract;

namespace image_f3xt
{
    public class Support
    {
        public static Dictionary<byte, IImageFormat> Format = new Dictionary<byte, IImageFormat>
        {
            [0] = new RGBA(8, 8, 8, 8),
            [1] = new RGBA(8, 8, 8),
            [2] = new RGBA(5, 5, 5, 1),
            [3] = new RGBA(5, 6, 5),
            [4] = new RGBA(4, 4, 4, 4),
            [5] = new LA(8, 8),
            [6] = new HL(8, 8),
            [7] = new LA(8, 0),
            [8] = new LA(0, 8),
            [9] = new LA(4, 4),
            [10] = new LA(4, 0),
            [11] = new LA(0, 4),
            [12] = new ETC1(),
            [13] = new ETC1(true)
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        Magic magic;
        uint texEntries;
        uint flags;
        uint unk1;
        public ushort width;
        public ushort height;
        public uint dataStart;

        public byte format => (byte)(flags & 0xFF);
    }
}
