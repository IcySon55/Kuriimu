using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract.Image.Format;
using Kontract.Interface;
using Kontract.IO;
using Kontract;
using System.Drawing;

namespace image_nintendo
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class BNTXImageHeader
    {
        public Magic8 magic;
        public int version;
        public ByteOrder bom;
        public short rev;
        public int filenameAddr; // not used in BCLIM
        public int strAddr;
        public int relocAddr;
        public int fileSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class NXImageHeader
    {
        public Magic magic;
        public int imgCount;
        public long infoPtrAddr;
        public long dataBlkAddr;
        public long dictAddr;
        public long strDictSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class BRTIEntry
    {
        public Magic magic;
        public int size;
        public long size2;
        public int unk1;
        public short unk2;
        public short mipCount;
        public int unk3;
        public int format;
        public int unk4;
        public int width;
        public int height;
        public int unk5;
        public int faceCount;
        public int numChannels;
        public int unk6;
        public int unk7;
        public int unk8;
        public int unk9;
        public int unk10;
        public int unk11;
        public int texDataSize;
        public int blkSize;
        public int compSel;
        public int type;
        public long nameAddr;
        public long unk12;
        public long ptrAddr;
    }

    public class BitmapMeta
    {
        public Bitmap bmp;
        public string name;
        public string format;
    }

    public class BntxSupport
    {
        public static Dictionary<byte, IImageFormat> Formats = new Dictionary<byte, IImageFormat>
        {
            [2] = new RGBA(8, 0, 0, 0),
            [7] = new RGBA(5, 6, 5),
            [9] = new RGBA(8, 8, 0),
            [0xB] = new RGBA(8, 8, 8, 8),
            [0x1A] = new DXT(DXT.Version.DXT1),
            [0x1B] = new DXT(DXT.Version.DXT3),
            [0x1C] = new DXT(DXT.Version.DXT5),
            [0x1D] = new ATI(ATI.Format.ATI1A),
            [0x1E] = new ATI(ATI.Format.ATI2),
        };
    }
}
