using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Kuriimu.IO;

namespace image_nintendo
{
    class CHNKTEX
    {
        private const int WidthMultiplier = 4;
        private const int HeightMultiplier = 32;
        private const int MinHeight = 8;

        public TXIF Txif { get; set; }
        public List<Color> Palette = new List<Color>();
        public List<byte> Map;
        public List<byte> Tiles;

        public List<Bitmap> Bitmaps = new List<Bitmap>();

        public CHNKTEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var chunk = br.ReadStruct<CHNK>();
                    var section = br.ReadString(4);

                    if (section == "TXIF")
                        Txif = br.ReadStruct<TXIF>();
                    else if (section == "TXIM")
                        ReadTXIM(chunk, br);
                    else if (section == "TX4I")
                        ReadTX4I(chunk, br);
                    else if (section == "TXPL")
                        ReadTXPL(chunk, br);
                }

                Bitmaps.Add(BuildBitmap());
            }
        }

        public void ReadTXIM(CHNK chunk, BinaryReaderX br)
        {
            if (chunk.DecompressedSize > 0)
                Map = Kuriimu.Compression.LZ11.Decompress(new MemoryStream(br.ReadBytes(br.ReadInt32()))).ToList();
            else
                Map = br.ReadBytes(br.ReadInt32()).ToList();
        }

        public void ReadTX4I(CHNK chunk, BinaryReaderX br)
        {
            if (chunk.DecompressedSize > 0)
                Tiles = Kuriimu.Compression.LZ11.Decompress(new MemoryStream(br.ReadBytes(br.ReadInt32()))).ToList();
            else
                Tiles = br.ReadBytes(br.ReadInt32()).ToList();
        }

        public void ReadTXPL(CHNK chunk, BinaryReaderX br)
        {
            byte[] paletteBytes;
            if (chunk.DecompressedSize > 0)
                paletteBytes = Kuriimu.Compression.LZ11.Decompress(new MemoryStream(br.ReadBytes(br.ReadInt32())));
            else
                paletteBytes = br.ReadBytes(br.ReadInt32());

            var brp = new BinaryReader(new MemoryStream(paletteBytes));

            //var palette = new Bitmap(160, 260, PixelFormat.Format32bppRgb);
            //var gfx = Graphics.FromImage(palette);

            //int x = 0, y = 0;
            int a = 255, r = 255, g = 255, b = 255;
            while (brp.BaseStream.Position < brp.BaseStream.Length)
            {
                var s = brp.ReadUInt16();

                b = (s >> 10) * 33 / 4;
                g = ((s & 0x3FF) >> 5) * 33 / 4;
                r = (s & 0x1F) * 33 / 4;

                var color = Color.FromArgb(a, r, g, b);
                Palette.Add(color);

                // Draw to the palette
                //gfx.FillRectangle(new SolidBrush(color), x, y, 10, 10);
                //x += 10;
                //if (x % palette.Width == 0)
                //{
                //    x = 0;
                //    y += 10;
                //}
            }
        }

        public Bitmap BuildBitmap()
        {
            var image = new Bitmap(Txif.Width, Txif.Height, PixelFormat.Format32bppRgb);
            var gfxI = Graphics.FromImage(image);

            int x = 0, y = 0;
            for (var i = 0; i < image.Width * image.Height; i++)
            {
                if ((y * image.Width + x) < Map.Count && Map[y * image.Width + x] < Palette.Count)
                    image.SetPixel(x, y, Palette[Map[y * image.Width + x]]);

                x++;
                if (x % image.Width == 0)
                {
                    x = 0;
                    y++;
                }
            }

            return image;
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
