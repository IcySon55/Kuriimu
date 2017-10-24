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
    public class ATI : IImageFormat
    {
        public enum Format
        {
            ATI1L,
            ATI1A,
            ATI2,
        }

        public int bitDepth { get; set; }
        public int blockBitDepth { get; set; }

        Format format;

        ByteOrder byteOrder;

        public ATI(Format format, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            bitDepth = (format == Format.ATI1L || format == Format.ATI1A) ? 4 : 8;
            blockBitDepth = (format == Format.ATI1L || format == Format.ATI1A) ? 64 : 128;

            this.format = format;

            this.byteOrder = byteOrder;
        }

        public IEnumerable<Color> Load(byte[] tex)
        {
            using (var br = new BinaryReaderX(new MemoryStream(tex), byteOrder))
            {
                Enum.TryParse<Support.ATI.Format>(format.ToString(), false, out var atiformat);
                var atidecoder = new Support.ATI.Decoder(atiformat);

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    yield return atidecoder.Get(() =>
                    {
                        //if (br.BaseStream.Position == br.BaseStream.Length) return (0, 0);
                        return (br.ReadUInt64(), format == Format.ATI2 ? br.ReadUInt64() : 0);
                    });
                }
            }
        }

        public void Save(Color color, Stream output)
        {
        }
    }
}
