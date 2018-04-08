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
        public int BitDepth { get; set; }
        public int BlockBitDepth { get; set; }

        public string FormatName { get; set; }

        bool alpha;
        bool _3ds_order;

        ByteOrder byteOrder;

        public ETC1(bool alpha = false, bool _3ds_order = true, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            BitDepth = alpha ? 8 : 4;
            BlockBitDepth = alpha ? 128 : 64;

            this.alpha = alpha;
            this._3ds_order = _3ds_order;

            FormatName = (alpha) ? "ETC1A4" : "ETC1";

            this.byteOrder = byteOrder;
        }

        public IEnumerable<Color> Load(byte[] tex)
        {
            using (var br = new BinaryReaderX(new MemoryStream(tex), byteOrder))
            {
                var etc1decoder = new Support.ETC1.Decoder(_3ds_order);

                while (true)
                {
                    yield return etc1decoder.Get(() =>
                    {
                        var etc1Alpha = alpha ? br.ReadUInt64() : ulong.MaxValue;
                        if (etc1Alpha != 0)
                            ;
                        var colorBlock = br.ReadUInt64();
                        var etc1Block = new Support.ETC1.Block
                        {
                            LSB = (ushort)(colorBlock & 0xFFFF),
                            MSB = (ushort)((colorBlock >> 16) & 0xFFFF),
                            flags = (byte)((colorBlock >> 32) & 0xFF),
                            B = (byte)((colorBlock >> 40) & 0xFF),
                            G = (byte)((colorBlock >> 48) & 0xFF),
                            R = (byte)((colorBlock >> 56) & 0xFF)
                        };
                        return new Support.ETC1.PixelData { Alpha = etc1Alpha, Block = etc1Block };
                    });
                }
            }
        }

        public byte[] Save(IEnumerable<Color> colors)
        {
            var etc1encoder = new Support.ETC1.Encoder(_3ds_order);

            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, byteOrder))
            {
                foreach (var color in colors)
                    etc1encoder.Set(color, data =>
                    {
                        if (alpha) bw.Write(data.Alpha);
                        ulong colorBlock = 0;
                        colorBlock |= data.Block.LSB;
                        colorBlock |= ((ulong)data.Block.MSB << 16);
                        colorBlock |= ((ulong)data.Block.flags << 32);
                        colorBlock |= ((ulong)data.Block.B << 40);
                        colorBlock |= ((ulong)data.Block.G << 48);
                        colorBlock |= ((ulong)data.Block.R << 56);
                        bw.Write(colorBlock);
                    });
            }

            return ms.ToArray();
        }
    }
}
