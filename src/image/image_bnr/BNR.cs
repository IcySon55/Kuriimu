using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CeteraDS.Hash;
//using Kontract.Image;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
//using Kontract.Image.Format;
using Kontract.Image.Swizzle;
using Kontract.Interface;
using Kontract.IO;

/*Nintendo DS Banner*/
namespace image_bnr
{
    public class BNR
    {
        BnrImport Imports = new BnrImport();

        Header header;
        byte[] titleInfo;
        public ImageSettings2 settings;
        //Palette format;

        public List<Bitmap> bmps = new List<Bitmap>();

        public BNR(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();
                br.BaseStream.Position += 0x1c;

                byte[] tileData = br.ReadBytes(0x200);
                //format = new Palette(br.ReadBytes(0x20), new RGBA(5, 5, 5), 4);
                settings = new ImageSettings2
                {
                    Width = 32,
                    Height = 32,
                    Format = Imports.RGBA,
                    Swizzle = new NitroSwizzle(32, 32)
                };
                bmps.Add(Imports.Common.Load(tileData, settings));

                titleInfo = br.ReadBytes(0x600);
            }
        }

        public void Save(string filename)
        {
            if (bmps[0].Width != 32 || bmps[0].Height != 32) throw new System.Exception("Banner needs to be 32x32");

            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                var tileData = Imports.Common.Save(bmps[0], settings);
                var paletteData = new byte[0];//format.paletteBytes;

                List<byte> result = new List<byte>();
                result.AddRange(tileData);
                result.AddRange(paletteData);
                result.AddRange(titleInfo);
                var crc16 = Crc16.Create(result.ToArray());
                header.crc16 = (ushort)crc16;

                bw.WriteStruct(header);
                bw.WritePadding(0x1c);
                bw.Write(result.ToArray());
            }
        }
    }
}
