using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Komponent.Image.Format;

namespace image_rawJtex
{
    public class Support
    {
        public static Dictionary<int, IImageFormat> Format = new Dictionary<int, IImageFormat>
        {
            [2] = new RGBA(8, 8, 8, 8),
            [3] = new RGBA(8, 8, 8),
            [4] = new RGBA(4, 4, 4, 4)
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RawHeader
    {
        public uint dataStart;
        public int format;
        public int virWidth;
        public int virHeight;
        public int width;
        public int height;
    }
}
