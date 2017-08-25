using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
                    var alpha0 = alpha[0] * (1 / 255);
                    var alpha1 = alpha[1] * (1 / 255);
                    var abits_0 = alpha[2];
                    var abits_1 = alpha[3];
                    var abits_2 = alpha[4];
                    var abits_3 = alpha[5];
                    var abits_4 = alpha[6];
                    var abits_5 = alpha[7];

                    var abits = (ulong)(abits_0 + 256 * (abits_1 + 256 * (abits_2 + 256 * (abits_3 + 256 * (abits_4 + 256 * abits_5)))));

                    // Color Bytes
                    var color = data.Block;
                    var c0_lo = color[0];
                    var c0_hi = color[1];
                    var c1_lo = color[2];
                    var c1_hi = color[3];
                    var cbits_0 = color[4];
                    var cbits_1 = color[5];
                    var cbits_2 = color[6];
                    var cbits_3 = color[7];

                    var color0 = c0_lo + c0_hi * 256u;
                    var color1 = c1_lo + c1_hi * 256u;
                    var cbits = (uint)(cbits_0 + 256 * (cbits_1 + 256 * (cbits_2 + 256 * cbits_3)));

                    var codes = new List<ushort>();
                    uint mask = 0x3;
                    for (var i = 0; i < 32; i += 2)
                    {
                        codes.Add((ushort)((cbits & mask) >> i));
                        mask = mask << 2;
                    }

                    // Neobeo
                    //var red = (color0 >> 11) * 33 / 4;
                    //var grn = (color0 >> 5) % 64 * 65 / 16;
                    //var blu = color0 % 32 * 33 / 4;

                    // Internet 2
                    var red = ((color0 >> 11) * 527 + 23) >> 6;
                    var grn = ((color0 >> 5) % 64 * 259 + 33) >> 6;
                    var blu = ((color0 % 32) * 527 + 23) >> 6;

                    var RGB0 = Color.FromArgb(255, (int)red, (int)grn, (int)blu);


                    red = ((color1 >> 11) * 527 + 23) >> 6;
                    grn = ((color1 >> 5) % 64 * 259 + 33) >> 6;
                    blu = ((color1 % 32) * 527 + 23) >> 6;

                    var RGB1 = Color.FromArgb(255, (int)red, (int)grn, (int)blu);

                    // Internet 1
                    //red = (red << 3) | (red >> 2);
                    //grn = (grn << 2) | (grn >> 4);
                    //blu = (blu << 3) | (blu >> 2);

                    //var RGB0 = Color.FromArgb(255, red, grn, blu);

                    //red = (color1 >> 11) & 0x1F;
                    //grn = (color1 >> 5) & 0x3F;
                    //blu = color1 & 0x1F;
                    //red = (red << 3) | (red >> 2);
                    //grn = (grn << 2) | (grn >> 4);
                    //blu = (blu << 3) | (blu >> 2);

                    //var RGB1 = Color.FromArgb(255, red, grn, blu);

                    // Build Block
                    for (var y = 0; y < 4; y++)
                        for (var x = 0; x < 4; x++)
                        {
                            var result = Color.Black;
                            var code = codes[y * 4 + x];

                            if (color0 > color1 && code == 0)
                                result = RGB0;
                            else if (color0 > color1 && code == 1)
                                result = RGB1;
                            else if (color0 > color1 && code == 2)
                            {
                                var r = (int)RGB0.R;
                                var g = (int)RGB0.G;
                                var b = (int)RGB0.B;
                                r *= 2;
                                g *= 2;
                                b *= 2;
                                r += RGB1.R;
                                g += RGB1.G;
                                b += RGB1.B;
                                r /= 3;
                                g /= 3;
                                b /= 3;
                                result = Color.FromArgb(255, r, g, b);
                                //result = InterpolateColor(RGB0, RGB1, 2, 3);
                            }
                            else if (color0 > color1 && code == 3)
                            {
                                var r = (int)RGB1.R;
                                var g = (int)RGB1.G;
                                var b = (int)RGB1.B;
                                r *= 2;
                                g *= 2;
                                b *= 2;
                                r += RGB0.R;
                                g += RGB0.G;
                                b += RGB0.B;
                                r /= 3;
                                g /= 3;
                                b /= 3;
                                result = Color.FromArgb(255, r, g, b);
                                //result = InterpolateColor(RGB1, RGB0, 2, 3);
                            }

                            else if (color0 <= color1 && code == 0)
                                result = RGB0;
                            else if (color0 <= color1 && code == 1)
                                result = RGB1;
                            else if (color0 <= color1 && code == 2)
                            {
                                var r = (int)RGB0.R;
                                var g = (int)RGB0.G;
                                var b = (int)RGB0.B;
                                r += RGB1.R;
                                g += RGB1.G;
                                b += RGB1.B;
                                r /= 2;
                                g /= 2;
                                b /= 2;
                                result = Color.FromArgb(255, r, g, b);
                                //result = InterpolateColor(RGB0, RGB1, 0, 2);
                            }
                            else if (color0 <= color1 && code == 3)
                                result = Color.Black;

                            queue.Enqueue(result);
                        }
                }
                return queue.Dequeue();
            }
        }
    }
}