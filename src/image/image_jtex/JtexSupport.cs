using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Komponent.Image.Format;
using Kontract.Interface;
using Komponent.IO;

namespace image_jtex
{
    public class Support
    {
        public static Dictionary<byte, IImageFormat> Format = new Dictionary<byte, IImageFormat>
        {
            [8] = new ETC1(),
            [11] = new ETC1(true)
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic;
        public int fileSize;
        public short width;
        public short height;
        public byte format;
        public byte orientation;
        public short unk2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public int[] unk3;
    }
}