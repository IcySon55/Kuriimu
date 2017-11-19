using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Komponent.IO;
using Komponent.Image;
using Komponent.Image.Format;

namespace image_aa
{
    public class AA
    {
        public List<Bitmap> bmps = new List<Bitmap>();
        public ImageSettings settings;
        Palette format;

        public AA(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                format = new Palette(4);
                format.SetPaletteFormat(new RGBA(5, 5, 5));
                format.SetPaletteColors(br.ReadBytes(0x20));
                settings = new ImageSettings
                {
                    Width = 256,
                    Height = 192,
                    Format = format
                };

                bmps.Add(Common.Load(br.ReadBytes((int)br.BaseStream.Length - 0x20), settings));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                var tileData = Common.Save(bmps[0], settings);
                var paletteData = format.GetPaletteBytes();

                bw.Write(paletteData);
                bw.Write(tileData);
            }
        }
    }
}
