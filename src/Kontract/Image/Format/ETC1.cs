using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;
using Kontract.IO;
using System.IO;

namespace Kontract.Image.Format
{
    public class ETC1 : IImageFormat
    {
        public int bitDepth { get; set; }
        public int blockBitDepth { get; set; }

        bool alpha;

        ByteOrder byteOrder;

        public ETC1(bool alpha = false, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            bitDepth = (alpha == false) ? 2 : 4;
            blockBitDepth = (alpha == false) ? 64 : 128;

            this.alpha = alpha;

            this.byteOrder = byteOrder;
        }

        public IEnumerable<Color> Load(byte[] tex)
        {
            using (var br = new BinaryReaderX(new MemoryStream(tex), byteOrder))
            {
                var etc1decoder = new Support.ETC1.Decoder();

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    yield return etc1decoder.Get(() =>
                    {
                        var etc1Alpha = alpha ? br.ReadUInt64() : ulong.MaxValue;
                        return new Support.ETC1.PixelData { Alpha = etc1Alpha, Block = br.ReadStruct<Support.ETC1.Block>() };
                    });
                }
            }
        }

        public void Save(Color color, Stream output)
        {
        }
    }
}
