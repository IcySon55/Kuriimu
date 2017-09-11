using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace image_iobj
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int table1Offset;
        public int PtgtOffset;
        public int table2Offset;
        public int image1Offset;
        public int image2Offset;
    }
}
