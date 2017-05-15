using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Contract;

namespace image_tim2
{
    public class Tim2Support
    {
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public short bitsPerPlane;
        public short nrOfPlanes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint planeSize;
        public long unk1;
        public long unk2;
        public ushort width;
        public ushort height;
        public long unk3;
        public int unk4;
    }
}
