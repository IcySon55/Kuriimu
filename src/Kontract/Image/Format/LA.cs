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
    public class LA : IImageFormat
    {
        public int bitDepth { get; set; }

        int lDepth;
        int aDepth;

        ByteOrder byteOrder;

        public LA(int l, int a, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            bitDepth = a + l;
            if (bitDepth % 4 != 0) throw new Exception($"Overall bitDepth has to be dividable by 4. Given bitDepth: {bitDepth}");
            if (bitDepth > 16) throw new Exception($"Overall bitDepth can't be bigger than 16. Given bitDepth: {bitDepth}");
            if (bitDepth < 4) throw new Exception($"Overall bitDepth can't be smaller than 4. Given bitDepth: {bitDepth}");
            if (l < 4 || a < 4) throw new Exception($"Luminance and Alpha value can't be smaller than 4.\nGiven Luminance: {l}; Given Alpha: {a}");

            lDepth = l;
            aDepth = a;
        }

        public IEnumerable<Color> Load(byte[] tex)
        {
            using (var br = new BinaryReaderX(new MemoryStream(tex), byteOrder))
            {
                var lShift = aDepth;

                var aBitMask = (int)Math.Pow(2, aDepth) - 1;
                var lBitMask = (int)Math.Pow(2, lDepth) - 1;

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    long value = 0;

                    switch (bitDepth)
                    {
                        case 4:
                            value = br.ReadNibble();
                            break;
                        case 8:
                            value = br.ReadByte();
                            break;
                        case 16:
                            value = br.ReadUInt16();
                            break;
                        default:
                            throw new Exception($"BitDepth {bitDepth} not supported!");
                    }

                    yield return Color.FromArgb(
                        (aDepth == 0) ? 255 : CorrectValue((int)(value & aBitMask), aDepth),
                        (lDepth == 0) ? 255 : CorrectValue((int)(value >> lShift & lBitMask), lDepth),
                        (lDepth == 0) ? 255 : CorrectValue((int)(value >> lShift & lBitMask), lDepth),
                        (lDepth == 0) ? 255 : CorrectValue((int)(value >> lShift & lBitMask), lDepth));
                }
            }
        }

        int CorrectValue(int value, int depth)
        {
            switch (depth)
            {
                case 4:
                    return value * 15;
                case 5:
                    return value * 33 / 4;
                case 6:
                    return value * 65 / 16;
                case 7:
                    return value * 129 / 64;
                case 9:
                    return value / 2;
                case 10:
                    return value / 4;
                default:
                    return value;
            }
        }

        public void Save(Color color, Stream output)
        {
        }
    }
}
