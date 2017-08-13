using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Compression;
using Kuriimu.IO;
using CeteraDS.Image;

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
        public List<Bitmap> bmps = new List<Bitmap>();

        private List<Section> Sections;

        public TXIF TXIF { get; }
        public TXIMBitDepth BitDepth { get; }
        public bool IsMultiTXIM { get; } = false;
        public bool HasMap { get; } = false;

        public CHNKTEX(Stream input)
        {
            //get sections
            Sections = GetSections(input).ToList();

            //separate sections
            TXIF = new BinaryReaderX(new MemoryStream(Sections.FirstOrDefault(s => s.magic == "TXIF").data)).ReadStruct<TXIF>();
            //var txim = Sections.Where(s => s.Magic == "TXIM").ToList();
            var txim = Sections.FirstOrDefault(s => s.magic == "TXIM");
            var txims = Sections.Where(s => s.magic == "TXIM").ToList();
            var txpl = Sections.FirstOrDefault(s => s.magic == "TXPL");
            var tx4i = Sections.FirstOrDefault(s => s.magic == "TX4I");

            IsMultiTXIM = txims.Count > 1;
            HasMap = tx4i != null;

            int width = TXIF.Width, height = TXIF.Height;
            var paddedWidth = 2 << (int)Math.Log(TXIF.Width - 1, 2);
            var imgCount = Math.Max((int)TXIF.ImageCount, 1);
            var bitDepth = 8 * txim.data.Length / height / paddedWidth / (!IsMultiTXIM ? imgCount : 1);
            BitDepth = (TXIMBitDepth)bitDepth;
            var bmp = new Bitmap(paddedWidth, height);
            var pal = Common.GetPalette(txpl.data, Format.BGR555);

            // TODO: This check needs to be replaced with something more concrete later
            var isL8 = bitDepth == 8 && txim.data.Any(b => b > pal.Count());
            if (isL8) BitDepth = TXIMBitDepth.L8;

            if (HasMap)
            {
                for (var i = 0; i < paddedWidth * height; i++)
                {
                    // TODO: Finish figuring out TX4I
                    var (x, y, z) = (i % paddedWidth, i / paddedWidth, 0);
                    var (a, b, c, d) = (i & -(4 * paddedWidth), i & (3 * paddedWidth), i & (paddedWidth - 4), i & 3);
                    var bits = (txim.data[a / 4 + b / paddedWidth + c] >> 2 * d) & 3;
                    var entry = BitConverter.ToInt16(tx4i.data, x / 4 * 2 + y / 4 * (paddedWidth / 2));
                    if (entry < 0 || bits < 3) z = 2 * (entry & 0x3FFF) + bits;
                    bmp.SetPixel(x, y, z >= pal.Count() ? Color.Black : pal.ToList()[z]);
                }

                bmps.Add(bmp);
            }
            else
            {
                for (var i = 0; i < imgCount; i++)
                {
                    if (isL8)
                    {
                        //create L8 palette
                        var list = new List<Color>() { Color.FromArgb(0, 0, 0, 0) };
                        for (int j = 1; j < Math.Pow(2, bitDepth); j++) list.Add(Color.FromArgb(255, j, j, j));
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
                    bmps.Add(Common.Load(txims[i].data, settings, pal));
                }
            }
        }

        //private Color ToBGR555(short s) => Color.FromArgb(s % 32 * 33 / 4, (s >> 5) % 32 * 33 / 4, (s >> 10) * 33 / 4);

        private IEnumerable<Section> GetSections(Stream stream)
        {
            using (var br = new BinaryReaderX(stream))
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var chunk = br.ReadStruct<CHNK>();
                    var section = new Section
                    {
                        chunk = chunk,
                        magic = br.ReadString(4),
                        size = br.ReadInt32()
                    };
                    section.data = br.ReadBytes(section.size);

                    if (chunk.decompSize != 0)
                        section.data = Nintendo.Decompress(new MemoryStream(section.data));

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

//get sections
/*sections = GetSections(input).ToList();

//separate sections
txif = new BinaryReaderX(new MemoryStream(sections.FirstOrDefault(s => s.Magic == "TXIF").Data)).ReadStruct<TXIF>();
var txim = sections.Where(s => s.Magic == "TXIM").ToList();
var tx4i = sections.FirstOrDefault(s => s.Magic == "TX4I");
var txpl = sections.FirstOrDefault(s => s.Magic == "TXPL");

//loop through txims
for (int i = 0; i < txim.Count; i++)
{
    if (tx4i == null)     //if no map is used
    {
        var palette = Common.GetPalette(txpl.Data, Format.BGR555);
        var settings = new ImageSettings
        {
            Width = txif.Width,
            Height = txif.Height,
            BitPerIndex = BitLength.Bit4,
            PadToPowerOf2 = true
        };
        bmps.Add(Common.Load(txim[0].Data, settings, palette));
    }
    else
    {
        /*for (var i = 0; i < paddedWidth * height; i++)
        {
            // TODO: Finish figuring out TX4I
            var (x, y, z) = (i % paddedWidth, i / paddedWidth, 0);
            var (a, b, c, d) = (i & -(4 * paddedWidth), i & (3 * paddedWidth), i & (paddedWidth - 4), i & 3);
            var bits = (txim.Data[a / 4 + b / paddedWidth + c] >> 2 * d) & 3;
            var entry = BitConverter.ToInt16(tx4i.Data, x / 4 * 2 + y / 4 * (paddedWidth / 2));
            if (entry < 0 || bits < 3) z = 2 * (entry & 0x3FFF) + bits;
            bmp.SetPixel(x, y, z == 0 ? Color.Transparent : pal[z]);
        }

        Bitmaps.Add(bmp);*/
/*}
}*/
