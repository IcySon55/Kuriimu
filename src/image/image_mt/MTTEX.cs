using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Cetera.Image;
using Kuriimu.IO;

namespace image_mt
{
    class MTTEX
    {
        public List<Bitmap> Bitmaps = new List<Bitmap>();
        private const int MinHeight = 8;

        private Header Header;
        public HeaderInfo HeaderInfo { get; set; }
        private int HeaderLength = 0x10;
        private ImageSettings Settings = new ImageSettings();
        private ByteOrder ByteOrder = ByteOrder.LittleEndian;

        public MTTEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                // Set endianess
                if (br.PeekString() == "\0XET")
                    br.ByteOrder = ByteOrder = ByteOrder.BigEndian;

                // Header
                Header = br.ReadStruct<Header>();
                HeaderInfo = new HeaderInfo
                {
                    // Block 1
                    Version = (Version)(Header.Block1 & 0xFFF),
                    Unknown1 = (int)((Header.Block1 >> 12) & 0xFFF),
                    Unused1 = (int)((Header.Block1 >> 24) & 0xF),
                    AlphaChannelFlags = (AlphaChannelFlags)((Header.Block1 >> 28) & 0xF),
                    // Block 2
                    MipMapCount = (int)(Header.Block2 & 0x3F),
                    Width = (int)((Header.Block2 >> 6) & 0x1FFF),
                    Height = Math.Max((int)((Header.Block2 >> 19) & 0x1FFF), MinHeight),
                    // Block 3
                    Unknown2 = (int)(Header.Block3 & 0xFF),
                    Format = (Format)((Header.Block3 >> 8) & 0xFF),
                    Unknown3 = (int)((Header.Block3 >> 16) & 0xFFFF)
                };

                // @todo: Consider whether the following settings make more sense if conditioned by the ByteOrder (or Platform)

                var format = HeaderInfo.Format.ToString().StartsWith("DXT5") ? Format.DXT5 : HeaderInfo.Format;
                Settings.Format = ImageSettings.ConvertFormat(format);
                if (Settings.Format.ToString().Contains("DXT"))
                {
                    Settings.PadToPowerOf2 = false;
                    Settings.ZOrder = false;
                    Settings.TileSize = 4;
                }

                var mipMaps = br.ReadMultiple<int>(HeaderInfo.MipMapCount);

                for (var i = 0; i < mipMaps.Count; i++)
                {
                    var texDataSize = (i + 1 < mipMaps.Count ? mipMaps[i + 1] : (int)br.BaseStream.Length) - mipMaps[i];
                    Settings.Width = HeaderInfo.Width >> i;
                    Settings.Height = HeaderInfo.Height >> i;

                    if (HeaderInfo.Format == Format.DXT5Other)
                        Settings.PixelShader = ToNoAlpha;
                    else if (HeaderInfo.Format == Format.DXT5YCbCr)
                        Settings.PixelShader = ToProperColors;

                    Bitmaps.Add(Common.Load(br.ReadBytes(texDataSize), Settings));
                }
            }
        }

        // Currently trying out YCbCr:
        // https://en.wikipedia.org/wiki/YCbCr#JPEG_conversion
        private static int Clamp(double n) => (int)Math.Max(0, Math.Min(n, 255));
        private const int CbCrThreshold = 123; // usually 128, but 123 seems to work better here

        private Color ToNoAlpha(Color c)
        {
            return Color.FromArgb(255, c.R, c.G, c.B);
        }

        private Color ToProperColors(Color c)
        {
            var (A, Y, Cb, Cr) = (c.G, c.A, c.B - CbCrThreshold, c.R - CbCrThreshold);
            return Color.FromArgb(A,
                Clamp(Y + 1.402 * Cr),
                Clamp(Y - 0.344136 * Cb - 0.714136 * Cr),
                Clamp(Y + 1.772 * Cb));
        }

        private Color ToOptimisedColors(Color c)
        {
            var (A, Y, Cb, Cr) = (c.A,
                0.299 * c.R + 0.587 * c.G + 0.114 * c.B,
                CbCrThreshold - 0.168736 * c.R - 0.331264 * c.G + 0.5 * c.B,
                CbCrThreshold + 0.5 * c.R - 0.418688 * c.G - 0.081312 * c.B);
            return Color.FromArgb(Clamp(Y), Clamp(Cr), c.A, Clamp(Cb));
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, ByteOrder))
            {
                Header.Block1 = (uint)((int)HeaderInfo.Version | (HeaderInfo.Unknown1 << 12) | (HeaderInfo.Unused1 << 24) | ((int)HeaderInfo.AlphaChannelFlags << 28));
                Header.Block2 = (uint)(HeaderInfo.MipMapCount | (HeaderInfo.Width << 6) | (HeaderInfo.Height << 19));
                Header.Block3 = (uint)(HeaderInfo.Unknown2 | ((int)HeaderInfo.Format << 8) | (HeaderInfo.Unknown3 << 16));
                bw.WriteStruct(Header);

                Settings.Format = ImageSettings.ConvertFormat(HeaderInfo.Format);

                // @todo: add other things like PadToPowerOf2, ZOrder and TileSize

                if (HeaderInfo.Format == Format.DXT5YCbCr)
                    Settings.PixelShader = ToOptimisedColors;

                var bitmaps = Bitmaps.Select(bmp => Common.Save(bmp, Settings)).ToList();

                // Mipmaps
                var offset = HeaderInfo.Version == Version.v154 ? HeaderInfo.MipMapCount * sizeof(int) + HeaderLength : 0;
                foreach (var bitmap in bitmaps)
                {
                    bw.Write(offset);
                    offset += bitmap.Length;
                }

                // Bitmaps
                foreach (var bitmap in bitmaps)
                    bw.Write(bitmap);
            }
        }
    }
}
