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
            // TODO: Investigate the unknown TXPI section

            int virtualWidth = TXIF.Width;
            int width = 2 << (int)Math.Log(TXIF.Width - 1, 2), height = TXIF.Height;
            BitDepth = (TXIMBitDepth)(width * height * Math.Max(TXIF.ImageCount, (short)1) / txim.Data.Length);
            var bmp = new Bitmap(width, height);
            var pal = Enumerable.Range(0, txpl.Data.Length / 2).Select(w => ToBGR555(BitConverter.ToInt16(txpl.Data, 2 * w))).ToList();

            // TODO: This check needs to be replaced with something more concrete later
            if (BitDepth == TXIMBitDepth.BPP8 && txim.Data.Any(b => b > pal.Count))
                BitDepth = TXIMBitDepth.L8;

            if (HasMap)
            {
                for (var i = 0; i < width * height; i++)
                {
                    // TODO: Finish figuring out TX4I
                    var (x, y, z) = (i % width, i / width, 0);
                    var (a, b, c, d) = (i & -(4 * width), i & (3 * width), i & (width - 4), i & 3);
                    var bits = (txim.Data[a / 4 + b / width + c] >> 2 * d) & 3;
                    var entry = BitConverter.ToInt16(tx4i.Data, x / 4 * 2 + y / 4 * (width / 2));
                    if (entry < 0 || bits < 3) z = 2 * (entry & 0x3FFF) + bits;
                    bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                }

                Bitmaps.Add(bmp);
            }
            else
            {
                var offset = 0;
                for (var i = 0; i < Math.Max(TXIF.ImageCount, (short)1); i++)
                {
                    bmp = new Bitmap(virtualWidth, height);

                    var accumulator = 0;
                    switch (BitDepth)
                    {
                        case TXIMBitDepth.BPP8:
                            for (var j = 0; j < width * height; j++)
                            {
                                if ((j + accumulator) % width < virtualWidth)
                                {
                                    var (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset]);
                                    if (z < pal.Count)
                                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                                }
                            }
                            break;
                        case TXIMBitDepth.BPP4:
                            for (var j = 0; j < width * height / (int)BitDepth; j++)
                            {
                                if ((j + accumulator) % width < virtualWidth)
                                {
                                    var (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset] & 0xF);
                                    if (z < pal.Count)
                                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                                }
                                accumulator++;
                                if ((j + accumulator) % width < virtualWidth)
                                {
                                    var (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset] >> 4);
                                    if (z < pal.Count)
                                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                                }
                            }
                            break;
                        case TXIMBitDepth.BPP2:
                            for (var j = 0; j < (width * height) / (int)BitDepth; j++)
                            {
                                if ((j + accumulator) % width < virtualWidth)
                                {
                                    var (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset] & 0x3);
                                    if (z < pal.Count)
                                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                                }
                                accumulator++;
                                if ((j + accumulator) % width < virtualWidth)
                                {
                                    var (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, (txim.Data[j + offset] & 0xF) >> 2);
                                    if (z < pal.Count)
                                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                                }
                                accumulator++;
                                if ((j + accumulator) % width < virtualWidth)
                                {
                                    var (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, (txim.Data[j + offset] & 0x3F) >> 4);
                                    if (z < pal.Count)
                                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                                }
                                accumulator++;
                                if ((j + accumulator) % width < virtualWidth)
                                {
                                    var (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset] >> 6);
                                    if (z < pal.Count)
                                        bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
                                }
                            }
                            break;
                        case TXIMBitDepth.L8:
                            for (var j = 0; j < width * height; j++)
                            {
                                // TODO: The results of this method aren't representative of the in-game result
                                if ((j + accumulator) % width < virtualWidth)
                                {
                                    var (x, y, z) = ((j + accumulator) % width, (j + accumulator) / width, txim.Data[j + offset]);
                                    bmp.SetPixel(x, y, z == 0 ? Color.Transparent : Color.FromArgb(z, z, z));
                                }
                            }
                            break;
                    }
                    offset += width * height / (BitDepth < TXIMBitDepth.L8 ? (int)BitDepth : 1);

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
