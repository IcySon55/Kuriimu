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
        public TXIF TXIF { get; }
        public TXIMBitDepth BitDepth { get; }
        public bool HasMap { get; } = false;

        public CHNKTEX(Stream input)
        {
            Sections = GetSections(input).ToDictionary(o => o.Magic, o => o);

            TXIF = new BinaryReaderX(new MemoryStream(Sections["TXIF"].Data)).ReadStruct<TXIF>();
            var txim = Sections["TXIM"];
            var txpl = Sections["TXPL"];
            Section tx4i;
            HasMap = Sections.TryGetValue("TX4I", out tx4i);

            int width = 2 << (int)Math.Log(TXIF.Width - 1, 2), height = TXIF.Height;
            BitDepth = (TXIMBitDepth)(width * height * Math.Max(TXIF.ImageCount, 1) / txim.Data.Length);
            var bmp = new Bitmap(width, height);
            var pal = Enumerable.Range(0, txpl.Data.Length / 2).Select(w => ToBGR555(BitConverter.ToInt16(txpl.Data, 2 * w))).ToList();
            //if (BitDepth == TXIMBitDepth.BPP8 && )
            for (var k = 0; k < 256 - txpl.Data.Length / 2; k++)
                pal.Add(Color.White);

            if (BitDepth == TXIMBitDepth.BPP8 || HasMap)
            {
                for (var i = 0; i < width * height; i++)
                {
                    var (x, y, z) = (i % width, i / width, HasMap ? 0 : txim.Data[i]);
                    if (HasMap)
                    {
                        var (a, b, c, d) = (i & -(4 * width), i & (3 * width), i & (width - 4), i & 3);
                        var bits = (txim.Data[a / 4 + b / width + c] >> 2 * d) & 3;
                        var entry = BitConverter.ToInt16(tx4i.Data, x / 4 * 2 + y / 4 * (width / 2));
                        if (entry < 0 || bits < 3) z = 2 * (entry & 0x3FFF) + bits;
                    }
                    bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                }

                Bitmaps.Add(bmp);
            }
            else if (BitDepth == TXIMBitDepth.BPP4)
            {
                var offset = 0;
                for (var i = 0; i < Math.Max(TXIF.ImageCount, 1); i++)
                {
                    bmp = new Bitmap(width, height);
                    var accumulator = 0;
                    for (var j = 0; j < width * height / 2; j++)
                    {
                        var (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset] & 0xF);
                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                        accumulator++;
                        (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset] >> 4);
                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                    }
                    offset += (width * height) / 2;
                    Bitmaps.Add(bmp);
                }
            }
            else if (BitDepth == TXIMBitDepth.BPP2)
            {
                var offset = 0;
                for (var i = 0; i < Math.Max(TXIF.ImageCount, 1); i++)
                {
                    bmp = new Bitmap(width, height);
                    var accumulator = 0;
                    for (var j = 0; j < (width * height) / 4; j++)
                    {
                        var (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset] & 0x3);
                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                        accumulator++;
                        (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, (txim.Data[j + offset] & 0xF) >> 2);
                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                        accumulator++;
                        (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, (txim.Data[j + offset] & 0x3F) >> 4);
                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                        accumulator++;
                        (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset] >> 6);
                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
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
