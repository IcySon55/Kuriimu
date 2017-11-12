using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Image;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;
using Kontract.IO;

namespace image_nintendo.VCG
{
    class VCG
    {
        public List<Bitmap> bmps = new List<Bitmap>();


        public VCG(Stream inputVCG, Stream inputVCL, Stream inputVCE)
        {
            List<Color> pal;
            int transparentIndex;

            // Palette
            using (var brVCL = new BinaryReaderX(inputVCL))
            {
                var magic = brVCL.ReadString(4);
                var count = brVCL.ReadInt16();
                transparentIndex = brVCL.ReadInt16();

                pal = new Palette(brVCL.ReadBytes((int)(brVCL.BaseStream.Length - brVCL.BaseStream.Position)), new RGBA(5, 5, 5)).colors;
            }

            var settings = new ImageSettings
            {
                Width = 112,
                Height = 112,
                Format = new Palette(pal, 4),
                Swizzle = new NitroSwizzle(112, 112)
                //TransparentColor = pal.ToList()[transparentIndex - 1]
            };

            // Tiles
            var tiles = new List<byte[]>();
            using (var brVCG = new BinaryReaderX(inputVCG))
            {
                var magic = brVCG.ReadString(4);
                brVCG.ReadInt64();
                var dataLength = brVCG.ReadInt32();

                while (brVCG.BaseStream.Position < brVCG.BaseStream.Length)
                    tiles.Add(brVCG.ReadBytes(4 * 4));

                // Temporary
                brVCG.BaseStream.Position = 0x10;
                bmps.Add(Common.Load(brVCG.ReadBytes((int)(brVCG.BaseStream.Length - brVCG.BaseStream.Position)), settings));
            }

            // Tile Map
            using (var brVCE = new BinaryReaderX(inputVCE))
            {
                var magic = brVCE.ReadString(4);



                //bmps.Add(Common.Load(brVCG.ReadBytes((int)(brVCG.BaseStream.Length - brVCG.BaseStream.Position)), settings, pal));
            }

        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {

            }
        }
    }
}
