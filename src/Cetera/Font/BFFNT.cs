using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Kontract;
using System.IO;
using Kontract.IO;
using System.Drawing;
using Kontract.Compression;
using Kontract.Image;
using Kontract.Interface;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;

namespace Cetera.Font
{
    /*Not usable with actual texts, just for image representation now*/
    public class BFFNT
    {
        public static Dictionary<byte, IImageFormat> CTRFormat = new Dictionary<byte, IImageFormat>
        {
            [0] = new RGBA(8, 8, 8, 8),
            [1] = new RGBA(8, 8, 8),
            [2] = new RGBA(5, 5, 5, 1),
            [3] = new RGBA(5, 6, 5),
            [4] = new RGBA(4, 4, 4, 4),
            [5] = new LA(8, 8),
            [6] = new HL(8, 8),
            [7] = new LA(8, 0),
            [8] = new LA(0, 8),
            [9] = new LA(4, 4),
            [10] = new LA(4, 0),
            [11] = new LA(0, 4),
            [12] = new ETC1(),
            [13] = new ETC1(true)
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [DebuggerDisplay("[{left}, {glyph_width}, {char_width}]")]
        public struct CharWidthInfo
        {
            public sbyte left;
            public byte glyph_width;
            public byte char_width;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FFNT
        {
            public Magic magic;
            public ushort endianness;
            public short header_size;
            public int version;
            public int file_size;
            public int num_blocks;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FINF
        {
            public Magic magic;
            public int section_size;
            public byte font_type;
            public byte height;
            public byte width;
            public byte ascent;
            public short line_feed;
            public short alter_char_index;
            public CharWidthInfo default_width;
            public byte encoding;
            public int tglp_offset;
            public int cwdh_offset;
            public int cmap_offset;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGLP
        {
            public Magic magic;
            public int section_size;
            public byte cell_width;
            public byte cell_height;
            public byte num_sheets;
            public byte max_character_width;
            public int sheet_size;
            public short baseline_position;
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

        public List<Bitmap> bmps;
        public List<ImageSettings> _settings = new List<ImageSettings>();

        bool _usesGlgr;
        byte[] _glgr;

        FINF finf;
        TGLP tglp;

        List<CharWidthInfo> lstCWDH = new List<CharWidthInfo>();
        Dictionary<char, int> dicCMAP = new Dictionary<char, int>();

        public BFFNT(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                // @todo: read as sections
                br.ReadStruct<FFNT>();

                if (br.ReadString(4) == "GLGR")
                {
                    _usesGlgr = true;
                    var glgrSize = br.ReadInt32();
                    br.BaseStream.Position -= 8;
                    _glgr = br.ReadBytes(glgrSize);
                }
                else
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
                if (_usesGlgr)
                {
                    bmps = new List<Bitmap>();
                    for (int i = 0; i < tglp.num_sheets; i++)
                    {
                        var compSize = br.ReadInt32();
                        var decomp = Huffman.Decompress(new MemoryStream(br.ReadBytes(compSize)), 8, 0);

                        var settings = new ImageSettings
                        {
                            Width = width,
                            Height = height,
                            Format = CTRFormat[(byte)(tglp.sheet_image_format & 0x7fff)],
                            Swizzle = new CTRSwizzle(width, height)
                        };

                        _settings.Add(settings);
                        bmps.Add(Kontract.Image.Common.Load(decomp, settings));
                    }
                }
                else
                {
                    bmps = new List<Bitmap>();
                    for (int i = 0; i < tglp.num_sheets; i++)
                    {
                        var settings = new ImageSettings
                        {
                            Width = width,
                            Height = height,
                            Format = CTRFormat[(byte)(tglp.sheet_image_format & 0x7fff)],
                            Swizzle = new CTRSwizzle(width, height)
                        };

                        _settings.Add(settings);
                        bmps.Add(Kontract.Image.Common.Load(br.ReadBytes(tglp.sheet_size), settings));
                    }
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
