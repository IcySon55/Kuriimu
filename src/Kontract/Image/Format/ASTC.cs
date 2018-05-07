using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace Kontract.Image.Format
{
    public class ASTC : IImageFormat
    {
        public int BitDepth { get; set; }
        public int BlockBitDepth { get; set; }
        public string FormatName { get; set; }

        ByteOrder byteOrder;

        List<(int, int)> legitBlockSizes = new List<(int, int)> { (4, 4), (5, 4), (5, 5), (6, 5), (6, 6), (8, 5), (8, 6), (8, 8), (10, 5), (10, 6), (10, 8), (10, 10), (12, 10), (12, 12) };
        (int, int) blockDim;

        public ASTC(int blockWidth, int blockHeight, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            if (!legitBlockSizes.Contains((blockWidth, blockHeight)))
                throw new Exception(blockWidth + "x" + blockHeight + " is an invalid block.");
            blockDim = (blockWidth, blockHeight);

            BitDepth = 8;
            BlockBitDepth = 128;

            this.byteOrder = byteOrder;

            FormatName = "ASTC" + blockWidth + "x" + blockHeight;
        }

        public IEnumerable<Color> Load(byte[] tex)
        {
            using (var br = new BinaryReaderX(new MemoryStream(tex), byteOrder))
            {
                var astcdecoder = new Support.ASTC.Decoder(blockDim.Item1, blockDim.Item2);

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    yield return astcdecoder.Get(() =>
                    {
                        return br.ReadBytes(BlockBitDepth / 8);
                    });
                }
            }
        }

        public byte[] Save(IEnumerable<Color> colors)
        {
            var astcencoder = new Support.ASTC.Encoder(blockDim.Item1, blockDim.Item2);

            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms))
                foreach (var color in colors)
                    astcencoder.Set(color, data =>
                    {
                        bw.Write(data);
                    });

            return ms.ToArray();
        }
    }
}
