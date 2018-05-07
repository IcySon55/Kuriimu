using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Kontract.Image.Support
{
    public class ATC
    {
        public class Decoder
        {
            private Queue<Color> queue = new Queue<Color>();

            private Color GetRGB565(ushort val) => Color.FromArgb(255, (val >> 11) * 33 / 4, (val >> 5) % 64 * 65 / 16, (val % 32) * 33 / 4);
            private Color GetRGB555(ushort val) => Color.FromArgb(255, ((val >> 11) % 32) * 33 / 4, ((val >> 5) % 32) * 33 / 4, (val % 32) * 33 / 4);

            private Color InterpolateColor(Color c0, Color c1, int code) => Color.FromArgb(255, c0.R + (code * (c1.R - c0.R)), c0.G + (code * (c1.G - c0.G)), c0.B + (code * (c1.B - c0.B)));
            private Color InterpolateColorDiv(Color c0, Color c1, int div) => Color.FromArgb(255, c0.R - (1 / div * c1.R), c0.G - (1 / div * c1.G), c0.B - (1 / div * c1.B));

            /*private int Interpolate(int a, int b, int num, int den) => (num * a + (den - num) * b + den / 2) / den;
            private Color InterpolateColor(Color a, Color b, int num, int den) => Color.FromArgb(Interpolate(a.R, b.R, num, den), Interpolate(a.G, b.G, num, den), Interpolate(a.B, b.B, num, den));

            private Color RightShiftColor(Color color, byte shift) => Color.FromArgb(color.A, color.R >> shift, color.G >> shift, color.B >> shift);*/

            bool alpha;

            public Decoder(bool alpha)
            {
                this.alpha = alpha;
            }

            public Color Get(Func<(ulong alpha, ulong block)> func)
            {
                if (!queue.Any())
                {
                    var (alphaBlock, block) = func();

                    var (a0, a1) = ((byte)alphaBlock, (byte)(alphaBlock >> 8));
                    var (color0, color1) = ((ushort)block, (ushort)(block >> 16));
                    var (c0, c1) = (GetRGB555(color0), GetRGB565(color1));
                    var method = color0 >> 15;

                    for (int i = 0; i < 16; i++)
                    {
                        var alphaValue = 255;
                        if (alpha)
                            alphaValue = (int)((alphaBlock >> (i * 4)) & 0xF) * 17;

                        Color colorValue;
                        var code = (int)(block >> 32 + 2 * i) & 0x3;
                        if (method == 0)
                        {
                            colorValue = InterpolateColor(c0, c1, code);
                        }
                        else
                        {
                            colorValue = (code == 0) ? Color.FromArgb(255, 0, 0, 0)
                                : (code == 1) ? InterpolateColorDiv(c0, c1, 4)
                                : (code == 2) ? c0
                                : c1;
                        }

                        queue.Enqueue(Color.FromArgb(alphaValue, colorValue));
                    }
                }
                return queue.Dequeue();
            }
        }

        public class Encoder
        {
            List<Color> queue = new List<Color>();
            bool alpha;

            public Encoder(bool alpha)
            {
                this.alpha = alpha;
            }

            public void Set(Color c, Action<(ulong alpha, ulong block)> func)
            {
                queue.Add(c);
                if (queue.Count == 16)
                {
                    // Alpha
                    ulong outAlpha = 0;
                    if (alpha)
                    {
                        /*var alphaEncoder = new BCn.BC4BlockEncoder();
                        alphaEncoder.LoadBlock(queue.Select(clr => clr.A / 255f).ToArray());
                        outAlpha = alphaEncoder.EncodeUnsigned().PackedValue;*/
                    }

                    // Color
                    ulong outColor = 0;
                    /*var colorEncoder = new BCn.BC1BlockEncoder();
                    colorEncoder.LoadBlock(
                        queue.Select(clr => clr.R / 255f).ToArray(),
                        queue.Select(clr => clr.G / 255f).ToArray(),
                        queue.Select(clr => clr.B / 255f).ToArray()
                    );
                    var outColor = colorEncoder.Encode().PackedValue;*/

                    func((outAlpha, outColor));
                    queue.Clear();
                }
            }
        }
    }
}