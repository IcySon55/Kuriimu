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
            [10] = new ETC1(false, ByteOrder.BigEndian),
            [11] = new ETC1(true, ByteOrder.BigEndian),
            [12] = new DXT(DXT.Version.DXT1, false, ByteOrder.BigEndian),
            [13] = new DXT(DXT.Version.DXT3, false, ByteOrder.BigEndian),
            [14] = new DXT(DXT.Version.DXT5, false, ByteOrder.BigEndian),
            [15] = new ATI(ATI.Format.ATI1L),
            [16] = new ATI(ATI.Format.ATI1A),
            [17] = new ATI(ATI.Format.ATI2),
            [18] = new LA(4, 0, ByteOrder.BigEndian),
            [19] = new LA(0, 4, ByteOrder.BigEndian),
            [20] = new RGBA(8, 8, 8, 8, ByteOrder.BigEndian),
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

                        Settings = new Kontract.Image.ImageSettings
                        {
                            Width = BCLIMHeader.width,
                            Height = BCLIMHeader.height,
                            Format = DSFormat[BCLIMHeader.format],
                            Swizzle = new CTR((BCLIMHeader.width + 7) & ~7, (BCLIMHeader.height + 7) & ~7, BCLIMHeader.orientation)
                        };
                        Image = Kontract.Image.Image.Load(tex, Settings);
                        break;
                    case "FLIM":
                        if (byteOrder == ByteOrder.LittleEndian)
                        {
                            BFLIMHeaderLE = sections[0].Data.BytesToStruct<BFLIMImageHeaderLE>(byteOrder);

                            Settings = new Kontract.Image.ImageSettings
                            {
                                Width = BFLIMHeaderLE.width,
                                Height = BFLIMHeaderLE.height,
                                Format = DSFormat[BFLIMHeaderLE.format],
                                Swizzle = new CTR((BFLIMHeaderLE.width + 7) & ~7, (BFLIMHeaderLE.height + 7) & ~7, BFLIMHeaderLE.orientation)
                            };
                            Image = Kontract.Image.Image.Load(tex, Settings);
                        }
                        else
                        {
                            BFLIMHeaderBE = sections[0].Data.BytesToStruct<BFLIMImageHeaderBE>(byteOrder);

                            GetPaddedDimensions(BFLIMHeaderBE.width, BFLIMHeaderBE.height, BFLIMHeaderBE.format, out var padWidth, out var padHeight);

                            Settings = new Kontract.Image.ImageSettings
                            {
                                Width = BFLIMHeaderBE.width,
                                Height = BFLIMHeaderBE.height,
                                Format = WiiUFormat[BFLIMHeaderBE.format],
                            };
                            Image = Kontract.Image.Image.Load(tex, Settings);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Unknown image format {sections.Header.magic}");
                }
            }
        }

        void GetPaddedDimensions(int width, int height, byte format, out int padWidth, out int padHeight)
        {
            padWidth = 0;
            padHeight = 0;

            if (format >= 10 && format <= 17)
            {
                padWidth = ((width / 4 + 32) & ~31) * 4;
                padHeight = (height / 32 + 1) * 32;
            }
            else
            {
                padWidth = (width / 32 + 1) * 32;
                padHeight = (height / 32 + 1) * 32;
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
