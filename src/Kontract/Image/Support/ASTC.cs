using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;

namespace Kontract.Image.Support
{
    public class ASTC
    {
        public class BitReader
        {
            BinaryReader _br;
            int _bitPos = 0;

            public BitReader(byte[] bytes)
            {
                _br = new BinaryReader(new MemoryStream(bytes));
            }

            public int ReadBits(int pos, int length)
            {
                if (length == 0) return 0;

                _br.BaseStream.Position = pos / 8;
                int bitsInto = pos % 8;

                var buffer = _br.ReadBytes(((bitsInto + length - 1) / 8) + 1);

                int result = 0;
                result |= buffer[0] >> bitsInto;
                for (int i = 1; i < buffer.Length; i++) result |= (buffer[i] << (i * 8)) >> (bitsInto);

                return (result & ((1 << length) - 1));
            }
        }

        public class Decoder
        {
            private Queue<Color> queue = new Queue<Color>();

            private int GetRField(int blockMode) => ((blockMode & 0x10) >> 4) | (((blockMode & 0x3) == 0) ? ((blockMode & 0x4) >> 1) | ((blockMode & 0x8) >> 1) : ((blockMode & 0x1) << 1) | ((blockMode & 0x2) << 1));
            private Dictionary<int, (int, int, byte, byte, byte)> RFieldTable = new Dictionary<int, (int, int, byte, byte, byte)>
            {
                [0b0010] = (0, 1, 0, 0, 1),
                [0b0011] = (0, 2, 1, 0, 0),
                [0b0100] = (0, 3, 0, 0, 2),
                [0b0101] = (0, 4, 0, 1, 0),
                [0b0110] = (0, 5, 1, 0, 1),
                [0b0111] = (0, 7, 0, 0, 3),
                [0b1010] = (0, 9, 0, 1, 1),
                [0b1011] = (0, 11, 1, 0, 2),
                [0b1100] = (0, 15, 0, 0, 4),
                [0b1101] = (0, 19, 0, 1, 2),
                [0b1110] = (0, 23, 1, 0, 3),
                [0b1111] = (0, 31, 0, 0, 5),
            };

            private int[] GetValuesFromTrits(int tritBlock, int bits)
            {
                int[] result = new int[5];

                var tritsOnly = ((tritBlock >> bits) & 0x3) |
                    (((tritBlock >> (bits * 2 + 2)) & 0x3) << 2) |
                    (((tritBlock >> (bits * 3 + 4)) & 0x1) << 4) |
                    (((tritBlock >> (bits * 4 + 5)) & 0x3) << 5) |
                    (((tritBlock >> (bits * 5 + 7)) & 0x1) << 7);

                //get values for trits
                int c = 0;
                if (((tritsOnly >> 2) & 0x7) == 0x7)
                {
                    c = (tritsOnly & 0x3) | (((tritsOnly >> 5) & 0x7) << 3);
                    result[4] = result[3] = 2;
                }
                else
                {
                    c = tritsOnly & 0x1F;

                    if (((tritsOnly >> 5) & 0x3) == 0x3)
                    {
                        result[4] = 2;
                        result[3] = tritsOnly >> 7;
                    }
                    else
                    {
                        result[4] = tritsOnly >> 7;
                        result[3] = (tritsOnly >> 5) & 0x3;
                    }
                }

                if ((c & 0x3) == 0x3)
                {
                    result[2] = 2;
                    result[1] = (c >> 4) & 0x1;
                    result[0] = ((c & 0x8) >> 2) | (((c & 0x4) >> 2) & ~((c & 0x8) >> 3));
                }
                else if (((c & 0xc) >> 2) == 0x3)
                {
                    result[2] = 2;
                    result[1] = 2;
                    result[0] = c & 0x3;
                }
                else
                {
                    result[2] = (c & 0x10) >> 4;
                    result[1] = (c & 0xc) >> 2;
                    result[0] = (c & 0x2) | ((c & 0x1) & ~((c & 0x2) >> 1));
                }

                //calculate final values
                result[0] = (result[0] << bits) | (tritBlock & ((1 << bits) - 1));
                result[1] = (result[1] << bits) | ((tritBlock >> (2 + bits)) & ((1 << bits) - 1));
                result[2] = (result[2] << bits) | ((tritBlock >> (4 + bits * 2)) & ((1 << bits) - 1));
                result[3] = (result[3] << bits) | ((tritBlock >> (5 + bits * 3)) & ((1 << bits) - 1));
                result[4] = (result[4] << bits) | ((tritBlock >> (7 + bits * 4)) & ((1 << bits) - 1));

                return result;
            }

            /*private int Interpolate(int a, int b, int num, int den) => (num * a + (den - num) * b + den / 2) / den;
            private Color InterpolateColor(Color a, Color b, int num, int den) => Color.FromArgb(Interpolate(a.R, b.R, num, den), Interpolate(a.G, b.G, num, den), Interpolate(a.B, b.B, num, den));

            private Color GetRGB565(ushort val) => Color.FromArgb(255, (val >> 11) * 33 / 4, (val >> 5) % 64 * 65 / 16, (val % 32) * 33 / 4);*/

            int blockWidth;
            int blockHeight;

            public Decoder(int blockWidth, int blockHeight)
            {
                this.blockWidth = blockWidth;
                this.blockHeight = blockHeight;
            }

            public Color Get(Func<byte[]> func)
            {
                if (!queue.Any())
                {
                    var bitR = new BitReader(func());

                    //Blockmode -> Documentation C.2.10
                    var blockMode = bitR.ReadBits(0, 11);
                    if ((blockMode & 0x1FC) == 0x1FC)
                    {
                        //void-extent
                    }
                    else
                    {
                        bool highPrec = ((blockMode >> 9) & 0x1) == 1;
                        bool dualPlane = ((blockMode >> 10) & 0x1) == 1;
                        var rField = GetRField(blockMode);
                        var (weightMin, weightMax, trits, quints, bits) = RFieldTable[(((highPrec) ? 1 : 0) << 3) | rField];

                        //Partition and CEM -> Documentation C.2.11
                        var parts = bitR.ReadBits(11, 2) + 1;    //4 == invalid
                        if (parts == 1)
                        {
                            //single-partition
                            var cem = bitR.ReadBits(13, 4);
                            if (trits == 0 && quints == 0)
                            {
                                //to get value i, do bitR.ReadBits(pos+i*bits,bits)
                            }
                            else if (trits != 0)
                            {
                                var tritBlock = bitR.ReadBits(17, 8 + 5 * bits);
                                var tritValues = GetValuesFromTrits(tritBlock, bits);
                            }
                        }
                        else
                        {
                            //multi-partition - stub - Table C.2.11
                        }
                    }

                    /*var (a0, a1) = ((byte)alpha, (byte)(alpha >> 8));
                    var (color0, color1) = ((ushort)block, (ushort)(block >> 16));
                    var (c0, c1) = (GetRGB565(color0), GetRGB565(color1));

                    for (int i = 0; i < 16; i++)
                    {
                        var code = (int)(alpha >> 16 + 3 * i) & 7;
                        var alp = 255;
                        if (Format == Formats.DXT3)
                        {
                            // TODO: Fix broken DXT3 alpha Neobeo!
                            // The DXT3 images might be using yet another color mess thing, maybe
                            alp = ((int)(alpha >> (4 * i)) & 0xF) * 17;
                        }
                        else if (Format == Formats.DXT5)
                        {
                            alp = code == 0 ? a0
                            : code == 1 ? a1
                            : a0 > a1 ? Interpolate(a0, a1, 8 - code, 7)
                            : code < 6 ? Interpolate(a0, a1, 6 - code, 5)
                            : code % 2 * 255;
                        }
                        code = (int)(block >> 32 + 2 * i) & 3;
                        var clr = code == 0 ? c0
                            : code == 1 ? c1
                            : Format == Formats.DXT5 || color0 > color1 ? InterpolateColor(c0, c1, 4 - code, 3)
                            : code == 2 ? InterpolateColor(c0, c1, 1, 2)
                            : Color.Black;
                        queue.Enqueue(Color.FromArgb(alp, clr));
                    }*/
                }
                return queue.Dequeue();
            }
        }

        public class Encoder
        {
            List<Color> queue = new List<Color>();

            int blockWidth;
            int blockHeight;

            public Encoder(int blockWidth, int blockHeight)
            {
                this.blockWidth = blockWidth;
                this.blockHeight = blockHeight;
            }

            public void Set(Color c, Action<byte[]> func)
            {
                queue.Add(c);
                if (queue.Count == blockWidth * blockHeight)
                {
                    // Alpha
                    /*ulong outAlpha = 0;
                    if (Format == Formats.DXT5)
                    {
                        var alphaEncoder = new BCn.BC4BlockEncoder();
                        alphaEncoder.LoadBlock(queue.Select(clr => clr.A / 255f).ToArray());
                        outAlpha = alphaEncoder.EncodeUnsigned().PackedValue;
                    }

                    // Color
                    var colorEncoder = new BCn.BC1BlockEncoder();
                    colorEncoder.LoadBlock(
                        queue.Select(clr => clr.R / 255f).ToArray(),
                        queue.Select(clr => clr.G / 255f).ToArray(),
                        queue.Select(clr => clr.B / 255f).ToArray()
                    );
                    var outColor = colorEncoder.Encode().PackedValue;

                    func((outAlpha, outColor));*/
                    queue.Clear();
                }
            }
        }
    }
}