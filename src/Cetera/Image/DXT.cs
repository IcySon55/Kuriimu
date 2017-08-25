using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Cetera.Image
{
    public class DXT
    {
        public struct PixelData
        {
            public byte[] Alpha { get; set; }
            public byte[] Block { get; set; }
        }

        public class Decoder
        {
            Queue<Color> queue = new Queue<Color>();

            private int Interpolate(int a, int b, int num, int den) => (num * a + (den - num) * b + den / 2) / den;
            private Color InterpolateColor(Color a, Color b, int num, int den) => Color.FromArgb(Interpolate(a.R, b.R, num, den), Interpolate(a.G, b.G, num, den), Interpolate(a.B, b.B, num, den));

            public Color Get(Func<PixelData> func)
            {
                if (!queue.Any())
                {
                    var data = func();

                    // Alpha Bytes
                    var alpha = data.Alpha;
                    var alpha0 = alpha[0];
                    var alpha1 = alpha[1];
                    var alphaIndices = (ulong)(alpha[2] + 256 * (alpha[3] + 256 * (alpha[4] + 256 * (alpha[5] + 256 * (alpha[6] + 256 * alpha[7])))));

                    var acodes = new List<byte>();
                    for (var i = 0; i < 48; i += 2)
                        acodes.Add((byte)((alphaIndices >> i) & 0x3));

                    // Color Bytes
                    var color = data.Block;
                    var color0 = (color[0] + color[1] * 256);
                    var color1 = (color[2] + color[3] * 256);
                    var colorIndices = (uint)(color[4] + 256 * (color[5] + 256 * (color[6] + 256 * color[7])));

                    var codes = new List<byte>();
                    for (var i = 0; i < 32; i += 2)
                        codes.Add((byte)((colorIndices >> i) & 0x3));

                    var red = (color0 >> 11) * 33 / 4;
                    var grn = (color0 >> 5) % 64 * 65 / 16;
                    var blu = (color0 % 32) * 33 / 4;
                    var RGB0 = Color.FromArgb(0, red, grn, blu);

                    red = (color1 >> 11) * 33 / 4;
                    grn = (color1 >> 5) % 64 * 65 / 16;
                    blu = (color1 % 32) * 33 / 4;
                    var RGB1 = Color.FromArgb(0, red, grn, blu);

                    // Build Block
                    for (var y = 0; y < 4; y++)
                        for (var x = 0; x < 4; x++)
                        {
                            var acode = acodes[y * 4 + x];
                            var code = codes[y * 4 + x];
                            int a = 0, r = 0, g = 0, b = 0;

                            // Alpha
                            if (acode == 0)
                                a = alpha0;
                            if (acode == 1)
                                a = alpha1;
                            if (alpha0 > alpha1 && acode == 2)
                                a = (6 * alpha0 + 1 * alpha1) / 7;
                            if (alpha0 > alpha1 && acode == 3)
                                a = (5 * alpha0 + 2 * alpha1) / 7;
                            if (alpha0 > alpha1 && acode == 4)
                                a = (4 * alpha0 + 3 * alpha1) / 7;
                            if (alpha0 > alpha1 && acode == 5)
                                a = (3 * alpha0 + 4 * alpha1) / 7;
                            if (alpha0 > alpha1 && acode == 6)
                                a = (2 * alpha0 + 5 * alpha1) / 7;
                            if (alpha0 > alpha1 && acode == 7)
                                a = (1 * alpha0 + 6 * alpha1) / 7;

                            if (alpha0 <= alpha1 && acode == 2)
                                a = (4 * alpha0 + 1 * alpha1) / 5;
                            if (alpha0 <= alpha1 && acode == 3)
                                a = (3 * alpha0 + 2 * alpha1) / 5;
                            if (alpha0 <= alpha1 && acode == 4)
                                a = (2 * alpha0 + 3 * alpha1) / 5;
                            if (alpha0 <= alpha1 && acode == 5)
                                a = (1 * alpha0 + 4 * alpha1) / 5;
                            if (alpha0 <= alpha1 && acode == 6)
                                a = 0;
                            if (alpha0 <= alpha1 && acode == 7)
                                a = 255;

                            // Colors 1
                            if (code == 0)
                            {
                                r = RGB0.R;
                                g = RGB0.G;
                                b = RGB0.B;
                            }
                            else if (code == 1)
                            {
                                r = RGB1.R;
                                g = RGB1.G;
                                b = RGB1.B;
                            }
                            else if (code == 2)
                            {
                                r = RGB0.R;
                                g = RGB0.G;
                                b = RGB0.B;
                                r *= 2;
                                g *= 2;
                                b *= 2;
                                r += RGB1.R;
                                g += RGB1.G;
                                b += RGB1.B;
                                r /= 3;
                                g /= 3;
                                b /= 3;
                            }
                            else if (code == 3)
                            {
                                r = RGB1.R;
                                g = RGB1.G;
                                b = RGB1.B;
                                r *= 2;
                                g *= 2;
                                b *= 2;
                                r += RGB0.R;
                                g += RGB0.G;
                                b += RGB0.B;
                                r /= 3;
                                g /= 3;
                                b /= 3;
                            }

                            // Alpha as Green for Capcom (not really? :confused:)
                            queue.Enqueue(Color.FromArgb(a, r, g, b));
                        }
                }
                return queue.Dequeue();
            }
        }
    }
}