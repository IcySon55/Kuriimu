using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Kontract.Interface;
using Komponent.IO;

namespace image_nintendo.CHNK
{
    public class Import
    {
        [Import("Nintendo")]
        public ICompressionCollection nintendo;

        public Import()
        {
            var catalog = new DirectoryCatalog("Komponents");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }

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
        public Magic Magic;
        public int DecompressedSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TXIF
    {
        public short Unk1;
        public short Unk2;
        public int ImageSize;
        public int MapSize;
        public int PaletteSize;
        public short Width;
        public short Height;
        public short ImageCount;
        public short Unk3;
    }

    public class Section
    {
        public CHNK Chunk;
        public Magic Magic;
        public int Size;
        public byte[] Data;
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
