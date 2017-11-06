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

        public AA(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                settings = new ImageSettings
                {
                    Width = 256,
                    Height = 192,
                    Format = new Palette(br.ReadBytes(0x20), new RGBA(5, 5, 5), 4)
                };

                bmps.Add(Kontract.Image.Image.Load(br.ReadBytes((int)br.BaseStream.Length - 0x20), settings));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                var imgData = Kontract.Image.Image.Save(bmps[0], settings);

                bw.Write(Palette.CreatePalette(bmps[0], new RGBA(5, 5, 5)));
                bw.Write(Kontract.Image.Image.Save(bmps[0], settings));
            }
        }
    }
}
