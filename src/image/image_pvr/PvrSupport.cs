using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kontract;
using Kontract.Interface;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;
using System.Drawing;

namespace image_pvr
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int flags;
        public long format;
        public Kontract.Image.Support.PVRTC.ColourSpace colorSpace;
        public Kontract.Image.Support.PVRTC.VariableType channelType;
        public int height;
        public int width;
        public int depth;
        public int surfaceCount;
        public int facesCount;
        public int mipmapCount;
        public int metaSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Meta
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string fourCC;
        public int key;
        public int dataSize;
    }

    public class Support
    {
        public static Dictionary<long, IImageFormat> Formats = new Dictionary<long, IImageFormat>
        {
            [0] = new PVRTC(PVRTC.Format.PVRTC_2bpp),
            [1] = new PVRTC(PVRTC.Format.PVRTCA_2bpp),
            [2] = new PVRTC(PVRTC.Format.PVRTC_4bpp),
            [3] = new PVRTC(PVRTC.Format.PVRTCA_4bpp),
            [4] = new PVRTC(PVRTC.Format.PVRTC2_2bpp),
            [5] = new PVRTC(PVRTC.Format.PVRTC2_4bpp),
            [6] = new ETC1(false, false),
            [7] = new DXT(DXT.Version.DXT1),
            //[8] = DXT2,
            [9] = new DXT(DXT.Version.DXT3),
            //[10] = DXT4,
            [11] = new DXT(DXT.Version.DXT5),
            [12] = new ATI(ATI.Format.ATI1L),
            [13] = new ATI(ATI.Format.ATI2),
            //[14]=BC6
            //[15]=BC7
            //[16]=UYVY
            //[17]=YUY2
            //[18]=BW1bpp
            //[19]=RGBE9995
            //[20]=RGBG8888
            //[21]=GRGB8888
            [22] = new ETC2(ETC2.Format.ETC2),
            [23] = new ETC2(ETC2.Format.ETC2A),
            [24] = new ETC2(ETC2.Format.ETC2A1),
            //[25]=EAC R11
            //[26]=EAC RG11
            //[27-50]=ASTC
        };

        public static List<int> formatToSwizzle = new List<int> {
            0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15
        };

        public class BlockSwizzle : IImageSwizzle
        {
            private MasterSwizzle _swizzle;

            public int Width { get; }
            public int Height { get; }

            public BlockSwizzle(int width, int height)
            {
                Width = (width + 3) & ~3;
                Height = (height + 3) & ~3;

                _swizzle = new MasterSwizzle(Width, new Point(0, 0), new[] { (1, 0), (2, 0), (0, 1), (0, 2) });
            }

            public Point Get(Point point) => _swizzle.Get(point.Y * Width + point.X);
        }
    }
}
