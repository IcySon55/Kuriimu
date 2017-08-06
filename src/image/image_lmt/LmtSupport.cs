using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace image_lmt
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint unk1;       //gradually incrementing, no static increment value
        public uint unk2;
        public uint unk3;
        public uint unk4;
        public uint unk5;
    }
}
