using System.ComponentModel;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace image_mt.Mobile
{
    public enum Version : short
    {
        v9 = 9
    }

    public enum Format : byte
    {

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
        //4-bits - R3

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
        public byte r3;

        //Block3
        public byte unk3;
        public Format format;
        public short width;
        public short height;
    }

    public sealed class MobileMTTexBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        public Format format { get; set; }
    }
}
