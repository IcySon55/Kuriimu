using System.ComponentModel;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace image_nintendo
{
    public enum ChnkTexFormat : byte
    {
        BGR555
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class CHNK
    {
        public Magic Magic;
        public int DecompressedSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TXIF
    {
        public int ChunkSize;
        public int Unk1;
        public int ImageSize;
        public int MapSize;
        public int PaletteSize;
        public short Width;
        public short Height;
        public int Unk5;
    }

    public sealed class ChnkTexBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        public ChnkTexFormat Format { get; set; }
    }
}
