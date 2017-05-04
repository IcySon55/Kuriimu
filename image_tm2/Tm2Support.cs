using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace image_tm2
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileHeader
    {
        public Magic8 magic; //PNGFILE2
        public int width;
        public int height;
        public int unk1;
        public int unk2;
        public int pngFileSize;
    }
}
