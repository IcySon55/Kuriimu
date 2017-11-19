using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using Komponent.Image.Format;
using Kontract.Interface;
using Komponent.Image.Swizzle;
using Komponent.IO;
using Komponent.Image;
using System.Linq;

namespace image_nintendo.BXLIM
{
    public sealed class BXLIM
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BCLIMImageHeader
        {
            public short width;
            public short height;
            public byte format;
            public byte swizzleTileMode; // not used in BCLIM
            public short alignment;
            public int datasize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BFLIMImageHeader
        {
            public short width;
            public short height;
            public short alignment;
            public byte format;
            public byte swizzleTileMode;
            public int datasize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class NW4CHeader
        {
            public Magic magic;
            public ByteOrder byte_order;
            public short header_size;
            public int version;
            public int file_size;
            public short section_count;
            public short padding;
        };

        [DebuggerDisplay("{Magic,nq}: {Data.Length} bytes")]
        public class NW4CSection
        {
            public Magic Magic { get; }
            public byte[] Data { get; set; }
            public object Object { get; set; }

            public NW4CSection(string magic, byte[] data)
            {
                Magic = magic;
                Data = data;
            }
        }

        public class NW4CSectionList : List<NW4CSection>
        {
            public NW4CHeader Header { get; set; }
        }

        NW4CSectionList ReadSections(BinaryReaderX br)
        {
            br.BaseStream.Position += 4;
            br.ByteOrder = (ByteOrder)br.ReadUInt16();
            br.BaseStream.Position -= 6;
            var lst = new NW4CSectionList { Header = br.ReadStruct<NW4CHeader>() };
            lst.AddRange(from _ in Enumerable.Range(0, lst.Header.section_count)
                         let magic1 = br.ReadStruct<Magic>()
                         let data = br.ReadBytes(br.ReadInt32())
                         select new NW4CSection(magic1, data));
            return lst;
        }

        public static void WriteSections(BinaryWriterX bw, NW4CSectionList sections)
        {
            bw.WriteStruct(sections.Header);
            foreach (var sec in sections)
            {
                bw.WriteStruct(sec.Magic);
                bw.Write(sec.Data.Length + 4);
                bw.Write(sec.Data);
            }
        }

        public Dictionary<byte, IImageFormat> DSFormat = new Dictionary<byte, IImageFormat>
        {
            [0] = new LA(8, 0),
            [1] = new LA(0, 8),
            [2] = new LA(4, 4),
            [3] = new LA(8, 8),
            [4] = new HL(8, 8),
            [5] = new RGBA(5, 6, 5),
            [6] = new RGBA(8, 8, 8),
            [7] = new RGBA(5, 5, 5, 1),
            [8] = new RGBA(4, 4, 4, 4),
            [9] = new RGBA(8, 8, 8, 8),
            [10] = new ETC1(),
            [11] = new ETC1(true),
            [18] = new LA(4, 0),
            [19] = new LA(0, 4),
        };

        public Dictionary<byte, IImageFormat> WiiUFormat = new Dictionary<byte, IImageFormat>
        {
            [0] = new LA(8, 0, ByteOrder.BigEndian),
            [1] = new LA(0, 8, ByteOrder.BigEndian),
            [2] = new LA(4, 4, ByteOrder.BigEndian),
            [3] = new LA(8, 8, ByteOrder.BigEndian),
            [4] = new HL(8, 8, ByteOrder.BigEndian),
            [5] = new RGBA(5, 6, 5, 0, ByteOrder.BigEndian),
            [6] = new RGBA(8, 8, 8, 0, ByteOrder.BigEndian),
            [7] = new RGBA(5, 5, 5, 1, ByteOrder.BigEndian),
            [8] = new RGBA(4, 4, 4, 4, ByteOrder.BigEndian),
            [9] = new RGBA(8, 8, 8, 8, ByteOrder.BigEndian),
            [10] = new ETC1(false),
            [11] = new ETC1(true),
            [12] = new DXT(DXT.Version.DXT1),
            [13] = new DXT(DXT.Version.DXT3),
            [14] = new DXT(DXT.Version.DXT5),
            [15] = new ATI(ATI.Format.ATI1L),
            [16] = new ATI(ATI.Format.ATI1A),
            [17] = new ATI(ATI.Format.ATI2),
            [18] = new LA(4, 0, ByteOrder.BigEndian),
            [19] = new LA(0, 4, ByteOrder.BigEndian),
            [20] = new RGBA(8, 8, 8, 8, ByteOrder.BigEndian),
            [21] = new DXT(DXT.Version.DXT1),
            [22] = new DXT(DXT.Version.DXT3),
            [23] = new DXT(DXT.Version.DXT5),
            [24] = new RGBA(10, 10, 10, 2, ByteOrder.BigEndian)
        };

        NW4CSectionList sections;

        private ByteOrder byteOrder { get; set; }

        public BCLIMImageHeader BCLIMHeader { get; private set; }
        public BFLIMImageHeader BFLIMHeader { get; private set; }

        public Bitmap Image { get; set; }

        public ImageSettings Settings { get; set; }

        public BXLIM(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                var tex = br.ReadBytes((int)br.BaseStream.Length - 40);
                sections = ReadSections(br);
                byteOrder = br.ByteOrder;

                switch (sections.Header.magic)
                {
                    case "CLIM":
                        BCLIMHeader = sections[0].Data.BytesToStruct<BCLIMImageHeader>(byteOrder);

                        Settings = new ImageSettings
                        {
                            Width = BCLIMHeader.width,
                            Height = BCLIMHeader.height,
                            Format = DSFormat[BCLIMHeader.format],
                            Swizzle = new CTRSwizzle(BCLIMHeader.width, BCLIMHeader.height, BCLIMHeader.swizzleTileMode)
                        };
                        Image = Common.Load(tex, Settings);
                        break;
                    case "FLIM":
                        if (byteOrder == ByteOrder.LittleEndian)
                        {
                            BFLIMHeader = sections[0].Data.BytesToStruct<BFLIMImageHeader>(byteOrder);

                            Settings = new ImageSettings
                            {
                                Width = BFLIMHeader.width,
                                Height = BFLIMHeader.height,
                                Format = DSFormat[BFLIMHeader.format],
                                Swizzle = new CTRSwizzle(BFLIMHeader.width, BFLIMHeader.height, BFLIMHeader.swizzleTileMode)
                            };
                            Image = Common.Load(tex, Settings);
                        }
                        else
                        {
                            BFLIMHeader = sections[0].Data.BytesToStruct<BFLIMImageHeader>(byteOrder);

                            var format = WiiUFormat[BFLIMHeader.format];
                            var isBlockBased = new[] { 10, 11, 12, 13, 14, 15, 16, 17, 21, 22, 23 }.Contains(BFLIMHeader.format); // hax

                            Settings = new ImageSettings
                            {
                                Width = BFLIMHeader.width,
                                Height = BFLIMHeader.height,
                                Format = format,
                                Swizzle = new WiiUSwizzle(BFLIMHeader.width, BFLIMHeader.height, BFLIMHeader.swizzleTileMode, isBlockBased, format.BitDepth)
                            };

                            Image = Common.Load(tex, Settings);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Unknown image format {sections.Header.magic}");
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, byteOrder))
            {
                byte[] texture;

                switch (sections.Header.magic)
                {
                    case "CLIM":
                        Settings.Width = Image.Width;
                        Settings.Height = Image.Height;
                        Settings.Swizzle = new CTRSwizzle(Image.Width, Image.Height, BCLIMHeader.swizzleTileMode);

                        texture = Common.Save(Image, Settings);
                        bw.Write(texture);

                        // We can now change the image width/height/filesize!
                        BCLIMHeader.width = (short)Image.Width;
                        BCLIMHeader.height = (short)Image.Height;
                        BCLIMHeader.datasize = texture.Length;

                        sections[0].Data = BCLIMHeader.StructToBytes(byteOrder);
                        sections.Header.file_size = texture.Length + 40;

                        WriteSections(bw, sections);
                        break;
                    case "FLIM":
                        Settings.Width = Image.Width;
                        Settings.Height = Image.Height;
                        if (byteOrder == ByteOrder.LittleEndian)
                        {
                            Settings.Swizzle = new CTRSwizzle(Image.Width, Image.Height, BFLIMHeader.swizzleTileMode);
                        }
                        else
                        {
                            var isBlockBased = new[] { 10, 11, 12, 13, 14, 15, 16, 17, 21, 22, 23 }.Contains(BFLIMHeader.format); // hax
                            Settings.Swizzle = new WiiUSwizzle(Image.Width, Image.Height, BFLIMHeader.swizzleTileMode, isBlockBased, Settings.Format.BitDepth);
                        }

                        texture = Common.Save(Image, Settings);
                        bw.Write(texture);

                        // We can now change the image width/height/filesize!
                        BFLIMHeader.width = (short)Image.Width;
                        BFLIMHeader.height = (short)Image.Height;
                        BFLIMHeader.datasize = texture.Length;

                        sections[0].Data = BFLIMHeader.StructToBytes(byteOrder);
                        sections.Header.file_size = texture.Length + 40;

                        WriteSections(bw, sections);
                        break;
                    default:
                        throw new NotSupportedException($"Unknown image format {sections.Header.magic}");
                }
            }
        }
    }
}
