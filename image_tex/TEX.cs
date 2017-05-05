using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Cetera.Image;

namespace image_tex
{
    class TEX
    {
        public Bitmap Image;
        public ImageSettings settings;
        public Header header;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {

        }

        public TEX(Stream input)
        {

        }

        public void Save(Stream input)
        {

        }
    }
}
