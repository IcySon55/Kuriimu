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
    public class GZF
    {
        static Lazy<GZF> StdFntLoader = new Lazy<GZF>(() => new GZF(new MemoryStream(Resources.std_gzf)));
        public static GZF StandardFont => StdFntLoader.Value;

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
            public int version;
            public ushort imgInfoOffset;
            public ushort unk1;
            public int entryLength;

            public int imgCount;
            public int entryCount;
            public uint unk2;
            public uint unk3;

            public Format format;

            public byte pad1;
            public short unk4;
            public short unk5;
            public short unk6;

            public int unk7;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ImgInfo
        {
            public uint offset;
            public ushort width;
            public ushort height;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LetterInf
        {
            public short height;
            public short imgId;
            public short width;
            public short id;
        }

        Header header;
        List<ImgInfo> imgInfos = new List<ImgInfo>();
        public static Dictionary<char, LetterInf> letterInfos = new Dictionary<char, LetterInf>();

        public GZF(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                var unicode = Encoding.Unicode;

                //Header
                header = br.ReadStruct<Header>();

                //Image information
                br.BaseStream.Position = header.imgInfoOffset;
                imgInfos = br.ReadMultiple<ImgInfo>(header.imgCount);

                //Letter information
                for (int i = 0; i < header.entryCount; i++)
                {
                    try
                    {
                        char letter = unicode.GetChars(br.ReadBytes(2))[0];
                        br.BaseStream.Position += 2;
                        letterInfos.Add(letter, br.ReadStruct<LetterInf>());
                    }
                    catch
                    {
                        var t = 0;
                    }
                }

                //Images
                for (int i = 0; i < header.imgCount; i++)
                {
                    br.BaseStream.Position = imgInfos[i].offset;
                    var settings = new ImageSettings
                    {
                        PadToPowerOf2 = false,
                        Width = imgInfos[i].width,
                        Height = imgInfos[i].height,
                        Format = header.format
                    };

                    bmps.Add(Image.Common.Load(br.ReadBytes(imgInfos[i].width * imgInfos[i].height / 2), settings));
                }
            }
        }
    }
}
