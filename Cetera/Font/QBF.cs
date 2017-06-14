using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cetera.Image;
using Cetera.Properties;
using Kuriimu.Compression;
using Kuriimu.IO;
using Kuriimu.Contract;
using System.Text;

namespace Cetera.Font
{
    public class QBF
    {
        static Lazy<QBF> StdFntLoader = new Lazy<QBF>(() => new QBF(new MemoryStream(Resources.std_qbf)));
        public static QBF StandardFont => StdFntLoader.Value;

        public List<Bitmap> bmps = new List<Bitmap>();
        ImageAttributes attr = new ImageAttributes();
        /*List<CharWidthInfo> lstCWDH = new List<CharWidthInfo>();
        Dictionary<char, int> dicCMAP = new Dictionary<char, int>();
        byte[] _glgr;
        bool _usesGlgr;

        public CharWidthInfo GetWidthInfo(char c) => lstCWDH[GetIndex(c)];
        public int LineFeed => finf.line_feed;*/

        /*int GetIndex(char c)
        {
            LetterInf result;
            if (!letterInfos.TryGetValue(c, out result))
                letterInfos.TryGetValue('?', out result);
            return result.id;
        }*/

        public void SetColor(Color color)
        {
            attr.SetColorMatrix(new ColorMatrix(new[]
            {
                new[] { color.R / 255f, 0, 0, 0, 0 },
                new[] { 0, color.G / 255f, 0, 0, 0 },
                new[] { 0, 0, color.B / 255f, 0, 0 },
                new[] { 0, 0, 0, 1f, 0 },
                new[] { 0, 0, 0, 0, 1f }
            }));
        }

        public void Draw(char c, Graphics g, float x, float y, float scaleX, float scaleY)
        {
            /*var index = GetIndex(c);
            var widthInfo = lstCWDH[index];

            int cellsPerSheet = tglp.num_rows * tglp.num_columns;
            int sheetNum = index / cellsPerSheet;
            int cellRow = (index % cellsPerSheet) / tglp.num_columns;
            int cellCol = index % tglp.num_columns;
            int xOffset = cellCol * (tglp.cell_width + 1);
            int yOffset = cellRow * (tglp.cell_height + 1);

            if (widthInfo.glyph_width > 0)
                g.DrawImage(bmps[sheetNum],
                    new[] { new PointF(x + widthInfo.left * scaleX, y),
                       new PointF(x + (widthInfo.left + widthInfo.glyph_width) * scaleX, y),
                       new PointF(x + widthInfo.left * scaleX, y + tglp.cell_height * scaleY) },
                    new RectangleF(xOffset + 1, yOffset + 1, widthInfo.glyph_width, tglp.cell_height),
                    GraphicsUnit.Pixel,
                    attr);*/
        }

        public float MeasureString(string text, char stopChar, float scale = 1.0f)
        {
            //return text.TakeWhile(c => c != stopChar).Sum(c => GetWidthInfo(c).char_width) * scale;
            return 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public Magic magic;
            public short entryCount;
            public short unk1;
            public int unk2;
            public byte bpp;
            public byte glyphWidth;
            public byte glyphHeight;
            public byte unk3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LetterInf
        {
            public short id;
            public byte unk1;
            public byte unk2;
            public short unk3;
        }

        Header header;
        public static Dictionary<char, LetterInf> letterInfos;
        List<char> chars;

        public QBF(Stream input)
        {
            letterInfos = new Dictionary<char, LetterInf>();
            chars = new List<char>();

            using (var br = new BinaryReaderX(input))
            {
                var unicode = Encoding.Unicode;

                //Header
                header = br.ReadStruct<Header>();

                //Letter information
                for (int i = 0; i < header.entryCount; i++)
                {
                    char letter = unicode.GetChars(br.ReadBytes(2))[0];
                    letterInfos.Add(letter, br.ReadStruct<LetterInf>());
                    chars.Add(letter);
                }

                //Images
                for (int i = 0; i < header.entryCount; i++)
                {
                    if (letterInfos[chars[i]].unk3 == 0)
                    {
                        bmps.Add(new Bitmap(header.glyphWidth, header.glyphHeight));

                        for (int y = 0; y < header.glyphHeight; y++)
                        {
                            for (int x = 0; x < header.glyphWidth; x++)
                            {
                                if (header.bpp == 4)
                                {
                                    var value = br.ReadNibble() * 17;
                                    var argb = (value << 24) | 0xffffff;
                                    bmps.Last().SetPixel(x + 1, y, Color.FromArgb(argb));

                                    value = br.ReadNibble() * 17;
                                    argb = (value << 24) | 0xffffff;
                                    bmps.Last().SetPixel(x++, y, Color.FromArgb(argb));
                                }
                                else if (header.bpp == 8)
                                {
                                    var value = br.ReadByte();
                                    var argb = (value << 24) | 0xffffff;
                                    bmps.Last().SetPixel(x, y, Color.FromArgb(argb));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
