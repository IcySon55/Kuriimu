using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Compression;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace image_nintendo
{
    class CHNKTEX
    {
        private Dictionary<Magic, Section> Sections;

        public List<Bitmap> Bitmaps = new List<Bitmap>();
        public bool HasMap { get; } = false;

        public CHNKTEX(Stream input)
        {
            Sections = GetSections(input).ToDictionary(o => o.Magic, o => o);

            var txif = new BinaryReaderX(new MemoryStream(Sections["TXIF"].Data)).ReadStruct<TXIF>();
            var txim = Sections["TXIM"];
            var txpl = Sections["TXPL"];
            Section tx4i;
            HasMap = Sections.TryGetValue("TX4I", out tx4i);

            int width = txif.Width, height = txif.Height;
            var bmp = new Bitmap(width, height);
            var pal = Enumerable.Range(0, txpl.Data.Length / 2).Select(w => ToBGR555(BitConverter.ToInt16(txpl.Data, 2 * w))).ToList();

            if (txif.ImageDepth == TXIMBitDepth.BPP8 || HasMap)
            {
                for (var i = 0; i < width * height; i++)
                {
                    var (x, y, z) = (i % width, i / width, HasMap ? -1 : txim.Data[i]);
                    if (HasMap)
                    {
                        var (a, b, c, d) = (i & -(4 * width), i & (3 * width), i & (width - 4), i & 3);
                        var bits = (txim.Data[a / 4 + b / width + c] >> 2 * d) & 3;
                        var entry = BitConverter.ToInt16(tx4i.Data, x / 4 * 2 + y / 4 * (width / 2));
                        if (entry < 0 || bits < 3) z = 2 * (entry & 0x3FFF) + bits;
                    }
                    bmp.SetPixel(x, y, z == -1 ? Color.Transparent : pal[z]);
                }

                Bitmaps.Add(bmp);
            }
            else if (txif.ImageDepth == TXIMBitDepth.BPP4)
            {
                var accumulator = 0;
                for (var i = 0; i < width * height / 2; i++)
                {
                    var (x, y, z) = ((i + accumulator) % width, (i + accumulator) / width, txim.Data[i] & 0xF);
                    bmp.SetPixel(x, y, pal[z]);
                    accumulator++;
                    (x, y, z) = ((i + accumulator) % width, (i + accumulator) / width, txim.Data[i] >> 4);
                    bmp.SetPixel(x, y, pal[z]);
                }

                Bitmaps.Add(bmp);
            }
            else if (txif.ImageDepth == TXIMBitDepth.BPP2)
            {
                var offset = 0;
                for (var i = 0; i < txif.ImageCount; i++)
                {
                    var accumulator = 0;
                    bmp = new Bitmap(width, height);
                    for (var j = 0; j < (width * height) / 4; j++)
                    {
                        var (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset] & 0x3);
                        bmp.SetPixel(x, y, pal[z]);
                        accumulator++;
                        (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, (txim.Data[j + offset] & 0xF) >> 2);
                        bmp.SetPixel(x, y, pal[z]);
                        accumulator++;
                        (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, (txim.Data[j + offset] & 0x3F) >> 4);
                        bmp.SetPixel(x, y, pal[z]);
                        accumulator++;
                        (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset] >> 6);
                        bmp.SetPixel(x, y, pal[z]);
                    }
                    offset += (width * height) / 4;
                    Bitmaps.Add(bmp);
                }
            }
        }

        private Color ToBGR555(short s) => Color.FromArgb(s % 32 * 33 / 4, (s >> 5) % 32 * 33 / 4, (s >> 10) * 33 / 4);

        private IEnumerable<Section> GetSections(Stream stream)
        {
            using (var br = new BinaryReaderX(stream))
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var chunk = br.ReadStruct<CHNK>();
                    var section = new Section
                    {
                        Chunk = chunk,
                        Magic = br.ReadString(4),
                        Size = br.ReadInt32()
                    };
                    section.Data = br.ReadBytes(section.Size);

                    if (chunk.DecompressedSize != 0)
                        section.Data = LZ11.Decompress(new MemoryStream(section.Data));

                    yield return section;
                }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                // TODO: Save
            }
        }
    }
}
