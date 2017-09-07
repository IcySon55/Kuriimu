using System.ComponentModel;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace image_cvt
{
    public enum Format : short
    {
        ETC1A4 = 0x1006
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public ushort Magic;
        public short Width;
        public short Height;
        public Format Format;
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
        public Format Format { get; set; }
    }
}
