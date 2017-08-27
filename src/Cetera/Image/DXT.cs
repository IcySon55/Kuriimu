using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Cetera.Image
{
    public class DXT
    {
        public enum Formats
        {
            DXT1, DXT5
        }

        public class Decoder
        {
            private Queue<Color> queue = new Queue<Color>();
            public Formats Format { get; set; }

            private int Interpolate(int a, int b, int num, int den) => (num * a + (den - num) * b + den / 2) / den;
            private Color InterpolateColor(Color a, Color b, int num, int den) => Color.FromArgb(Interpolate(a.R, b.R, num, den), Interpolate(a.G, b.G, num, den), Interpolate(a.B, b.B, num, den));

            private Color GetRGB565(ushort val) => Color.FromArgb(255, (val >> 11) * 33 / 4, (val >> 5) % 64 * 65 / 16, (val % 32) * 33 / 4);

            public Color Get(Func<(ulong alpha, ulong block)> func)
            {
                if (!queue.Any())
                {
                    var (alpha, block) = func();

                    var (a0, a1) = ((byte)alpha, (byte)(alpha >> 8));
                    var (color0, color1) = ((ushort)block, (ushort)(block >> 16));
                    var (c0, c1) = (GetRGB565(color0), GetRGB565(color1));

                    for (int i = 0; i < 16; i++)
                    {
                        var code = (int)(alpha >> 16 + 3 * i) & 7;
                        var alp = Format == Formats.DXT1 ? 255
                                : code == 0 ? a0
                                : code == 1 ? a1
                                : a0 > a1 ? Interpolate(a0, a1, 8 - code, 7)
                                : code < 6 ? Interpolate(a0, a1, 6 - code, 5)
                                : code % 2 * 255;
                        code = (int)(block >> 32 + 2 * i) & 3;
                        var clr = code == 0 ? c0
                                : code == 1 ? c1
                                : Format == Formats.DXT5 || color0 > color1 ? InterpolateColor(c0, c1, 4 - code, 3)
                                : code == 2 ? InterpolateColor(c0, c1, 1, 2)
                                : Color.Black;
                        queue.Enqueue(Color.FromArgb(alp, clr));
                    }
                }
                return queue.Dequeue();
            }
        }
    }
}