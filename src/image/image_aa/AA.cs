using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Kontract.Image;
using Kontract.Image.Format;
using Kontract.IO;

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
                format = new Palette(br.ReadBytes(0x20), new RGBA(5, 5, 5), 4);
                settings = new ImageSettings
                {
                    Width = 256,
                    Height = 192,
                    Format = format
                };

                bmps.Add(Kontract.Image.Common.Load(br.ReadBytes((int)br.BaseStream.Length - 0x20), settings));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                var tileData = Kontract.Image.Common.Save(bmps[0], settings);
                var paletteData = format.paletteBytes;

                bw.Write(paletteData);
                bw.Write(tileData);
            }
        }
    }
}
