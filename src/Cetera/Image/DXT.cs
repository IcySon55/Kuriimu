using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Cetera.Image
{
    public class DXT
    {
        static List<byte> alphaLookup;
        static List<Color> colorLookup;

        public enum Format
        {
            DXT1, DXT5
        }

        public static Format format;
        public struct PixelData
        {
            public byte[] Alpha { get; set; }
            public byte[] Block { get; set; }
        }

        public class Decoder
        {
            Queue<Color> queue = new Queue<Color>();

            //private int Interpolate(int a, int b, int num, int den) => (num * a + (den - num) * b + den / 2) / den;
            //private Color InterpolateColor(Color a, Color b, int num, int den) => Color.FromArgb(Interpolate(a.R, b.R, num, den), Interpolate(a.G, b.G, num, den), Interpolate(a.B, b.B, num, den));

            private Color GetRGB565(ushort val) => Color.FromArgb(255, (val >> 11) * 33 / 4, (val >> 5) % 64 * 65 / 16, (val % 32) * 33 / 4);

            private void CreateAlphaLookupTable(byte alpha0, byte alpha1)
            {
                alphaLookup = new List<byte>();

                alphaLookup.Add(alpha0);
                alphaLookup.Add(alpha1);

                if (alpha0 > alpha1)
                {
                    for (int i = 6, j = 1; i > 0; i--, j++)
                        alphaLookup.Add((byte)((i * alpha0 + j * alpha1) / 7));
                }
                else
                {
                    for (int i = 4, j = 1; i > 0; i--, j++)
                        alphaLookup.Add((byte)((i * alpha0 + j * alpha1) / 5));
                    alphaLookup.Add(0);
                    alphaLookup.Add(255);
                }

            }

            private void CreateColorLookupTable(ushort color0, ushort color1)
            {
                var color_0 = GetRGB565(color0);
                var color_1 = GetRGB565(color1);

                colorLookup = new List<Color>();

                colorLookup.Add(color_0);
                colorLookup.Add(color_1);

                for (int i = 2, j = 1; i > 0; i--, j++)
                    colorLookup.Add(Color.FromArgb(255,
                        ((i * color_0.R + j * color_1.R) / 3),
                        ((i * color_0.G + j * color_1.G) / 3),
                        ((i * color_0.B + j * color_1.B) / 3)));
            }

            public Color Get(Func<PixelData> func)
            {
                if (!queue.Any())
                {
                    var data = func();

                    //If DXT5, then Alpha
                    var acodes = new List<byte>();
                    if (format == Format.DXT5)
                    {
                        // Alpha Bytes
                        var alpha = data.Alpha;

                        //Alpha Lookup
                        CreateAlphaLookupTable(alpha[0], alpha[1]);

                        //Alpha bit codes
                        var alphaIndices = (((((ulong)alpha[7] << 8 | alpha[6]) << 8 | alpha[5]) << 8 | alpha[4]) << 8 | alpha[3]) << 8 | alpha[2];
                        for (var i = 0; i < 48; i += 3)
                            acodes.Add((byte)((alphaIndices >> i) & 0x7));
                    }


                    // Color Bytes
                    var color = data.Block;

                    //Colour Lookup Table
                    CreateColorLookupTable((ushort)(color[1] << 8 | color[0]), (ushort)(color[3] << 8 | color[2]));

                    //Color bit codes
                    var colorIndices = (uint)(((color[7] << 8 | color[6]) << 8 | color[5]) << 8 | color[4]);
                    var codes = new List<byte>();
                    for (var i = 0; i < 32; i += 2)
                        codes.Add((byte)((colorIndices >> i) & 0x3));


                    // Build Block
                    for (var y = 0; y < 4; y++)
                        for (var x = 0; x < 4; x++)
                        {
                            int a = 255;

                            // Alpha, if DXT5
                            if (format == Format.DXT5)
                            {
                                var acode = acodes[y * 4 + x];
                                a = alphaLookup[acode];
                            }

                            // Colors
                            var code = codes[y * 4 + x];
                            int r = colorLookup[code].R;
                            int g = colorLookup[code].G;
                            int b = colorLookup[code].B;

                            // Alpha as Green for Capcom (not really? :confused:)
                            queue.Enqueue(Color.FromArgb(a, r, g, b));
                        }
                }
                return queue.Dequeue();
            }
        }
    }
}