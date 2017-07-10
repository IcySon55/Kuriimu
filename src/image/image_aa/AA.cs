using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using CeteraDS.Image;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_aa
{
    public class AA
    {
        //[StructLayout(LayoutKind.Sequential, Pack = 1)]

        public List<Bitmap> bmps = new List<Bitmap>();

        public AA(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                var palette = Common.GetPalette(br.ReadBytes(0x20), Format.BGR555);

                var settings = new ImageSettings
                {
                    Width = 256,
                    Height = 192,
                    BitPerIndex = BitLength.Bit4,
                    PadToPowerOf2 = false
                };

                bmps.Add(Common.Load(br.ReadBytes((int)br.BaseStream.Length - 0x20), settings, palette));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                for (int i = 0; i < bmps.Count; i++)
                {
                    var settings = new ImageSettings
                    {
                        Width = 256,
                        Height = 192,
                        BitPerIndex = BitLength.Bit4,
                        PadToPowerOf2 = false
                    };

                    bw.Write(Common.EncodePalette(Common.CreatePalette(bmps[i]), Format.BGR555));

                    bw.Write(Common.Save(bmps[i], settings, Common.CreatePalette(bmps[i])));
                }
            }
        }
    }
}
