using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Cetera.IO;
using Kuriimu.IO;
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
            public CLIMFormat format;
            public Orientation orientation;
            public short alignment;
            public int datasize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BFLIMImageHeaderLE
        {
            public short width;
            public short height;
            public short alignment;
            public FLIMFormat format;
            public Orientation orientation;
            public int datasize;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BFLIMImageHeaderBE
        {
            public short width;
            public short height;
            public short alignment;
            public FLIMFormat format;
            private byte tmp;
            public int datasize;

            public Orientation orientation => (Orientation)(tmp >> 5);
            public int tileMode => tmp & 0x1F;
        }

        public enum CLIMFormat : byte
        {
            L8, A8, LA44, LA88, HL88,
            RGB565, RGB888, RGBA5551,
            RGBA4444, RGBA8888,
            ETC1, ETC1A4, L4, A4
        }
        public enum FLIMFormat : byte
        {
            L8, A8, LA44, LA88, HL88,
            RGB565, RGB888, RGBA5551,
            RGBA4444, RGBA8888,
            ETC1, ETC1A4,
            DXT1, DXT3, DXT5,
            L4 = 0x12, A4
        }

        public enum Orientation : byte
        {
            Default = 0,
            Rotate90 = 4,
            Transpose = 8,
        }

        NW4CSectionList sections;
        private ByteOrder byteOrder { get; set; }
        public BCLIMImageHeader BCLIMHeader { get; private set; }
        public BFLIMImageHeaderLE BFLIMHeaderLE { get; private set; }
        public BFLIMImageHeaderBE BFLIMHeaderBE { get; private set; }
        public Bitmap Image { get; set; }
        public ImageSettings Settings { get; set; }

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
                        Settings = new ImageSettings
                        {
                            Width = BCLIMHeader.width,
                            Height = BCLIMHeader.height,
                            Format = ImageSettings.ConvertFormat(BCLIMHeader.format),
                            Orientation = ImageSettings.ConvertOrientation(BCLIMHeader.orientation)
                        };
                        Image = Common.Load(tex, Settings);
                        break;
                    case "FLIM":
                        if (byteOrder == ByteOrder.LittleEndian)
                        {
                            BFLIMHeaderLE = sections[0].Data.BytesToStruct<BFLIMImageHeaderLE>(byteOrder);
                            Settings = new ImageSettings
                            {
                                Width = BFLIMHeaderLE.width,
                                Height = BFLIMHeaderLE.height,
                                Format = ImageSettings.ConvertFormat(BFLIMHeaderLE.format),
                                Orientation = ImageSettings.ConvertOrientation(BFLIMHeaderLE.orientation),
                            };
                            Image = Common.Load(tex, Settings);
                        }
                        else
                        {
                            BFLIMHeaderBE = sections[0].Data.BytesToStruct<BFLIMImageHeaderBE>(byteOrder);
                            var padWidth = 2 << (int)Math.Log(BFLIMHeaderBE.width - 1, 2);
                            var padHeight = 2 << (int)Math.Log(BFLIMHeaderBE.height - 1, 2);
                            Settings = new ImageSettings
                            {
                                Width = padWidth,
                                Height = padHeight,
                                Format = ImageSettings.ConvertFormat(BFLIMHeaderBE.format),
                                Orientation = Cetera.Image.Orientation.Default,
                                PadToPowerOf2 = true,
                                ZOrder = (br.ByteOrder == ByteOrder.LittleEndian) ? true : false
                            };
                            Image = SwizzleTiles(Common.Load(tex, Settings), padWidth, padHeight, BFLIMHeaderBE.width, BFLIMHeaderBE.height, 8, BFLIMHeaderBE.tileMode);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Unknown image format {sections.Header.magic}");
                }
            }
        }

        private Bitmap SwizzleTiles(Bitmap tex, int padWidth, int padHeight, int origWidth, int origHeight, int tileSize, int tileMode)
        {
            var newImage = new Bitmap(padWidth, padHeight);

            var oldG = Graphics.FromImage(tex);
            var newG = Graphics.FromImage(newImage);

            switch (tileMode)
            {
                case 4:
                    var swizzleY = new int[] { tileSize, 0, tileSize, 0 };
                    var swizzleX = new int[] { 0, 0, tileSize, tileSize };
                    var xValues = new int[] { 0, 0, padWidth, padWidth };
                    var xValuesPos = 0;

                    var newPosX = 0;
                    var newPosY = 0;

                    for (int y = 0; y < padHeight; y += 2 * tileSize)
                    {
                        if (xValues[xValuesPos] == 0)
                            for (int x = 0; x < padWidth / 2; x += 2 * tileSize)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    var tmpTile = new Bitmap(tileSize, tileSize);
                                    Graphics.FromImage(tmpTile).DrawImage(tex, 0, 0, new Rectangle(new Point(x + swizzleX[i], y + swizzleY[i]), new Size(tileSize, tileSize)), GraphicsUnit.Pixel);

                                    newG.DrawImage(tmpTile, new Point(newPosX, newPosY));
                                    newPosX += tileSize;
                                    if (newPosX >= padWidth)
                                    {
                                        newPosX = 0;
                                        newPosY += tileSize;
                                    }

                                    tmpTile = new Bitmap(tileSize, tileSize);
                                    Graphics.FromImage(tmpTile).DrawImage(tex, 0, 0, new Rectangle(new Point(padWidth - ((x + 2 * tileSize)) + swizzleX[i], y + swizzleY[i]), new Size(tileSize, tileSize)), GraphicsUnit.Pixel);
                                    newG.DrawImage(tmpTile, new Point(newPosX, newPosY));
                                    newPosX += tileSize;
                                    if (newPosX >= padWidth)
                                    {
                                        newPosX = 0;
                                        newPosY += tileSize;
                                    }
                                }

                                newImage.Save("C:\\Users\\Kirito\\Desktop\\test.bmp");
                            }
                        else
                            for (int x = padWidth / 2 - 2 * tileSize; x >= 0; x -= 2 * tileSize)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    var tmpTile = new Bitmap(tileSize, tileSize);
                                    Graphics.FromImage(tmpTile).DrawImage(tex, 0, 0, new Rectangle(new Point(padWidth - ((x + 2 * tileSize)) + swizzleX[i], y + swizzleY[i]), new Size(tileSize, tileSize)), GraphicsUnit.Pixel);
                                    newG.DrawImage(tmpTile, new Point(newPosX, newPosY));
                                    newPosX += tileSize;
                                    if (newPosX >= padWidth)
                                    {
                                        newPosX = 0;
                                        newPosY += tileSize;
                                    }

                                    tmpTile = new Bitmap(tileSize, tileSize);
                                    Graphics.FromImage(tmpTile).DrawImage(tex, 0, 0, new Rectangle(new Point(x + swizzleX[i], y + swizzleY[i]), new Size(tileSize, tileSize)), GraphicsUnit.Pixel);

                                    newG.DrawImage(tmpTile, new Point(newPosX, newPosY));
                                    newPosX += tileSize;
                                    if (newPosX >= padWidth)
                                    {
                                        newPosX = 0;
                                        newPosY += tileSize;
                                    }
                                }

                                newImage.Save("C:\\Users\\Kirito\\Desktop\\test.bmp");
                            }

                        xValuesPos++;
                        swizzleY = swizzleY.Reverse().ToArray();
                    }

                    if (origWidth != padWidth || origHeight != padHeight)
                    {
                        var cropImage = new Bitmap(origWidth, origHeight);
                        Graphics.FromImage(cropImage).DrawImage(newImage, 0, 0, new Rectangle(new Point(0, 0), new Size(origWidth, origHeight)), GraphicsUnit.Pixel);
                        return cropImage;
                    }

                    return newImage;
                default:
                    return tex;
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var settings = new ImageSettings();
                byte[] texture;

                switch (sections.Header.magic)
                {
                    case "CLIM":
                        settings.Width = BCLIMHeader.width;
                        settings.Height = BCLIMHeader.height;
                        settings.Orientation = ImageSettings.ConvertOrientation(BCLIMHeader.orientation);
                        settings.Format = ImageSettings.ConvertFormat(BCLIMHeader.format);
                        texture = Common.Save(Image, settings);
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
                            settings.Width = BFLIMHeaderLE.width;
                            settings.Height = BFLIMHeaderLE.height;
                            settings.Orientation = ImageSettings.ConvertOrientation(BFLIMHeaderLE.orientation);
                            settings.Format = ImageSettings.ConvertFormat(BFLIMHeaderLE.format);
                            texture = Common.Save(Image, settings);
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
                            throw new NotSupportedException($"Big Endian FLIM isn't savable yet!");
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Unknown image format {sections.Header.magic}");
                }
            }
        }
    }
}
