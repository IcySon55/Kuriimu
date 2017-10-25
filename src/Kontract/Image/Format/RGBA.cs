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
    public class RGBA : IImageFormat
    {
        public int BitDepth { get; set; }

        public string FormatName { get; set; }

        int rDepth;
        int gDepth;
        int bDepth;
        int aDepth;

        ByteOrder byteOrder;

        bool standard;

        public RGBA(int r, int g, int b, int a = 0, ByteOrder byteOrder = ByteOrder.LittleEndian, bool standard = false)
        {
            BitDepth = r + g + b + a;
            if (BitDepth % 8 != 0) throw new Exception($"Overall bitDepth has to be dividable by 8. Given bitDepth: {BitDepth}");
            if (BitDepth <= 8) throw new Exception($"Overall bitDepth can't be smaller than 16. Given bitDepth: {BitDepth}");
            if (BitDepth > 32) throw new Exception($"Overall bitDepth can't be bigger than 32. Given bitDepth: {BitDepth}");

            this.byteOrder = byteOrder;

            rDepth = r;
            gDepth = g;
            bDepth = b;
            aDepth = a;

            FormatName = "RGB" + ((a != 0) ? "A" : "") + r.ToString() + g.ToString() + b.ToString() + ((a != 0) ? a.ToString() : "");

            this.standard = standard;
        }

        public IEnumerable<Color> Load(byte[] tex)
        {
            using (var br = new BinaryReaderX(new MemoryStream(tex), byteOrder))
            {
                var bShift = aDepth;
                var gShift = bShift + bDepth;
                var rShift = gShift + gDepth;

                var aBitMask = (int)Math.Pow(2, aDepth) - 1;
                var bBitMask = (int)Math.Pow(2, bDepth) - 1;
                var gBitMask = (int)Math.Pow(2, gDepth) - 1;
                var rBitMask = (int)Math.Pow(2, rDepth) - 1;

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    long value = 0;

                    switch (BitDepth)
                    {
                        case 16:
                            value = br.ReadUInt16();
                            break;
                        case 24:
                            var tmp = br.ReadBytes(3);
                            value = (byteOrder == ByteOrder.LittleEndian) ? tmp[2] << 16 | tmp[1] << 8 | tmp[0] : tmp[0] << 16 | tmp[1] << 8 | tmp[0];
                            break;
                        case 32:
                            value = br.ReadUInt32();
                            break;
                        default:
                            throw new Exception($"BitDepth {BitDepth} not supported!");
                    }

                    yield return Color.FromArgb(
                        (aDepth == 0) ? 255 : CorrectValue((int)(value & aBitMask), aDepth),
                        CorrectValue((int)(value >> rShift & rBitMask), rDepth),
                        CorrectValue((int)(value >> gShift & gBitMask), gDepth),
                        CorrectValue((int)(value >> bShift & bBitMask), bDepth));
                }
            }
        }

        int CorrectValue(int value, int depth)
        {
            switch (depth)
            {
                case 1:
                    return value * 255;
                case 2:
                    return value * 85;
                case 3:
                    return value * 73 / 2;
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
