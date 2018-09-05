using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Image;
using Kontract.Image.Format;
using System.Drawing.Drawing2D;

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
                /*br.BaseStream.Position = 4;
                var exotic = br.ReadUInt32() > 0;
                var width = br.ReadInt32();
                var height = br.ReadInt32();
                var set = new ImageSettings
                {
                    Width = width,
                    Height = height,
                    Format = new DXT(DXT.Version.DXT5, exotic),
                    Swizzle = new BlockSwizzle(width, height)
                };
                bmps.Add(Common.Load(br.ReadBytes((int)br.BaseStream.Length - 0x10), set));
                return;*/

                // Header
                header = br.ReadStruct<Header>();
                headerInfo = new HeaderInfo
                {
                    //Block 1
                    unk1 = (byte)(header.Block1 >> 24),
                    format = (byte)(header.Block1 >> 16 & 0xFF),
                    version = (Version)(header.Block1 & 0xFFFF),

                    //Block 2
                    r1 = (byte)(header.Block2 >> 4),
                    unk2 = (byte)(header.Block2 & 0xF),

                    //Block 3
                    mipMapCount = (int)(header.Block3 >> 26 & 0xF),
                    height = (short)(header.Block3 >> 13 & 0x1FFF),
                    width = (short)(header.Block3 & 0x1FFF)
                };

                if (headerInfo.format == 0xc)
                {
                    var texOffsets = br.ReadMultiple<int>(3);
                    var texSizes = br.ReadMultiple<int>(3);
                    for (var i = 0; i < texOffsets.Count; i++)
                    {
                        br.BaseStream.Position = texOffsets[i];

                        settings.Width = Math.Max((int)headerInfo.width, 2);
                        settings.Height = Math.Max((int)headerInfo.height, 2);

                        if (i == 0)
                        {
                            settings.Format = new DXT(DXT.Version.DXT5);
                            settings.Swizzle = new BlockSwizzle(headerInfo.width, headerInfo.height);
                        }
                        else if (i == 1)
                        {
                            settings.Format = new PVRTC(PVRTC.Format.PVRTCA_4bpp);
                            (settings.Format as PVRTC)._width = headerInfo.width;
                            (settings.Format as PVRTC)._height = headerInfo.height;
                            settings.Swizzle = null;
                        }
                        else
                        {
                            settings.Format = new ATC(true, Kontract.Image.Support.ATC.AlphaMode.Interpolated);
                            settings.Swizzle = new BlockSwizzle(headerInfo.width, headerInfo.height);
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

                    var texOffsets = br.ReadMultiple<int>(3);
                    var texSizes = br.ReadMultiple<int>(3);
                    var texMeta = new List<TexMeta>();
                    for (int i = 0; i < 3; i++)
                        if (texMeta.Count(tm => tm.offset == texOffsets[i]) <= 0)
                            texMeta.Add(new TexMeta { offset = texOffsets[i], size = texSizes[i] });
                    for (var i = 0; i < texMeta.Count; i++)
                    {
                        br.BaseStream.Position = texMeta[i].offset;
                        for (var j = 0; j < headerInfo.mipMapCount; j++)
                        {
                            var texDataSize = settings.Format.BitDepth * (headerInfo.width >> j) * (headerInfo.height >> j) / 8;
                            settings.Width = Math.Max(headerInfo.width >> j, 1);
                            settings.Height = Math.Max(headerInfo.height >> j, 1);

                            formatNames.Add(settings.Format.FormatName);

                            bmps.Add(Common.Load(br.ReadBytes(texDataSize), settings));
                        }
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var newHeight = bmps[0].Height;
                var newWidth = bmps[0].Width;
                var newMipMapCount = headerInfo.mipMapCount;
                if (headerInfo.mipMapCount > 1)
                {
                    if (bmps[0].Width >= bmps[0].Height)
                    {
                        newWidth = 2 << (int)Math.Log(bmps[0].Width - 1, 2);
                        newHeight = (int)(newWidth / (bmps[0].Width / bmps[0].Height));

                    } else {
                        newHeight = 2 << (int)Math.Log(bmps[0].Height - 1, 2);
                        newWidth = (int)(newHeight * (bmps[0].Width / bmps[0].Height));
                    }
                    newMipMapCount = ((int)Math.Log(Math.Max(newWidth, newHeight), 2) + 1);
                }

                header.Block1 = (uint)((ushort)headerInfo.version | (headerInfo.format << 16) | (headerInfo.unk1 << 24));
                header.Block2 = (uint)((headerInfo.r1 << 4) | (headerInfo.unk2));
                header.Block3 = (uint)((newMipMapCount << 26) | (newHeight << 13) | (ushort)newWidth);
                bw.WriteStruct(header);

                if (headerInfo.format == 0xc)
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

                        if (i == 0)
                        {
                            settings.Format = new DXT(DXT.Version.DXT5);
                            settings.Swizzle = new BlockSwizzle(settings.Width, settings.Height);
                        }
                        else if (i == 1)
                        {
                            settings.Format = new PVRTC(PVRTC.Format.PVRTCA_4bpp);
                            (settings.Format as PVRTC)._width = settings.Width;
                            (settings.Format as PVRTC)._height = settings.Height;
                            settings.Swizzle = null;
                        }
                        else
                        {
                            settings.Format = new ATC(true, Kontract.Image.Support.ATC.AlphaMode.Interpolated);
                            settings.Swizzle = new BlockSwizzle(settings.Width, settings.Height);
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

                    bw.BaseStream.Position = 0x28;
                    for (int i = 0; i < newMipMapCount; i++)
                    {
                        settings.Width = Math.Max(newWidth >> i, 1);
                        settings.Height = Math.Max(newHeight >> i, 1);

                        if (Support.Format[headerInfo.format].FormatName.Contains("DXT") || settings.Format.FormatName.Contains("ETC"))
                            settings.Swizzle = new BlockSwizzle(newWidth >> i, newHeight >> i);

                        bw.Write(Common.Save(bmps[0].Resize(settings.Width, settings.Height), settings));
                    }

                    bw.BaseStream.Position = 0x10;
                    for (int i = 0; i < 3; i++)
                        bw.Write(0x28);
                }
            }
        }
    }

    public static class MTExtensions
    {
        public static Bitmap Resize(this Image image, int newwidth, int newheight)
        {
            var res = new Bitmap(newwidth, newheight);

            using (var graphic = Graphics.FromImage(res))
            {
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphic.CompositingQuality = CompositingQuality.HighQuality;
                graphic.DrawImage(image, 0, 0, newwidth, newheight);
            }

            return res;
        }
    }
}
