using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kontract.Image.Format;
using Kontract.Interface;
using Kontract.Image.Swizzle;
using Cetera.IO;
using Kontract.IO;
using Cetera;
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
            [10] = new ETC1(false, false, ByteOrder.BigEndian),
            [11] = new ETC1(true, false, ByteOrder.BigEndian),
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

        public Kontract.Image.ImageSettings Settings { get; set; }

        public BXLIM(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                var tex = br.ReadBytes((int)br.BaseStream.Length - 40);
                sections = br.ReadSections();
                byteOrder = br.ByteOrder;

                switch (sections.Header.magic)
                {
                    case "CLIM":
                        BCLIMHeader = sections[0].Data.BytesToStruct<BCLIMImageHeader>(byteOrder);

                        Settings = new Kontract.Image.ImageSettings
                        {
                            Width = BCLIMHeader.width,
                            Height = BCLIMHeader.height,
                            Format = DSFormat[BCLIMHeader.format],
                            Swizzle = new CTRSwizzle(BCLIMHeader.width, BCLIMHeader.height, BCLIMHeader.swizzleTileMode)
                        };
                        Image = Kontract.Image.Common.Load(tex, Settings);
                        break;
                    case "FLIM":
                        if (byteOrder == ByteOrder.LittleEndian)
                        {
                            BFLIMHeader = sections[0].Data.BytesToStruct<BFLIMImageHeader>(byteOrder);

                            Settings = new Kontract.Image.ImageSettings
                            {
                                Width = BFLIMHeader.width,
                                Height = BFLIMHeader.height,
                                Format = DSFormat[BFLIMHeader.format],
                                Swizzle = new CTRSwizzle(BFLIMHeader.width, BFLIMHeader.height, BFLIMHeader.swizzleTileMode)
                            };
                            Image = Kontract.Image.Common.Load(tex, Settings);
                        }
                        else
                        {
                            BFLIMHeader = sections[0].Data.BytesToStruct<BFLIMImageHeader>(byteOrder);

                            var format = WiiUFormat[BFLIMHeader.format];
                            var isBlockBased = new[] { 10, 11, 12, 13, 14, 15, 16, 17, 21, 22, 23 }.Contains(BFLIMHeader.format); // hax

                            Settings = new Kontract.Image.ImageSettings
                            {
                                Width = BFLIMHeader.width,
                                Height = BFLIMHeader.height,
                                Format = format,
                                Swizzle = new WiiUSwizzle(BFLIMHeader.swizzleTileMode, isBlockBased, format.BitDepth, BFLIMHeader.width, BFLIMHeader.height)
                            };

                            // Uncomment the following line to use the padded width/height instead 
                            //(Settings.Width, Settings.Height) = (Settings.Swizzle.Width, Settings.Swizzle.Height);

                            Image = Kontract.Image.Common.Load(tex, Settings);
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

                        texture = Kontract.Image.Common.Save(Image, Settings);
                        bw.Write(texture);

                        // We can now change the image width/height/filesize!
                        BCLIMHeader.width = (short)Image.Width;
                        BCLIMHeader.height = (short)Image.Height;
                        BCLIMHeader.datasize = texture.Length;

                        sections[0].Data = BCLIMHeader.StructToBytes(byteOrder);
                        sections.Header.file_size = texture.Length + 40;

                        bw.WriteSections(sections);
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
                            Settings.Swizzle = new WiiUSwizzle(BFLIMHeader.swizzleTileMode, isBlockBased, Settings.Format.BitDepth, Image.Width, Image.Height);
                        }

                        texture = Kontract.Image.Common.Save(Image, Settings);
                        bw.Write(texture);

                        // We can now change the image width/height/filesize!
                        BFLIMHeader.width = (short)Image.Width;
                        BFLIMHeader.height = (short)Image.Height;
                        BFLIMHeader.datasize = texture.Length;

                        sections[0].Data = BFLIMHeader.StructToBytes(byteOrder);
                        sections.Header.file_size = texture.Length + 40;

                        bw.WriteSections(sections);
                        break;
                    default:
                        throw new NotSupportedException($"Unknown image format {sections.Header.magic}");
                }
            }
        }
    }
}
