using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.Image.Swizzle;
using Komponent.Image;
using Komponent.Image.Format;
using Komponent.IO;

/*Nintendo DS Banner*/
namespace image_bnr
{
    public class BNR
    {
        Header header;
        byte[] titleInfo;
        public ImageSettings settings;
        Palette format;

        public List<Bitmap> bmps = new List<Bitmap>();

        public BNR(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();
                br.BaseStream.Position += 0x1c;

                byte[] tileData = br.ReadBytes(0x200);
                format = new Palette(4);
                format.SetPaletteFormat(new RGBA(5, 5, 5));
                format.SetPaletteColors(br.ReadBytes(0x20));
                settings = new ImageSettings
                {
                    Width = 32,
                    Height = 32,
                    Format = format,
                    Swizzle = new NitroSwizzle(32, 32)
                };
                bmps.Add(Common.Load(tileData, settings));

                titleInfo = br.ReadBytes(0x600);
            }
        }

        public void Save(string filename, Import imports)
        {
            if (bmps[0].Width != 32 || bmps[0].Height != 32) throw new System.Exception("Banner needs to be 32x32");

            uint GetInt(byte[] ba) => ba.Aggregate(0u, (i, b) => (i << 8) | b);

            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                var tileData = Common.Save(bmps[0], settings);
                var paletteData = format.GetPaletteBytes();

                List<byte> result = new List<byte>();
                result.AddRange(tileData);
                result.AddRange(paletteData);
                result.AddRange(titleInfo);
                var crc16 = GetInt(imports.crc16.Create(result.ToArray(), 0));
                header.crc16 = (ushort)crc16;

                bw.WriteStruct(header);
                bw.WritePadding(0x1c);
                bw.Write(result.ToArray());
            }
        }
    }
}
