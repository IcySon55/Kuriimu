using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace image_aif
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TexInfo
    {
        public ushort width;
        public ushort height;
        public byte format; 
    }
}
