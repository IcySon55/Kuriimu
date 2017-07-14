using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CeteraDS.Image;
using Kuriimu.Kontract;
using Kuriimu.IO;
using CeteraDS.Hash;

/*Nintendo DS Banner*/

namespace image_bnr
{
    public class BNR
    {
        Header header;
        byte[] titleInfo;

        public List<Bitmap> bmps = new List<Bitmap>();

        public BNR(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();
                br.BaseStream.Position += 0x1c;

                byte[] tileData = br.ReadBytes(0x200);
                var palette = Common.GetPalette(br.ReadBytes(0x20), Format.BGR555);
                var settings = new ImageSettings
                {
                    Width = 32,
                    Height = 32,
                    BitPerIndex = BitLength.Bit4
                };
                bmps.Add(Common.Load(tileData, settings, palette));

                titleInfo = br.ReadBytes(0x600);
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                List<byte> result = new List<byte>();
                var settings = new ImageSettings
                {
                    Width = 32,
                    Height = 32,
                    BitPerIndex = BitLength.Bit4
                };

                var palette = Common.CreatePalette(bmps[0]);
                var paletteB = Common.EncodePalette(palette, Format.BGR555);
                var tileData = Common.Save(bmps[0], settings, palette);

                result.AddRange(tileData);
                result.AddRange(paletteB);
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
