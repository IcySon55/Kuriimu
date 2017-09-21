using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Cetera.IO;
using Kuriimu.IO;

namespace Cetera.Image
{
    public sealed class BXLIM
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BCLIMImageHeader
        {
            public short width;
            public short height;
            public Format format;
            public Orientation orientation;
            public short alignment;
            public int datasize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BFLIMImageHeader
        {
            public short height;
            public short width;
            public short alignment;
            public Format format;
            public Orientation orientation;
            public int datasize;
        }

        public enum Format : byte
        {
            L8, A8, LA44, LA88, HL88,
            RGB565, RGB888, RGBA5551,
            RGBA4444, RGBA8888,
            ETC1, ETC1A4, L4, A4
        }

        public enum Orientation : byte
        {
            Default = 0,
            TransposeTile = 1,
            Rotate270 = 4,
            XFlip90 = 8,
        }

        NW4CSectionList sections;
        public BCLIMImageHeader BCLIMHeader { get; private set; }
        public BFLIMImageHeader BFLIMHeader { get; private set; }
        public Bitmap Image { get; set; }
        public ImageSettings Settings { get; set; }

        public BXLIM(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                var tex = br.ReadBytes((int)br.BaseStream.Length - 40);
                sections = br.ReadSections();
                switch (sections.Header.magic)
                {
                    case "CLIM":
                        BCLIMHeader = sections[0].Data.BytesToStruct<BCLIMImageHeader>(br.ByteOrder);
                        Settings = new ImageSettings
                        {
                            Width = 2 << (int)Math.Log(BCLIMHeader.width - 1, 2),
                            Height = 2 << (int)Math.Log(BCLIMHeader.height - 1, 2),
                            Format = ImageSettings.ConvertFormat(BCLIMHeader.format),
                            Orientation = ImageSettings.ConvertOrientation(BCLIMHeader.orientation)
                        };
                        break;
                    case "FLIM":
                        BFLIMHeader = sections[0].Data.BytesToStruct<BFLIMImageHeader>(br.ByteOrder);
                        Settings = new ImageSettings
                        {
                            Width = 2 << (int)Math.Log(BFLIMHeader.width - 1, 2),
                            Height = 2 << (int)Math.Log(BFLIMHeader.height - 1, 2),
                            Format = ImageSettings.ConvertFormat(BFLIMHeader.format),
                            Orientation = ImageSettings.ConvertOrientation(BFLIMHeader.orientation),
                            PadToPowerOf2 = true
                        };
                        break;
                    default:
                        throw new NotSupportedException($"Unknown image format {sections.Header.magic}");
                }
                Image = Common.Load(tex, Settings);
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
                        settings.Width = 2 << (int)Math.Log(BCLIMHeader.width - 1, 2);
                        settings.Height = 2 << (int)Math.Log(BCLIMHeader.height - 1, 2);
                        settings.Orientation = ImageSettings.ConvertOrientation(BCLIMHeader.orientation);
                        settings.Format = ImageSettings.ConvertFormat(BCLIMHeader.format);
                        texture = Common.Save(Image, settings);
                        bw.Write(texture);

                        // We can now change the image width/height/filesize!
                        var modifiedBCLIMHeader = BCLIMHeader;
                        modifiedBCLIMHeader.width = BCLIMHeader.width;
                        modifiedBCLIMHeader.height = BCLIMHeader.height;
                        /*modifiedBCLIMHeader.width = (short)Image.Width;
                        modifiedBCLIMHeader.height = (short)Image.Height;*/
                        modifiedBCLIMHeader.datasize = texture.Length;
                        BCLIMHeader = modifiedBCLIMHeader;
                        sections[0].Data = BCLIMHeader.StructToBytes();
                        sections.Header.file_size = texture.Length + 40;
                        bw.WriteSections(sections);
                        break;
                    case "FLIM":
                        settings.Width = 2 << (int)Math.Log(BFLIMHeader.height - 1, 2);
                        settings.Height = 2 << (int)Math.Log(BFLIMHeader.width - 1, 2);
                        settings.Orientation = ImageSettings.ConvertOrientation(BFLIMHeader.orientation);
                        settings.Format = ImageSettings.ConvertFormat(BFLIMHeader.format);
                        texture = Common.Save(Image, settings);
                        bw.Write(texture);

                        // We can now change the image width/height/filesize!
                        var modifiedBFLIMHeader = BFLIMHeader;
                        modifiedBFLIMHeader.width = BFLIMHeader.width;
                        modifiedBFLIMHeader.height = BFLIMHeader.height;
                        /*modifiedBFLIMHeader.width = (short)Image.Width;
                        modifiedBFLIMHeader.height = (short)Image.Height;*/
                        modifiedBFLIMHeader.datasize = texture.Length;
                        BFLIMHeader = modifiedBFLIMHeader;
                        sections[0].Data = BFLIMHeader.StructToBytes();
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
