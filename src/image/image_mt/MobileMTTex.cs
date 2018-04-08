using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Image;
using Kontract.Image.Format;

namespace image_mt.Mobile
{
    public class MobileMTTEX
    {
        public List<Bitmap> bmps = new List<Bitmap>();
        public List<string> formatNames = new List<string>();
        public ImageSettings settings = new ImageSettings();

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
                    mipMapCount = (byte)(header.Block2 & 0xF),

                    //Block 3
                    format = (int)(header.Block3 >> 26 & 0x3F),
                    width = (short)(header.Block3 >> 13 & 0x1FFF),
                    height = (short)(header.Block3 & 0x1FFF)
                };

                if (headerInfo.format == 0x31)
                {
                    var texOffsets = br.ReadMultiple<int>(3);
                    var texSizes = br.ReadMultiple<int>(3);
                    for (var i = 0; i < texOffsets.Count; i++)
                    {
                        br.BaseStream.Position = texOffsets[i];

                        settings.Width = Math.Max((int)headerInfo.width, 2);
                        settings.Height = Math.Max((int)headerInfo.height, 2);

                        if (i == 0 || i == 2)
                        {
                            settings.Format = new DXT(DXT.Version.DXT5);
                            settings.Swizzle = new BlockSwizzle(headerInfo.width, headerInfo.height);
                        }
                        else
                        {
                            settings.Format = new PVRTC(PVRTC.Format.PVRTCA_4bpp);
                            (settings.Format as PVRTC)._width = headerInfo.width;
                            (settings.Format as PVRTC)._height = headerInfo.height;
                            settings.Swizzle = null;
                        }
                        formatNames.Add(settings.Format.FormatName);
                        bmps.Add(Common.Load(br.ReadBytes(texSizes[i]), settings));
                    }
                }
                else
                {
                    settings.Format = Support.Format[headerInfo.format];
                    if (settings.Format.FormatName.Contains("DXT") || settings.Format.FormatName.Contains("ETC"))
                        settings.Swizzle = new BlockSwizzle(headerInfo.width, headerInfo.height);

                    var texOffsets = br.ReadMultiple<int>(3).Distinct().ToList();
                    var texSizes = br.ReadMultiple<int>(3);
                    for (var i = 0; i < texOffsets.Count; i++)
                    {
                        var texDataSize = (i + 1 < texOffsets.Count ? texOffsets[i + 1] : (int)br.BaseStream.Length) - texOffsets[i];
                        settings.Width = Math.Max(headerInfo.width >> i, 2);
                        settings.Height = Math.Max(headerInfo.height >> i, 2);

                        //if (HeaderInfo.Format == Format.DXT5_B)
                        //    Settings.PixelShader = ToNoAlpha;
                        //else if (HeaderInfo.Format == Format.DXT5_YCbCr)
                        //    Settings.PixelShader = ToProperColors;

                        formatNames.Add(settings.Format.FormatName);

                        bmps.Add(Common.Load(br.ReadBytes(texDataSize), settings));
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                header.Block1 = (uint)((ushort)headerInfo.version | (headerInfo.unk2 << 16) | (headerInfo.unk1 << 24));
                header.Block2 = (uint)((headerInfo.r1 << 4) | (headerInfo.mipMapCount));
                header.Block3 = (uint)((headerInfo.format << 26) | (headerInfo.width << 13) | (ushort)headerInfo.height);
                bw.WriteStruct(header);

                if (headerInfo.format == 0x31)
                {
                    bw.BaseStream.Position = 0x10 + 3 * 2 * 0x4;

                    var texOffsets = new List<uint>();
                    var texSizes = new List<uint>();

                    for (int i = 0; i < bmps.Count; i++)
                    {
                        settings = new ImageSettings
                        {
                            Width = bmps[i].Width,
                            Height = bmps[i].Height
                        };

                        if (i == 0 || i == 2)
                        {
                            settings.Format = new DXT(DXT.Version.DXT5);
                            settings.Swizzle = new BlockSwizzle(settings.Width, settings.Height);
                        }
                        else
                        {
                            settings.Format = new PVRTC(PVRTC.Format.PVRTCA_4bpp);
                            (settings.Format as PVRTC)._width = settings.Width;
                            (settings.Format as PVRTC)._height = settings.Height;
                            settings.Swizzle = null;
                        }

                        var texData = Common.Save(bmps[i], settings);

                        texOffsets.Add((uint)bw.BaseStream.Position);
                        texSizes.Add((uint)texData.Length);

                        bw.Write(texData);
                    }

                    bw.BaseStream.Position = 0x10;
                    foreach (var offset in texOffsets)
                        bw.Write(offset);
                    foreach (var size in texSizes)
                        bw.Write(size);
                }
                else
                {
                    var settings = new ImageSettings
                    {
                        Format = Support.Format[headerInfo.format]
                    };

                    var texOffsets = new List<uint>();
                    bw.BaseStream.Position = 0x28;
                    foreach (var bmp in bmps)
                    {
                        settings.Width = bmp.Width;
                        settings.Height = bmp.Height;

                        if (Support.Format[headerInfo.format].FormatName.Contains("DXT") || settings.Format.FormatName.Contains("ETC"))
                            settings.Swizzle = new BlockSwizzle(bmp.Width, bmp.Height);

                        texOffsets.Add((uint)bw.BaseStream.Position);

                        bw.Write(Common.Save(bmp, settings));
                    }
                    while (texOffsets.Count < 3) texOffsets.Add(texOffsets.Last());

                    bw.BaseStream.Position = 0x10;
                    foreach (var offset in texOffsets)
                        bw.Write(offset);
                }
            }
        }
    }
}
