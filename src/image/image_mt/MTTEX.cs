using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Image;
using Kontract.Image.Swizzle;
using Kontract.IO;

namespace image_mt
{
    public partial class MTTEX
    {
        public List<Bitmap> Bitmaps = new List<Bitmap>();
        private const int MinHeight = 8;

        private Header Header;
        public HeaderInfo HeaderInfo { get; set; }
        private int HeaderLength = 0x10;
        public ImageSettings Settings = new ImageSettings();
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
                    Format = (byte)((Header.Block3 >> 8) & 0xFF),
                    Unknown3 = (int)((Header.Block3 >> 16) & 0xFFFF)
                };

                // @todo: Consider whether the following settings make more sense if conditioned by the ByteOrder (or Platform)

                //var format = HeaderInfo.Format.ToString().StartsWith("DXT1") ? Format.DXT1 : HeaderInfo.Format.ToString().StartsWith("DXT5") ? Format.DXT5 : HeaderInfo.Format;
                Settings.Format = Formats[HeaderInfo.Format];

                var mipMaps = br.ReadMultiple<int>(HeaderInfo.MipMapCount);

                for (var i = 0; i < mipMaps.Count; i++)
                {
                    var texDataSize = (i + 1 < mipMaps.Count ? mipMaps[i + 1] : (int)br.BaseStream.Length) - mipMaps[i];

                    Settings.Width = Math.Max(HeaderInfo.Width >> i, 2);
                    Settings.Height = Math.Max(HeaderInfo.Height >> i, 2);
                    if (Settings.Format.FormatName.Contains("DXT"))
                        Settings.Swizzle = new BlockSwizzle(Settings.Width, Settings.Height);
                    else
                        Settings.Swizzle = new CTRSwizzle(Settings.Width, Settings.Height);

                    if ((Format)HeaderInfo.Format == Format.DXT5_B)
                        Settings.PixelShader = ToNoAlpha;
                    else if ((Format)HeaderInfo.Format == Format.DXT5_YCbCr)
                        Settings.PixelShader = ToProperColors;

                    Bitmaps.Add(Common.Load(br.ReadBytes(texDataSize), Settings));
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, ByteOrder))
            {
                Header.Block1 = (uint)((int)HeaderInfo.Version | (HeaderInfo.Unknown1 << 12) | (HeaderInfo.Unused1 << 24) | ((int)HeaderInfo.AlphaChannelFlags << 28));
                Header.Block2 = (uint)(HeaderInfo.MipMapCount | (HeaderInfo.Width << 6) | (HeaderInfo.Height << 19));
                Header.Block3 = (uint)(HeaderInfo.Unknown2 | ((int)HeaderInfo.Format << 8) | (HeaderInfo.Unknown3 << 16));
                bw.WriteStruct(Header);

                //var format = HeaderInfo.Format.ToString().StartsWith("DXT1") ? Format.DXT1 : HeaderInfo.Format.ToString().StartsWith("DXT5") ? Format.DXT5 : HeaderInfo.Format;
                Settings.Format = Formats[HeaderInfo.Format];

                // @todo: add other things like PadToPowerOf2, ZOrder and TileSize

                if ((Format)HeaderInfo.Format == Format.DXT5_B)
                    Settings.PixelShader = ToNoAlpha;
                else if ((Format)HeaderInfo.Format == Format.DXT5_YCbCr)
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
