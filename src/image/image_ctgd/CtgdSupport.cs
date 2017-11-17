using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract;

namespace image_ctgd
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class NNSEntry
    {
        public string magic;
        public int nnsSize;
        public byte[] data;
    }
}
