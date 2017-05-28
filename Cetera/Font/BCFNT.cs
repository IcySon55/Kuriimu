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

namespace Cetera.Font
{
    public class BCFNT
    {
        static Lazy<BCFNT> StdFntLoader = new Lazy<BCFNT>(() => new BCFNT(new MemoryStream(GZip.Decompress(new MemoryStream(Resources.cbf_std_bcfnt)))));
        public static BCFNT StandardFont => StdFntLoader.Value;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [DebuggerDisplay("[{left}, {glyph_width}, {char_width}]")]
        public struct CharWidthInfo
        {
            public sbyte left;
            public byte glyph_width;
            public byte char_width;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CFNT
        {
            public uint magic;
            public ushort endianness;
            public short header_size;
            public int version;
            public int file_size;
            public int num_blocks;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FINF
        {
            public uint magic;
            public int section_size;
            public byte font_type;
            public byte line_feed;
            public short alter_char_index;
            public CharWidthInfo default_width;
            public byte encoding;
            public int tglp_offset;
            public int cwdh_offset;
            public int cmap_offset;
            public byte height;
            public byte width;
            public byte ascent;
            public byte reserved;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGLP
        {
            public uint magic;
            public int section_size;
            public byte cell_width;
            public byte cell_height;
            public byte baseline_position;
            public byte max_character_width;
            public int sheet_size;
            public short num_sheets;
            public short sheet_image_format;
            public short num_columns;
            public short num_rows;
            public short sheet_width;
            public short sheet_height;
            public int sheet_data_offset;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
        public struct CMAP
        {
            public uint magic;
            public int section_size;
            public char code_begin;
            public char code_end;
            public short mapping_method;
            public short reserved;
            public int next_offset;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CWDH
        {
            public uint magic;
            public int section_size;
            public short start_index;
            public short end_index;
            public int next_offset;
        };

        FINF finf;
        TGLP tglp;
        public Bitmap[] bmps;
        ImageAttributes attr = new ImageAttributes();
        List<CharWidthInfo> lstCWDH = new List<CharWidthInfo>();
        Dictionary<char, int> dicCMAP = new Dictionary<char, int>();

        public CharWidthInfo GetWidthInfo(char c) => lstCWDH[GetIndex(c)];
        public int LineFeed => finf.line_feed;

        int GetIndex(char c)
        {
            int result;
            if (!dicCMAP.TryGetValue(c, out result))
                dicCMAP.TryGetValue('?', out result);
            return result;
        }

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
            var index = GetIndex(c);
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
                    attr);
        }

        public float MeasureString(string text, char stopChar, float scale = 1.0f)
        {
            return text.TakeWhile(c => c != stopChar).Sum(c => GetWidthInfo(c).char_width) * scale;
        }

        byte[] glgr;
        bool usesGlgr = false;

        public byte[] CheckTGLP(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                if (usesGlgr)
                {
                    var compSize = br.ReadInt32();
                    var t =Huffman.Decompress(new MemoryStream(br.ReadBytes(compSize)), 8);
                    return t;
                } else
                {
                    return br.ReadBytes(tglp.sheet_size);
                }
            }
        }

        public BCFNT(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                // @todo: read as sections
                br.ReadStruct<CFNT>();

                if (br.ReadString(4)=="GLGR")
                {
                    usesGlgr = true;
                    var glgrSize = br.ReadInt32();
                    br.BaseStream.Position -= 8;
                    glgr = br.ReadBytes(glgrSize);
                } else
                {
                    br.BaseStream.Position -= 4;
                }

                finf = br.ReadStruct<FINF>();

                // read TGLP
                br.BaseStream.Position = finf.tglp_offset - 8;
                tglp = br.ReadStruct<TGLP>();

                // read image data
                br.BaseStream.Position = tglp.sheet_data_offset;
                int width = tglp.sheet_width;
                int height = tglp.sheet_height;
                if (usesGlgr)
                {
                    bmps = new Bitmap[tglp.num_sheets];
                    for (int i = 0; i < tglp.num_sheets; i++)
                    {
                        var compSize = br.ReadInt32();
                        var decomp = Huffman.Decompress(new MemoryStream(br.ReadBytes(compSize)), 8);
                        bmps[i] = Image.Common.Load(decomp, new ImageSettings
                        {
                            Width = width,
                            Height = height,
                            Format = ImageSettings.ConvertFormat(tglp.sheet_image_format & 0x7fff)
                        });
                    }
                }
                else
                {
                    bmps = Enumerable.Range(0, tglp.num_sheets).Select(_ => Image.Common.Load(br.ReadBytes(tglp.sheet_size), new ImageSettings
                    {
                        Width = width,
                        Height = height,
                        Format = ImageSettings.ConvertFormat(tglp.sheet_image_format)
                    })).ToArray();
                }

                // read CWDH
                for (int offset = finf.cwdh_offset; offset != 0;)
                {
                    br.BaseStream.Position = offset - 8;
                    var cwdh = br.ReadStruct<CWDH>();
                    for (int i = cwdh.start_index; i <= cwdh.end_index; i++)
                        lstCWDH.Add(br.ReadStruct<CharWidthInfo>());
                    offset = cwdh.next_offset;
                }

                // read CMAP
                for (int offset = finf.cmap_offset; offset != 0;)
                {
                    br.BaseStream.Position = offset - 8;
                    var cmap = br.ReadStruct<CMAP>();
                    switch (cmap.mapping_method)
                    {
                        case 0:
                            var charOffset = br.ReadUInt16();
                            for (char i = cmap.code_begin; i <= cmap.code_end; i++)
                            {
                                int idx = i - cmap.code_begin + charOffset;
                                dicCMAP[i] = idx < ushort.MaxValue ? idx : 0;
                            }
                            break;
                        case 1:
                            for (char i = cmap.code_begin; i <= cmap.code_end; i++)
                            {
                                int idx = br.ReadInt16();
                                if (idx != -1) dicCMAP[i] = idx;
                            }
                            break;
                        case 2:
                            var n = br.ReadUInt16();
                            for (int i = 0; i < n; i++)
                            {
                                char c = br.ReadChar();
                                int idx = br.ReadInt16();
                                if (idx != -1) dicCMAP[c] = idx;
                            }
                            break;
                        default:
                            throw new Exception("Unsupported mapping method");
                    }
                    offset = cmap.next_offset;
                }
            }
        }
    }
}
