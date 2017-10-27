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
using System.Linq;

namespace Cetera.Image
{
    public sealed class BXLIM
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BCLIMImageHeader
        {
            public short width;
            public short height;
            public byte format;
            public byte orientation;
            public short alignment;
            public int datasize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BFLIMImageHeaderLE
        {
            public short width;
            public short height;
            public short alignment;
            public byte format;
            public byte orientation;
            public int datasize;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BFLIMImageHeaderBE
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
            [10] = new Kontract.Image.Format.ETC1(),
            [11] = new Kontract.Image.Format.ETC1(true),
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
            [10] = new Kontract.Image.Format.ETC1(false, ByteOrder.BigEndian),
            [11] = new Kontract.Image.Format.ETC1(true, ByteOrder.BigEndian),
            [12] = new Kontract.Image.Format.DXT(Kontract.Image.Format.DXT.Version.DXT1, false, ByteOrder.BigEndian),
            [13] = new Kontract.Image.Format.DXT(Kontract.Image.Format.DXT.Version.DXT3, false, ByteOrder.BigEndian),
            [14] = new Kontract.Image.Format.DXT(Kontract.Image.Format.DXT.Version.DXT5, false, ByteOrder.BigEndian),
            [15] = new Kontract.Image.Format.ATI(Kontract.Image.Format.ATI.Format.ATI1L, ByteOrder.BigEndian),
            [16] = new Kontract.Image.Format.ATI(Kontract.Image.Format.ATI.Format.ATI1A, ByteOrder.BigEndian),
            [17] = new Kontract.Image.Format.ATI(Kontract.Image.Format.ATI.Format.ATI2, ByteOrder.BigEndian),
            [18] = new LA(4, 0, ByteOrder.BigEndian),
            [19] = new LA(0, 4, ByteOrder.BigEndian),
            [20] = null,
            [21] = null,
            [22] = null,
            [23] = null,
            [24] = new RGBA(10, 10, 10, 2, ByteOrder.BigEndian)
        };

        NW4CSectionList sections;

        private ByteOrder byteOrder { get; set; }

        public BCLIMImageHeader BCLIMHeader { get; private set; }
        public BFLIMImageHeaderLE BFLIMHeaderLE { get; private set; }
        public BFLIMImageHeaderBE BFLIMHeaderBE { get; private set; }

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

                        CreateSwizzleLists3DS(BCLIMHeader.orientation, byteOrder, out var innerS, out var outerS);

                        Settings = new Kontract.Image.ImageSettings
                        {
                            Width = BCLIMHeader.width,
                            Height = BCLIMHeader.height,
                            Format = DSFormat[BCLIMHeader.format],
                            InnerSwizzle = innerS,
                            OuterSwizzle = outerS,
                        };
                        Image = Kontract.Image.Image.Load(tex, Settings);
                        break;
                    case "FLIM":
                        if (byteOrder == ByteOrder.LittleEndian)
                        {
                            BFLIMHeaderLE = sections[0].Data.BytesToStruct<BFLIMImageHeaderLE>(byteOrder);

                            CreateSwizzleLists3DS(BFLIMHeaderLE.orientation, byteOrder, out innerS, out outerS);

                            Settings = new Kontract.Image.ImageSettings
                            {
                                Width = BFLIMHeaderLE.width,
                                Height = BFLIMHeaderLE.height,
                                Format = DSFormat[BFLIMHeaderLE.format],
                                InnerSwizzle = innerS,
                                OuterSwizzle = outerS,
                            };
                            Image = Kontract.Image.Image.Load(tex, Settings);
                        }
                        else
                        {
                            BFLIMHeaderBE = sections[0].Data.BytesToStruct<BFLIMImageHeaderBE>(byteOrder);

                            Settings = new Kontract.Image.ImageSettings
                            {
                                Width = BFLIMHeaderBE.width,
                                Height = BFLIMHeaderBE.height,
                                padWidth = (BFLIMHeaderBE.width % 32 != 0) ? (BFLIMHeaderBE.width / 32 + 1) * 32 : 0,
                                padHeight = (BFLIMHeaderBE.height % 32 != 0) ? (BFLIMHeaderBE.height / 32 + 1) * 32 : 0,
                                Format = WiiUFormat[BFLIMHeaderBE.format],
                                OuterSwizzle = new List<IImageSwizzle> { new Kontract.Image.Swizzle.WiiU(BFLIMHeaderBE.swizzleTileMode) }
                            };
                            Image = Kontract.Image.Image.Load(tex, Settings);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Unknown image format {sections.Header.magic}");
                }
            }
        }

        void CreateSwizzleLists3DS(byte orient, ByteOrder byteOrder, out List<IImageSwizzle> inner, out List<IImageSwizzle> outer)
        {
            inner = new List<IImageSwizzle>();
            outer = new List<IImageSwizzle>();

            if (byteOrder == ByteOrder.LittleEndian)
                inner = new List<IImageSwizzle> { new ZOrder() };

            if (orient != 0)
            {
                byte count = 8;
                while (count != 0)
                {
                    switch (orient & (int)Math.Pow(2, --count))
                    {
                        case 0x80:
                            break;
                        case 0x40:
                            break;
                        case 0x20:
                            break;
                        case 0x10:
                            break;
                        //Transpose
                        case 0x8:
                            inner.Add(new Transpose());
                            outer.Add(new Transpose());
                            break;
                        //Rotated by 90
                        case 0x4:
                            inner.Add(new Rotate(270));
                            outer.Add(new Rotate(270));
                            break;
                        case 0x2:
                            break;
                        case 0x1:
                            break;
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, byteOrder))
            {
                //var settings = new ImageSettings();
                byte[] texture;

                switch (sections.Header.magic)
                {
                    case "CLIM":
                        /*settings.Width = BCLIMHeader.width;
                        settings.Height = BCLIMHeader.height;*/
                        texture = Kontract.Image.Image.Save(Image, Settings);
                        bw.Write(texture);

                        // We can now change the image width/height/filesize!
                        BCLIMHeader.width = (short)Image.Width;
                        BCLIMHeader.height = (short)Image.Height;
                        BCLIMHeader.datasize = texture.Length;
                        sections[0].Data = BCLIMHeader.StructToBytes();
                        sections.Header.file_size = texture.Length + 40;
                        bw.WriteSections(sections);
                        break;
                    case "FLIM":
                        if (byteOrder == ByteOrder.LittleEndian)
                        {
                            /*settings.Width = BFLIMHeaderLE.width;
                            settings.Height = BFLIMHeaderLE.height;*/
                            texture = Kontract.Image.Image.Save(Image, Settings);
                            bw.Write(texture);

                            // We can now change the image width/height/filesize!
                            BFLIMHeaderLE.width = (short)Image.Width;
                            BFLIMHeaderLE.height = (short)Image.Height;
                            BFLIMHeaderLE.datasize = texture.Length;
                            sections[0].Data = BFLIMHeaderLE.StructToBytes();
                            sections.Header.file_size = texture.Length + 40;
                            bw.WriteSections(sections);
                        }
                        else
                        {
                            /*settings.Width = BFLIMHeaderLE.width;
                            settings.Height = BFLIMHeaderLE.height;*/
                            texture = Kontract.Image.Image.Save(Image, Settings);
                            bw.Write(texture);

                            // We can now change the image width/height/filesize!
                            BFLIMHeaderBE.width = (short)Image.Width;
                            BFLIMHeaderBE.height = (short)Image.Height;
                            BFLIMHeaderBE.datasize = texture.Length;
                            sections[0].Data = BFLIMHeaderBE.StructToBytes(byteOrder);
                            sections.Header.file_size = texture.Length + 40;
                            bw.WriteSections(sections);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Unknown image format {sections.Header.magic}");
                }
            }
        }
    }
}
