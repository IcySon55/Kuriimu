using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Image;
using Kontract.Image.Swizzle;
using Kontract.Image.Format;
using Kontract.IO;

namespace image_ctgd
{
    public class CTGD
    {
        public List<Bitmap> bmps = new List<Bitmap>();
        public ImageSettings settings;
        Palette format;

        List<NNSEntry> entries = new List<NNSEntry>();

        public CTGD(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                var width = br.ReadInt16();
                var height = br.ReadUInt16();

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var magic = br.ReadString(8);
                    var size = br.ReadInt32();

                    entries.Add(new NNSEntry
                    {
                        magic = magic,
                        nnsSize = size,
                        data = br.ReadBytes(size - 12)
                    });
                }

                format = new Palette(entries.Find(e => e.magic == "nns_pcol").data, new RGBA(5, 5, 5), 8);
                settings = new ImageSettings
                {
                    Width = width,
                    Height = height,
                    Format = format
                };

                bmps.Add(Kontract.Image.Image.Load(entries.Find(e => e.magic == "nns_txel").data, settings));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                settings.Width = bmps[0].Width;
                settings.Height = bmps[0].Height;

                var texData = Kontract.Image.Image.Save(bmps[0], settings);
                var palData = format.paletteBytes;

                if (palData.Length > 0x200) throw new System.Exception("Your image contains more than 256 colors. The format doesn't allow more than 256 colors.");

                //Header
                bw.Write((short)bmps[0].Width);
                bw.Write((short)bmps[0].Height);

                var entry = entries.Find(e => e.magic == "nns_frmt");
                bw.WriteASCII(entry.magic);
                bw.Write(entry.nnsSize);
                bw.Write(entry.data);

                //image data
                bw.WriteASCII("nns_txel");
                bw.Write(texData.Length + 0xc);
                bw.Write(texData);

                //palette data
                bw.WriteASCII("nns_pcol");
                bw.Write(0x20c);
                bw.Write(palData);
                bw.WritePadding(0x20c - (palData.Length + 0xc), 0xff);

                //end tag
                bw.WriteASCII("nns_endb");
                bw.Write(0xc);
            }
        }
    }
}
