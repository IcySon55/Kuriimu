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

        int Version;

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
                if (br.PeekString(4) == "\0XET")
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
                Settings.Format = (HeaderInfo.Version == image_mt.Version._Switchv1) ? SwitchFormats[HeaderInfo.Format] : Formats[HeaderInfo.Format];

                List<int> mipMaps = null;
                if (HeaderInfo.Version == image_mt.Version._Switchv1)
                {
                    var texOverallSize = br.ReadInt32();
                    mipMaps = br.ReadMultiple<int>(HeaderInfo.MipMapCount);
                }
                else if (HeaderInfo.Version != image_mt.Version._3DSv1)
                    mipMaps = br.ReadMultiple<int>(HeaderInfo.MipMapCount);

                for (var i = 0; i < HeaderInfo.MipMapCount; i++)
                {
                    int texDataSize = 0;
                    if (HeaderInfo.Version != image_mt.Version._3DSv1)
                        texDataSize = (i + 1 < HeaderInfo.MipMapCount ? mipMaps[i + 1] : (int)br.BaseStream.Length) - mipMaps[i];
                    else
                        texDataSize = Formats[HeaderInfo.Format].BitDepth * (HeaderInfo.Width >> i) * (HeaderInfo.Height >> i) / 8;

                    Settings.Width = Math.Max(HeaderInfo.Width >> i, 2);
                    Settings.Height = Math.Max(HeaderInfo.Height >> i, 2);

                    //Set possible Swizzles
                    if (HeaderInfo.Version == image_mt.Version._3DSv1 || HeaderInfo.Version == image_mt.Version._3DSv2 || HeaderInfo.Version == image_mt.Version._3DSv3)
                        Settings.Swizzle = new CTRSwizzle(Settings.Width, Settings.Height);
                    else if (HeaderInfo.Version == image_mt.Version._Switchv1)
                        Settings.Swizzle = new SwitchSwizzle(Settings.Width, Settings.Height, Settings.Format.BitDepth, GetSwitchSwizzleFormat(Settings.Format.FormatName));
                    else if (Settings.Format.FormatName.Contains("DXT"))
                        Settings.Swizzle = new BlockSwizzle(Settings.Width, Settings.Height);

                    //Set possible pixel shaders
                    if ((Format)HeaderInfo.Format == Format.DXT5_B)
                        Settings.PixelShader = ToNoAlpha;
                    else if ((Format)HeaderInfo.Format == Format.DXT5_YCbCr)
                        Settings.PixelShader = ToProperColors;

                    Bitmaps.Add(Common.Load(br.ReadBytes(texDataSize), Settings));
                }
            }
        }

        SwitchSwizzle.Format GetSwitchSwizzleFormat(string formatName)
        {
            switch (formatName)
            {
                case "DXT1":
                    return SwitchSwizzle.Format.BC1;
                case "DXT5":
                    return SwitchSwizzle.Format.BC3;
                case "RGBA8888":
                    return SwitchSwizzle.Format.RGBA8888;
                default:
                    return SwitchSwizzle.Format.Empty;
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
                Settings.Format = (HeaderInfo.Version == image_mt.Version._Switchv1) ? SwitchFormats[HeaderInfo.Format] : Formats[HeaderInfo.Format];

                if ((Format)HeaderInfo.Format == Format.DXT5_B)
                    Settings.PixelShader = ToNoAlpha;
                else if ((Format)HeaderInfo.Format == Format.DXT5_YCbCr)
                    Settings.PixelShader = ToOptimisedColors;

                List<byte[]> bitmaps = new List<byte[]>();
                foreach (var bmp in Bitmaps)
                {
                    //Set possible Swizzles
                    if (HeaderInfo.Version == image_mt.Version._3DSv1 || HeaderInfo.Version == image_mt.Version._3DSv2 || HeaderInfo.Version == image_mt.Version._3DSv3)
                        Settings.Swizzle = new CTRSwizzle(bmp.Width, bmp.Height);
                    else if (HeaderInfo.Version == image_mt.Version._Switchv1)
                        Settings.Swizzle = new SwitchSwizzle(bmp.Width, bmp.Height, Settings.Format.BitDepth, GetSwitchSwizzleFormat(Settings.Format.FormatName));    //Switch Swizzle
                    else if (Settings.Format.FormatName.Contains("DXT"))
                        Settings.Swizzle = new BlockSwizzle(bmp.Width, bmp.Height);

                    bitmaps.Add(Common.Save(bmp, Settings));
                }

                // Mipmaps, but not for Version 3DS v1
                if (HeaderInfo.Version != image_mt.Version._3DSv1)
                {
                    if (HeaderInfo.Version == image_mt.Version._Switchv1)
                        bw.Write(bitmaps.Sum(b => b.Length));
                    var offset = HeaderInfo.Version == image_mt.Version._PS3v1 ? HeaderInfo.MipMapCount * sizeof(int) + HeaderLength : 0;
                    foreach (var bitmap in bitmaps)
                    {
                        bw.Write(offset);
                        offset += bitmap.Length;
                    }
                }

                // Bitmaps
                foreach (var bitmap in bitmaps)
                    bw.Write(bitmap);
            }
        }
    }
}
