using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Kontract.Image.Support
{
    public class ASTC
    {
        public class Decoder
        {
            private Queue<Color> queue = new Queue<Color>();

            private int GetRField(byte[] block) => ((block[0] & 0x10) >> 4) | (((block[0] & 0x3) == 0) ? ((block[0] & 0x4) >> 1) | ((block[0] & 0x8) >> 1) : ((block[0] & 0x1) << 1) | ((block[0] & 0x2) << 1));
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
                    var block = func();

                    var blockMode = (block[1] & 0x7) | block[0];
                    if ((blockMode & 0x1FC) == 0x1FC)
                    {
                        //void-extent
                    }
                    else
                    {
                        bool highPrec = ((block[1] >> 1) & 0x1) == 1;
                        bool dualPlane = ((block[1] >> 2) & 0x1) == 1;
                        var rField = GetRField(block);
                        (int weightMin, int weightMax, byte trits, byte quints, byte bits) = RFieldTable[(((highPrec) ? 1 : 0) << 3) | rField];
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