using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Kontract;
using Cetera.Image;

namespace image_jtex
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic;
        public int unk1;
        public short width;
        public short height;
        public BXLIM.CLIMFormat format;
        public Orientation orientation;
        public short unk2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public int[] unk3;
    }
}
