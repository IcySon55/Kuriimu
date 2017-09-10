using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CeteraDS.Image;
using Kuriimu.IO;

namespace image_nintendo.VCG
{
    class VCG
    {
        public List<Bitmap> bmps = new List<Bitmap>();
        

        public VCG(Stream inputVCG, Stream inputVCL, Stream inputVCE)
        {
            IEnumerable<Color> pal;
            int transparentIndex;

            using (var brVCL = new BinaryReaderX(inputVCL))
            {
                var magic = brVCL.ReadString(4);
                var count = brVCL.ReadInt16();
                transparentIndex = brVCL.ReadInt16();

                pal = Common.GetPalette(brVCL.ReadBytes((int)(brVCL.BaseStream.Length - brVCL.BaseStream.Position)), Format.BGR555);
            }

            var settings = new ImageSettings
            {
                Width = 96,
                Height = 96,
                TileSize = 8,
                BitPerIndex = BitLength.Bit4,
                TransparentColor = pal.ToList()[transparentIndex]
            };

            using (var brVCG = new BinaryReaderX(inputVCG))
            {
                var magic = brVCG.ReadString(4);

                bmps.Add(Common.Load(brVCG.ReadBytes((int)(brVCG.BaseStream.Length - brVCG.BaseStream.Position)), settings, pal));
            }

            using (var brVCE = new BinaryReaderX(inputVCE))
            {
                var magic = brVCE.ReadString(4);
                
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
