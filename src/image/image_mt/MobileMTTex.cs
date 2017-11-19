using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.IO;

namespace image_mt.Mobile
{
    public class MobileMTTEX
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        private Header header;
        public HeaderInfo headerInfo { get; set; }

        public MobileMTTEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                // Header
                header = br.ReadStruct<Header>();
                headerInfo = new HeaderInfo
                {
                    //Block 1
                    unk1 = (byte)(header.Block1 >> 24),
                    unk2 = (byte)(header.Block1 >> 16 & 0xFF),
                    version = (Version)(header.Block1 & 0xFFFF),

                    //Block 2
                    r1 = (byte)(header.Block2 >> 4),
                    r3 = (byte)(header.Block2 & 0xF),

                    //Block 3
                    unk3 = (byte)(header.Block3 >> 30),
                    format = (Format)(header.Block3 >> 24 & 0x3F),
                    width = (short)(header.Block3 >> 13 & 0x1FFF),
                    height = (short)(header.Block3 & 0x1FFF)
                };

                //var format = HeaderInfo.Format.ToString().StartsWith("DXT1") ? Format.DXT1 : HeaderInfo.Format.ToString().StartsWith("DXT5") ? Format.DXT5 : HeaderInfo.Format;
                //Settings.Format = ImageSettings.ConvertFormat(format);
                //if (Settings.Format.ToString().Contains("DXT"))
                //{
                //    Settings.PadToPowerOf2 = false;
                //    Settings.ZOrder = false;
                //    Settings.TileSize = 4;
                //}

                //var mipMapOffsets = br.ReadMultiple<int>(HeaderInfo.MipMapCount);

                //for (var i = 0; i < mipMapOffsets.Count; i++)
                //{
                //  var texDataSize = (i + 1 < mipMapOffsets.Count ? mipMapOffsets[i + 1] : (int)br.BaseStream.Length) - mipMapOffsets[i];
                //Settings.Width = Math.Max(HeaderInfo.Width >> i, 2);
                //Settings.Height = Math.Max(HeaderInfo.Height >> i, 2);

                //if (HeaderInfo.Format == Format.DXT5_B)
                //    Settings.PixelShader = ToNoAlpha;
                //else if (HeaderInfo.Format == Format.DXT5_YCbCr)
                //    Settings.PixelShader = ToProperColors;

                //bmps.Add(Common.Load(br.ReadBytes(texDataSize), settings));
                //}
            }
        }

        // Currently trying out YCbCr:
        // https://en.wikipedia.org/wiki/YCbCr#JPEG_conversion
        /*private static int Clamp(double n) => (int)Math.Max(0, Math.Min(n, 255));
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
            return Color.FromArgb(Clamp(Y), Clamp(Cr), A, Clamp(Cb));
        }*/

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                /*Header.Block1 = (uint)((int)HeaderInfo.Version | (HeaderInfo.Unknown1 << 12) | (HeaderInfo.Unused1 << 24) | ((int)HeaderInfo.AlphaChannelFlags << 28));
                Header.Block2 = (uint)(HeaderInfo.MipMapCount | (HeaderInfo.Width << 6) | (HeaderInfo.Height << 19));
                Header.Block3 = (uint)(HeaderInfo.Unknown2 | ((int)HeaderInfo.Format << 8) | (HeaderInfo.Unknown3 << 16));
                bw.WriteStruct(Header);

                var format = HeaderInfo.Format.ToString().StartsWith("DXT1") ? Format.DXT1 : HeaderInfo.Format.ToString().StartsWith("DXT5") ? Format.DXT5 : HeaderInfo.Format;
                Settings.Format = ImageSettings.ConvertFormat(format);

                // @todo: add other things like PadToPowerOf2, ZOrder and TileSize

                if (HeaderInfo.Format == Format.DXT5_B)
                    Settings.PixelShader = ToNoAlpha;
                else if (HeaderInfo.Format == Format.DXT5_YCbCr)
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
                    bw.Write(bitmap);*/
            }
        }
    }
}
