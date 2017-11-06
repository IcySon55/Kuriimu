using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kontract.Image.Format;
using Kontract.Interface;

namespace image_cvt
{
    public class Support
    {
        public static Dictionary<short, IImageFormat> Format = new Dictionary<short, IImageFormat>
        {
            [0x1006] = new ETC1(true)
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public ushort Magic;
        public short Width;
        public short Height;
        public short Format;
        public int Unknown1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string Name;
        public int Unknown2;
        public int Unknown3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1C)]
        public byte[] Padding;
    }

    public sealed class CvtBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        [ReadOnly(true)]
        public string Format { get; set; }
    }
}
