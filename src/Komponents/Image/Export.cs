using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract.IO;
using Image.Format;

namespace Image
{
    public class Formats
    {
        //RGBA
        [Export("RGBA1010102", typeof(IImageFormat))]
        IImageFormat rgb1010102 => new RGBA(10, 10, 10, 2);
        [Export("RGBA8888", typeof(IImageFormat))]
        IImageFormat rgba8888 => new RGBA(8, 8, 8, 8);
        [Export("RGBA4444", typeof(IImageFormat))]
        IImageFormat rgba4444 => new RGBA(4, 4, 4, 4);
        [Export("RGBA5551", typeof(IImageFormat))]
        IImageFormat rgba5551 => new RGBA(5, 5, 5, 1);
        [Export("ABGR2101010", typeof(IImageFormat))]
        IImageFormat abgr2101010 => new RGBA(10, 10, 10, 2, ByteOrder.LittleEndian);
        [Export("RGBA8888", typeof(IImageFormat))]
        IImageFormat abgr8888 => new RGBA(8, 8, 8, 8, ByteOrder.LittleEndian);
        [Export("RGBA4444", typeof(IImageFormat))]
        IImageFormat abgr4444 => new RGBA(4, 4, 4, 4, ByteOrder.LittleEndian);
        [Export("RGBA5551", typeof(IImageFormat))]
        IImageFormat abgr1555 => new RGBA(5, 5, 5, 1, ByteOrder.LittleEndian);
        [Export("RGB888", typeof(IImageFormat))]
        IImageFormat rgb888 => new RGBA(8, 8, 8);
        [Export("RGB565", typeof(IImageFormat))]
        IImageFormat rgb565 => new RGBA(5, 6, 5);
        [Export("BGR888", typeof(IImageFormat))]
        IImageFormat bgr888 => new RGBA(8, 8, 8, 0, ByteOrder.LittleEndian);
        [Export("BGR565", typeof(IImageFormat))]
        IImageFormat bgr565 => new RGBA(5, 6, 5, 0, ByteOrder.LittleEndian);

        //LA
        [Export("LA88", typeof(IImageFormat))]
        IImageFormat la88 => new LA(8, 8);
        [Export("LA44", typeof(IImageFormat))]
        IImageFormat la44 => new LA(4, 4);
        [Export("AL88", typeof(IImageFormat))]
        IImageFormat al88 => new LA(8, 8, ByteOrder.LittleEndian);
        [Export("AL44", typeof(IImageFormat))]
        IImageFormat al44 => new LA(4, 4, ByteOrder.LittleEndian);
        [Export("L8", typeof(IImageFormat))]
        IImageFormat l8 => new LA(8, 0);
        [Export("L4", typeof(IImageFormat))]
        IImageFormat l4 => new LA(4, 0);
        [Export("A8", typeof(IImageFormat))]
        IImageFormat a8 => new LA(0, 8);
        [Export("A4", typeof(IImageFormat))]
        IImageFormat a4 => new LA(0, 4);

        //HL
        [Export("HL88", typeof(IImageFormat))]
        IImageFormat hl88 => new HL(8, 8);

        //ETC
        [Export("ETC1", typeof(IImageFormat))]
        IImageFormat etc1 => new ETC1();
        [Export("ETC1A4", typeof(IImageFormat))]
        IImageFormat etc1a4 => new ETC1(true);

        //DXT
        [Export("DXT1", typeof(IImageFormat))]
        IImageFormat dxt1 => new DXT(DXT.Version.DXT1);
        [Export("DXT3", typeof(IImageFormat))]
        IImageFormat dxt3 => new DXT(DXT.Version.DXT3);
        [Export("DXT5", typeof(IImageFormat))]
        IImageFormat dxt5 => new DXT(DXT.Version.DXT5);

        //ATI
        [Export("ATI1L", typeof(IImageFormat))]
        IImageFormat ati1l => new ATI(ATI.Format.ATI1L);
        [Export("ATI1A", typeof(IImageFormat))]
        IImageFormat ati1a => new ATI(ATI.Format.ATI1A);
        [Export("ATI2", typeof(IImageFormat))]
        IImageFormat ati2 => new ATI(ATI.Format.ATI2);

        //Palette
        [Export("Palette_4Bit", typeof(IImagePalette))]
        IImageFormat pal4b => new Palette(4);
        [Export("Palette_8Bit", typeof(IImagePalette))]
        IImageFormat pal8b => new Palette(8);
    }
}
