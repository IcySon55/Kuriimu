using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Komponent.Image.Format;
using Komponent.Image.Swizzle;
using Komponent.Image;
using Komponent.IO;

namespace image_nintendo.BCFNX
{
    public class BCFNT
    {
        public class Import
        {
            [Import("Huff8")]
            public ICompression huff8b;

            public Import()
            {
                var catalog = new DirectoryCatalog("Komponents");
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
        }

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

        Import imports = new Import();
        FINF finf;
        TGLP tglp;
        public Bitmap[] bmps;
        ImageAttributes attr = new ImageAttributes();
        List<CharWidthInfo> lstCWDH = new List<CharWidthInfo>();
        Dictionary<char, int> dicCMAP = new Dictionary<char, int>();
        byte[] _glgr;
        bool _usesGlgr;

        public byte[] CheckTGLP(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                if (_usesGlgr)
                {
                    var compSize = br.ReadInt32();
                    var t = imports.huff8b.Decompress(new MemoryStream(br.ReadBytes(compSize)), 0);
                    return t;
                }
                return br.ReadBytes(tglp.sheet_size);
            }
        }

        public List<ImageSettings> _settings = new List<ImageSettings>();

        public BCFNT(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                // @todo: read as sections
                br.ReadStruct<CFNT>();

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
                    bmps = new Bitmap[tglp.num_sheets];
                    for (int i = 0; i < tglp.num_sheets; i++)
                    {
                        var compSize = br.ReadInt32();
                        var decomp = imports.huff8b.Decompress(new MemoryStream(br.ReadBytes(compSize)), 0);

                        var settings = new ImageSettings
                        {
                            Width = width,
                            Height = height,
                            Format = CTRFormat[(byte)(tglp.sheet_image_format & 0x7fff)],
                            Swizzle = new CTRSwizzle(width, height)
                        };
                        _settings.Add(settings);
                        bmps[i] = Common.Load(decomp, settings);
                    }
                }
                else
                {
                    bmps = new Bitmap[tglp.num_sheets];
                    for (int i = 0; i < tglp.num_sheets; i++)
                    {
                        var tex = br.ReadBytes(tglp.sheet_size);

                        var settings = new ImageSettings
                        {
                            Width = width,
                            Height = height,
                            Format = CTRFormat[(byte)(tglp.sheet_image_format & 0x7fff)],
                            Swizzle = new CTRSwizzle(width, height)
                        };
                        _settings.Add(settings);
                        bmps[i] = Common.Load(tex, settings);
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
