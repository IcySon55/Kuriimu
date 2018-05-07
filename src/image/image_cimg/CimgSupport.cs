using System.Runtime.InteropServices;
using Kontract;
using System.Collections.Generic;
using Kontract.Interface;

namespace image_cimg
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint paletteOffset;

        public short unkOffset1;
        public short unkCount1;     //Size 0x8
        public short unkOffset2;
        public short unkCount2;     //Size 0xc

        public short tileDataOffset;
        public short tileEntryCount;    //Size 0x2
        public short imageDataOffset;
        public short imageTileCount;    //Size 0x40

        public short imgFormat;
        public short colorCount;
        public short width;
        public short height;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] unk2;
    }
}
