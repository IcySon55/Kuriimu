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


                /*Version = br.ReadInt32() & 0xFFF;
                br.BaseStream.Position = 0;
                switch (Version)
                {
                    //encountered on Switch
                    case 0xa0:  //Monster Hunter XX
                        break;
                    //encountered on PS3
                    case 0x9a:  //E.X.Troopers
                        Header = br.ReadStruct<Header>();
                        var LocHeader = (Header)Header;
                        HeaderInfo = new HeaderInfo
                        {
                            // Block 1
                            Version = (int)LocHeader.Block1 & 0xFFF,
                            Unknown1 = (int)((LocHeader.Block1 >> 12) & 0xFFF),
                            Unused1 = (int)((LocHeader.Block1 >> 24) & 0xF),
                            AlphaChannelFlags = (AlphaChannelFlags)((LocHeader.Block1 >> 28) & 0xF),
                            // Block 2
                            MipMapCount = (int)(LocHeader.Block2 & 0x3F),
                            Width = (int)((LocHeader.Block2 >> 6) & 0x1FFF),
                            Height = Math.Max((int)((LocHeader.Block2 >> 19) & 0x1FFF), MinHeight),
                            // Block 3
                            Unknown2 = (int)(LocHeader.Block3 & 0xFF),
                            Format = (byte)((LocHeader.Block3 >> 8) & 0xFF),
                            Unknown3 = (int)((LocHeader.Block3 >> 16) & 0xFFFF)
                        };

                        var LocHeaderInfo = (HeaderInfo)HeaderInfo;
                        Settings.Format = Formats[LocHeaderInfo.Format];

                        var mipMaps = br.ReadMultiple<int>(LocHeaderInfo.MipMapCount);
                        for (var i = 0; i < mipMaps.Count; i++)
                        {
                            var texDataSize = (i + 1 < mipMaps.Count ? mipMaps[i + 1] : (int)br.BaseStream.Length) - mipMaps[i];

                            Settings.Width = Math.Max(LocHeaderInfo.Width >> i, 2);
                            Settings.Height = Math.Max(LocHeaderInfo.Height >> i, 2);
                            if (Settings.Format.FormatName.Contains("DXT"))
                                Settings.Swizzle = new BlockSwizzle(Settings.Width, Settings.Height);

                            if ((Format)LocHeaderInfo.Format == Format.DXT5_B)
                                Settings.PixelShader = ToNoAlpha;
                            else if ((Format)LocHeaderInfo.Format == Format.DXT5_YCbCr)
                                Settings.PixelShader = ToProperColors;

                            Bitmaps.Add(Common.Load(br.ReadBytes(texDataSize), Settings));
                        }
                        break;
                    //encountered on 3DS
                    case 0xa4:  //Resident Evil The Mercenaries,
                    case 0xa5:  //Dual Destinies
                    case 0xa6:  //Spirit of Justice, Dai Gyakuten Saiban, Dai Gyakuten Saiban 2
                        Header = br.ReadStruct<Header>();
                        LocHeader = (Header)Header;
                        HeaderInfo = new HeaderInfo
                        {
                            // Block 1
                            Version = (int)LocHeader.Block1 & 0xFFF,
                            Unknown1 = (int)((LocHeader.Block1 >> 12) & 0xFFF),
                            Unused1 = (int)((LocHeader.Block1 >> 24) & 0xF),
                            AlphaChannelFlags = (AlphaChannelFlags)((LocHeader.Block1 >> 28) & 0xF),
                            // Block 2
                            MipMapCount = (int)(LocHeader.Block2 & 0x3F),
                            Width = (int)((LocHeader.Block2 >> 6) & 0x1FFF),
                            Height = Math.Max((int)((LocHeader.Block2 >> 19) & 0x1FFF), MinHeight),
                            // Block 3
                            Unknown2 = (int)(LocHeader.Block3 & 0xFF),
                            Format = (byte)((LocHeader.Block3 >> 8) & 0xFF),
                            Unknown3 = (int)((LocHeader.Block3 >> 16) & 0xFFFF)
                        };

                        LocHeaderInfo = (HeaderInfo)HeaderInfo;
                        Settings.Format = Formats[LocHeaderInfo.Format];
                        Settings.Swizzle = new CTRSwizzle(Settings.Width, Settings.Height);

                        if (Version != 0xa4)
                        {
                            mipMaps = br.ReadMultiple<int>(LocHeaderInfo.MipMapCount);
                            for (var i = 0; i < mipMaps.Count; i++)
                            {
                                var texDataSize = (i + 1 < mipMaps.Count ? mipMaps[i + 1] : (int)br.BaseStream.Length) - mipMaps[i];

                                Settings.Width = Math.Max(LocHeaderInfo.Width >> i, 2);
                                Settings.Height = Math.Max(LocHeaderInfo.Height >> i, 2);
                                Settings.Swizzle = new CTRSwizzle(Settings.Width, Settings.Height);

                                Bitmaps.Add(Common.Load(br.ReadBytes(texDataSize), Settings));
                            }
                        }
                        else
                        {
                            for (var i = 0; i < LocHeaderInfo.MipMapCount; i++)
                            {
                                var texDataSize = Formats[LocHeaderInfo.Format].BitDepth * (LocHeaderInfo.Width >> i) * (LocHeaderInfo.Height >> i) / 8;

                                Settings.Width = Math.Max(LocHeaderInfo.Width >> i, 2);
                                Settings.Height = Math.Max(LocHeaderInfo.Height >> i, 2);
                                Settings.Swizzle = new CTRSwizzle(Settings.Width, Settings.Height);

                                Bitmaps.Add(Common.Load(br.ReadBytes(texDataSize), Settings));
                            }
                        }
                        break;
                    //encountered on mobile
                    case 0x09:
                        break;
                }*/

                // Header
                Header = br.ReadStruct<Header>();
                HeaderInfo = new HeaderInfo
                {
                    // Block 1
                    Version = (int)Header.Block1 & 0xFFF,
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

                List<int> mipMaps = null;
                if (HeaderInfo.Version != 0xa4)
                    mipMaps = br.ReadMultiple<int>(HeaderInfo.MipMapCount);
                //br.BaseStream.Position = br.BaseStream.Position + (HeaderInfo.Unknown1 - 1) & ~(HeaderInfo.Unknown1 - 1);

                for (var i = 0; i < HeaderInfo.MipMapCount; i++)
                {
                    int texDataSize = 0;
                    if (HeaderInfo.Version != 0xa4)
                        texDataSize = (i + 1 < mipMaps.Count ? mipMaps[i + 1] : (int)br.BaseStream.Length) - mipMaps[i];
                    else
                        texDataSize = Formats[HeaderInfo.Format].BitDepth * (HeaderInfo.Width >> i) * (HeaderInfo.Height >> i) / 8;

                    Settings.Width = Math.Max(HeaderInfo.Width >> i, 2);
                    Settings.Height = Math.Max(HeaderInfo.Height >> i, 2);

                    //Set possible Swizzles
                    if (Settings.Format.FormatName.Contains("DXT"))
                        Settings.Swizzle = new BlockSwizzle(Settings.Width, Settings.Height);
                    else if (HeaderInfo.Version == 0xa4 || HeaderInfo.Version == 0xa5 || HeaderInfo.Version == 0xa6)
                        Settings.Swizzle = new CTRSwizzle(Settings.Width, Settings.Height);
                    else if (HeaderInfo.Version == 0xa0)
                        Settings.Swizzle = null;    //Switch Swizzle

                    //Set possible pixel shaders
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

                if ((Format)HeaderInfo.Format == Format.DXT5_B)
                    Settings.PixelShader = ToNoAlpha;
                else if ((Format)HeaderInfo.Format == Format.DXT5_YCbCr)
                    Settings.PixelShader = ToOptimisedColors;

                List<byte[]> bitmaps = new List<byte[]>();
                foreach (var bmp in Bitmaps)
                {
                    //Set possible Swizzles
                    if (Settings.Format.FormatName.Contains("DXT"))
                        Settings.Swizzle = new BlockSwizzle(bmp.Width, bmp.Height);
                    else if (HeaderInfo.Version == 0xa4 || HeaderInfo.Version == 0xa5 || HeaderInfo.Version == 0xa6)
                        Settings.Swizzle = new CTRSwizzle(bmp.Width, bmp.Height);
                    else if (HeaderInfo.Version == 0xa0)
                        Settings.Swizzle = null;    //Switch Swizzle

                    bitmaps.Add(Common.Save(bmp, Settings));
                }

                // Mipmaps, but not for Version 0xa4
                if (HeaderInfo.Version != 0xa4)
                {
                    var offset = HeaderInfo.Version == 0x9a ? HeaderInfo.MipMapCount * sizeof(int) + HeaderLength : 0;
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
