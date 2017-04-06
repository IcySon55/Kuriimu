using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cetera.Compression;
using Cetera.Image;
using Kuriimu.IO;

namespace Cetera.Font
{
    public sealed class XF
    {
        public Bitmap bmp;
        public string txt;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [DebuggerDisplay("[{offset_x}, {offset_y}, {glyph_width}, {glyph_height}]")]
        public struct CharSizeInfo
        {
            public sbyte offset_x;
            public sbyte offset_y;
            public byte glyph_width;
            public byte glyph_height;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
        [DebuggerDisplay("[{code_point}] {ColorChannel}:{ImageOffsetX}:{ImageOffsetY}")]
        public struct CharacterMap
        {
            public char code_point;
            ushort char_size;
            int image_offset;

            public int CharSizeInfoIndex => char_size % 1024;
            public int CharWidth => char_size / 1024;
            public int ColorChannel => image_offset % 16;
            public int ImageOffsetX => image_offset / 16 % 16384;
            public int ImageOffsetY => image_offset / 16 / 16384;
        }

        List<CharSizeInfo> lstCharSizeInfo;
        Dictionary<char, CharacterMap> dicGlyphLarge;
        Dictionary<char, CharacterMap> dicGlyphSmall;

        public XF(Stream input)
        {
            using (var br = new BinaryReader(input))
            {
                br.ReadBytes(64);
                bmp = new XI(new MemoryStream(br.ReadBytes(0x3396C))).Image; // temporary hack -- only works with nrm_main.xf for now
                br.ReadBytes(0x28); // temporary hack -- should be the header
                var buf1 = CriWare.GetDecompressedBytes(br.BaseStream);
                var buf2 = CriWare.GetDecompressedBytes(br.BaseStream);
                var buf3 = CriWare.GetDecompressedBytes(br.BaseStream);

                lstCharSizeInfo = Enumerable.Range(0, buf1.Length / 4).Select(i => buf1.Skip(4 * i).Take(4).ToArray().ToStruct<CharSizeInfo>()).ToList();
                dicGlyphLarge = Enumerable.Range(0, buf2.Length / 8).Select(i => buf2.Skip(8 * i).Take(8).ToArray().ToStruct<CharacterMap>()).ToDictionary(x => x.code_point);
                dicGlyphSmall = Enumerable.Range(0, buf3.Length / 8).Select(i => buf3.Skip(8 * i).Take(8).ToArray().ToStruct<CharacterMap>()).ToDictionary(x => x.code_point);
            }
        }

        CharacterMap GetCharacterMap(char c)
        {
            CharacterMap result;
            if (!dicGlyphLarge.TryGetValue(c, out result))
                dicGlyphLarge.TryGetValue('?', out result);
            return result;
        }

        public float Draw(char ch, Color color, Graphics g, float x, float y)
        {
            var map = GetCharacterMap(ch);
            var sizeInfo = lstCharSizeInfo[map.CharSizeInfoIndex];

            var attr = new ImageAttributes();
            var matrix = Enumerable.Repeat(new float[5], 5).ToArray();
            matrix[map.ColorChannel] = new[] { 0, 0, 0, 1f, 0 };
            matrix[4] = new[] { color.R / 255f, color.G / 255f, color.B / 255f, 0, 0 };
            attr.SetColorMatrix(new ColorMatrix(matrix));

            g.DrawImage(bmp,
                new[] { new PointF(x + sizeInfo.offset_x, y + sizeInfo.offset_y),
                        new PointF(x + sizeInfo.offset_x + sizeInfo.glyph_width, y + sizeInfo.offset_y),
                        new PointF(x + sizeInfo.offset_x, y + sizeInfo.offset_y + sizeInfo.glyph_height) },
                new RectangleF(map.ImageOffsetX, map.ImageOffsetY, sizeInfo.glyph_width, sizeInfo.glyph_height),
                GraphicsUnit.Pixel,
                attr);

            return x + map.CharWidth;
        }

    }
}
