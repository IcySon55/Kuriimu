using System;
using System.Drawing;
using System.IO;
using Kuriimu.IO;

/*Original functions and understanding by xdaniel and his tool Tharsis
 * https://github.com/xdanieldzd/Tharsis */

namespace image_tmx
{
    public class TMX
    {
        public Bitmap bmp;

        public Header header;
        public string comment;
        public Color[] Palette;

        public TMX(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input))
            {
                header = br.ReadStruct<Header>();
                comment = br.ReadString(0x1c);

                switch (header.imageFormat)
                {
                    case TMXPixelFormat.PSMT8:
                        bmp = TmxSupport.ConvertIndexed8(br, header, out Palette);
                        break;
                    case TMXPixelFormat.PSMT4:
                        bmp = TmxSupport.ConvertIndexed4(br, header, out Palette);
                        break;
                    case TMXPixelFormat.PSMCT16:
                        bmp = TmxSupport.Convert16(br, header);
                        break;
                    case TMXPixelFormat.PSMCT24:
                        bmp = TmxSupport.Convert24(br, header);
                        break;
                    case TMXPixelFormat.PSMCT32:
                        bmp = TmxSupport.Convert32(br, header);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public void Save(string filename)
        {
            Color[] colors = TmxSupport.GetPalette(bmp);
            byte[] picData = null;
            byte[] paletteData = null;

            if (colors.Length == 16)
            {
                picData = TmxSupport.CreateIndexed4(bmp, colors);
                paletteData = TmxSupport.GetPaletteBytes(colors);

                header.imageFormat = TMXPixelFormat.PSMT4;
                header.paletteFormat = TMXPixelFormat.PSMCT32;
            }
            else if (colors.Length == 256)
            {
                picData = TmxSupport.CreateIndexed8(bmp, colors);
                paletteData = TmxSupport.GetPaletteBytes(colors);

                header.imageFormat = TMXPixelFormat.PSMT8;
                header.paletteFormat = TMXPixelFormat.PSMCT32;
            }
            else
            {
                picData = TmxSupport.Create32(bmp);

                header.imageFormat = TMXPixelFormat.PSMCT32;
            }

            header.height = (ushort)bmp.Height;
            header.width = (ushort)bmp.Width;

            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                bw.WriteStruct(header);
                bw.WriteASCII(comment);
                bw.BaseStream.Position = 0x40;
                if (paletteData != null) bw.Write(paletteData);
                bw.Write(picData);
            }
        }
    }
}
