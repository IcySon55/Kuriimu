using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CeteraDS.Image;
using Kontract.Compression;
using Kontract.IO;

/**CHNK - Chunk, contains every partition and provides compression information
 * TXIF - Gives general information about the textures
 * TXIM - Indeces to colours in palette
 * TX4I - Map to group the indeces of TXIM, one entry presents a 4x4 texel
 * TXPL - Presents the palette, encoded with BGR555 by default
 * TXPI - Page Interrupt, controls the animation if buttons are pressed ingame, not needed for image visualization
 * TXLS - Light Source, seems to control some sort of lighting, not needed for image visualization
 **/

namespace image_nintendo
{
    class CHNKTEX
    {
        private List<Section> Sections;

        public List<Bitmap> Bitmaps = new List<Bitmap>();
        public TXIF TXIF { get; }
        public TXIMBitDepth BitDepth { get; }
        public bool IsMultiTXIM { get; } = false;
        public bool HasMap { get; } = false;

        public CHNKTEX(Stream input)
        {
            Sections = GetSections(input).ToList();

            TXIF = new BinaryReaderX(new MemoryStream(Sections.FirstOrDefault(s => s.Magic == "TXIF").Data)).ReadStruct<TXIF>();
            var txim = Sections.FirstOrDefault(s => s.Magic == "TXIM");
            var txims = Sections.Where(s => s.Magic == "TXIM").ToList();
            var txpl = Sections.FirstOrDefault(s => s.Magic == "TXPL");
            var tx4i = Sections.FirstOrDefault(s => s.Magic == "TX4I");

            IsMultiTXIM = txims.Count > 1;
            HasMap = tx4i != null;

            int width = TXIF.Width, height = TXIF.Height;
            var paddedWidth = 2 << (int)Math.Log(TXIF.Width - 1, 2);
            var imgCount = Math.Max((int)TXIF.ImageCount, 1);
            var bitDepth = 8 * txim.Data.Length / height / paddedWidth / (!IsMultiTXIM ? imgCount : 1);
            BitDepth = (TXIMBitDepth)bitDepth;
            var bmp = new Bitmap(paddedWidth, height);
            var pal = Common.GetPalette(txpl.Data, Format.BGR555);

            // TODO: This check needs to be replaced with something more concrete later
            var isL8 = bitDepth == 8 && txim.Data.Any(b => b > pal.Count());
            if (isL8) BitDepth = TXIMBitDepth.L8;

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
                    bmp.SetPixel(x, y, z >= pal.Count() ? Color.Black : pal.ToList()[z]);
                }

                Bitmaps.Add(bmp);
            }
            else
            {
                for (var i = 0; i < imgCount; i++)
                {
                    if (isL8)
                    {
                        // Create L8 palette
                        var list = new List<Color>() { Color.FromArgb(0, 0, 0, 0) };
                        for (int j = 1; j < Math.Pow(2, bitDepth); j++)
                            list.Add(Color.FromArgb(255, j, j, j));
                        pal = list;
                    }

                    var settings = new ImageSettings
                    {
                        Width = width,
                        Height = height,
                        BitPerIndex = (bitDepth == 4) ? BitLength.Bit4 : BitLength.Bit8,
                        TileSize = paddedWidth,
                        TransparentColor = pal.ToList()[0]
                    };
                    Bitmaps.Add(Common.Load(txims[i].Data, settings, pal));
                }
            }
        }

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
                        section.Data = Nintendo.Decompress(new MemoryStream(section.Data));

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
