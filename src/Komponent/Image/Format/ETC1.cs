using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Komponent.Interface;
using System.Drawing;
using Komponent.IO;
using System.IO;

namespace Komponent.Image.Format
{
    public class ETC1 : IImageFormat
    {
        public int BitDepth { get; set; }
        public int BlockBitDepth { get; set; }

        public string FormatName { get; set; }

        bool alpha;

        public ETC1(bool alpha = false)
        {
            BitDepth = alpha ? 8 : 4;
            BlockBitDepth = alpha ? 128 : 64;

            this.alpha = alpha;

            FormatName = (alpha) ? "ETC1A4" : "ETC1";
        }

        public IEnumerable<Color> Load(byte[] tex)
        {
            using (var br = new BinaryReaderX(new MemoryStream(tex)))
            {
                var etc1decoder = new Support.ETC1.Decoder();

                //while (br.BaseStream.Position < br.BaseStream.Length)
                while (true)
                {
                    yield return etc1decoder.Get(() =>
                    {
                        var etc1Alpha = alpha ? br.ReadUInt64() : ulong.MaxValue;
                        return new Support.ETC1.PixelData { Alpha = etc1Alpha, Block = br.ReadStruct<Support.ETC1.Block>() };
                    });
                }
            }
        }

        public byte[] Save(IEnumerable<Color> colors)
        {
            var etc1encoder = new Support.ETC1.Encoder();

            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms))
            {
                foreach (var color in colors)
                    etc1encoder.Set(color, data =>
                    {
                        if (alpha) bw.Write(data.Alpha);
                        bw.WriteStruct(data.Block);
                    });
            }

            return ms.ToArray();
        }
    }
}
