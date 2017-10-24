using System.Runtime.InteropServices;
using Kontract;

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
