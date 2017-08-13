using System.ComponentModel;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using CeteraDS.Image;

namespace image_nintendo
{
    public enum TXPLFormat : byte
    {
        BGR555
    }

    public enum TXIMBitDepth
    {
        BPP8 = 8,
        BPP4 = 4,
        BPP2 = 2,
        L8 = 0
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CHNK
    {
        public Magic magic;
        public int decompSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TXIF
    {
        public short Unk1;
        public short Unk2;
        public int imgDataSize;
        public int mapDataSize;
        public int paletteDataSize;
        public short Width;
        public short Height;
        public short ImageCount;
        public short Unk3;
    }

    public class Section
    {
        public CHNK chunk;
        public Magic magic;
        public int size;
        public byte[] data;
    }

    public sealed class ChnkTexBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        [ReadOnly(true)]
        public TXIMBitDepth IndexBitDepth { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public TXPLFormat PaletteFormat { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public bool TX4I { get; set; }
    }
}
