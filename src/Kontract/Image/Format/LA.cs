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
        public int BitDepth { get; set; }

        public string FormatName { get; set; }

        int lDepth;
        int aDepth;

        ByteOrder byteOrder;

        public LA(int l, int a, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            BitDepth = a + l;
            if (BitDepth % 4 != 0) throw new Exception($"Overall bitDepth has to be dividable by 4. Given bitDepth: {BitDepth}");
            if (BitDepth > 16) throw new Exception($"Overall bitDepth can't be bigger than 16. Given bitDepth: {BitDepth}");
            if (BitDepth < 4) throw new Exception($"Overall bitDepth can't be smaller than 4. Given bitDepth: {BitDepth}");
            if (l < 4 && a < 4) throw new Exception($"Luminance and Alpha value can't be smaller than 4.\nGiven Luminance: {l}; Given Alpha: {a}");

            lDepth = l;
            aDepth = a;

            FormatName = ((l != 0) ? "L" : "") + ((a != 0) ? "A" : "") + ((l != 0) ? l.ToString() : "") + ((a != 0) ? a.ToString() : "");

            this.byteOrder = byteOrder;
        }

        public IEnumerable<Color> Load(byte[] tex)
        {
            using (var br = new BinaryReaderX(new MemoryStream(tex), byteOrder))
            {
                var lShift = aDepth;

                var aBitMask = (1 << aDepth) - 1;
                var lBitMask = (1 << lDepth) - 1;

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    long value = 0;

                    switch (BitDepth)
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
                            throw new Exception($"BitDepth {BitDepth} not supported!");
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
                    return value * 17;
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

        BinaryWriterX bw = null;

        public void Save(Color color, Stream output)
        {
            var a = (aDepth == 0) ? 0 : CompressValue(color.A, aDepth);
            var l = (lDepth == 0) ? 0 : CompressValue(color.G, lDepth);

            var lShift = aDepth;

            long value = a;
            value |= (uint)(l << lShift);

            if (bw == null)
                bw = new BinaryWriterX(output, true, byteOrder);

            switch (BitDepth)
            {
                case 4:
                    bw.WriteNibble((int)value);
                    break;
                case 8:
                    bw.Write((byte)value);
                    break;
                case 16:
                    bw.Write((ushort)value);
                    break;
                default:
                    throw new Exception($"BitDepth {BitDepth} not supported!");
            }
        }

        int CompressValue(int value, int depth)
        {
            switch (depth)
            {
                case 4:
                    return value / 17;
                case 5:
                    return (((value + 1) * 4) - 1) / 33;
                case 6:
                    return (((value + 1) * 16) - 1) / 65;
                case 7:
                    return (((value + 1) * 64) - 1) / 129;
                case 9:
                    return ((value + 1) * 2) - 1;
                case 10:
                    return ((value + 1) * 4) - 1;
                default:
                    return value;
            }
        }
    }
}
