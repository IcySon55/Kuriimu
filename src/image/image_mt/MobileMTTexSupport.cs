using System.ComponentModel;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;
using Kontract.Image.Format;
using System.Collections.Generic;

namespace image_mt.Mobile
{
    public enum Version : short
    {
        v9 = 9
    }

    public class Support
    {
        public static Dictionary<int, IImageFormat> Format = new Dictionary<int, IImageFormat>
        {
            [1] = new RGBA(8, 8, 8, 8, Kontract.IO.ByteOrder.BigEndian),
            [0x31] = new DXT(DXT.Version.DXT5, false, Kontract.IO.ByteOrder.BigEndian),
            [0x20]=new RGBA(8,8,8,8,Kontract.IO.ByteOrder.BigEndian)
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic Magic;
        public uint Block1;
        //8-bits - unk1
        //8-bits - unk2
        //16-bits - version

        public uint Block2;
        //24-bits - unused
        //4-bits - R1
        //4-bits - mipMapCount

        public uint Block3;
        //2-bits - unk4
        //4-bits - unk5
        //13-bits - width
        //13-bits - height
    }

    public class HeaderInfo
    {
        //Block 1
        public byte unk1;
        public byte unk2;
        public Version version;

        //Block 2
        public byte r1;
        public byte mipMapCount;

        //Block3
        public int format;
        public short width;
        public short height;
    }

    public sealed class MobileMTTexBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        [ReadOnly(true)]
        public string Format { get; set; }
    }
}
