using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace image_bnr
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public short version;
        public ushort crc16;
    }
}
