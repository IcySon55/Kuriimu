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
            HasMap = Sections.TryGetValue("TX4I", out var tx4i);
            // TODO: Investigate the unknown TXPI section

            int width = TXIF.Width, height = TXIF.Height;
            var paddedWidth = 2 << (int)Math.Log(TXIF.Width - 1, 2);
            var imgCount = Math.Max((int)TXIF.ImageCount, 1);
            var bitDepth = 8 * txim.Data.Length / height / paddedWidth / imgCount;
            BitDepth = TXIMBitDepth.BPP2; // THIS IS JUST A HAX TO MAKE KUKKII NOT CRASH FOR NOW
            var bmp = new Bitmap(paddedWidth, height);
            var pal = Enumerable.Range(0, txpl.Data.Length / 2).Select(w => ToBGR555(BitConverter.ToInt16(txpl.Data, 2 * w))).ToList();

            // TODO: This check needs to be replaced with something more concrete later
            var IsL8 = bitDepth == 8 && txim.Data.Any(b => b > pal.Count);

            if (HasMap)
            {
                for (var i = 0; i < paddedWidth * height; i++)
                {
                    // TODO: Finish figuring out TX4I
                    var (x, y, z) = (i % paddedWidth, i / paddedWidth, 0);
                    var (a, b, c, d) = (i & -(4 * paddedWidth), i & (3 * paddedWidth), i & (paddedWidth - 4), i & 3);
                    var bits = (txim.Data[a / 4 + b / paddedWidth + c] >> 2 * d) & 3;
                    var entry = BitConverter.ToInt16(tx4i.Data, x / 4 * 2 + y / 4 * (paddedWidth / 2));
                    if (entry < 0 || bits < 3) z = 2 * (entry & 0x3FFF) + bits;
                    bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                }

                Bitmaps.Add(bmp);
            }
            else
            {
                for (var i = 0; i < imgCount; i++)
                {
                    bmp = new Bitmap(width, height);

                    for (var y = 0; y < height; y++)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            var k = y * paddedWidth + x;
                            var z = (txim.Data[k * bitDepth / 8] >> bitDepth * (x % (8 / bitDepth))) & ((1 << bitDepth) - 1);
                            if (IsL8)
                                bmp.SetPixel(x, y, z == 0 ? Color.Transparent : Color.FromArgb(z, z, z));
                            if (z < pal.Count)
                                bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                        }
                    }

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
                // TODO: Save ;_;
            }
        }
    }
}
