using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;

namespace Kontract.Image
{
    /// <summary>
    /// Defines the settings with which an image will be loaded/saved
    /// </summary>
    public class ImageSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public IImageFormat Format { get; set; }

        public int padWidth { get; set; } = 0;
        public int padHeight { get; set; } = 0;

        public List<IImageSwizzle> InnerSwizzle { get; set; } = new List<IImageSwizzle>();
        public List<IImageSwizzle> OuterSwizzle { get; set; } = new List<IImageSwizzle>();
        public Func<Color, Color> PixelShader { get; set; }

        public int TileSize { get; set; } = 8;
        public bool PadToPowerOf2 { get; set; } = false;
    }

    /// <summary>
    /// Basic wrapper for all supported Image Formats in Kuriimu
    /// </summary>
    public class Image
    {
        static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max - 1);

        /// <summary>
        /// Gives back a sequence of points, modified by Swizzles if applied
        /// </summary>
        static IEnumerable<Point> GetPointSequence(ImageSettings settings)
        {
            int tileSize = settings.TileSize;
            int powTileSize = tileSize * tileSize;

            int strideWidth = (settings.Width + (tileSize - 1)) & ~(tileSize - 1);
            int strideHeight = (settings.Height + (tileSize - 1)) & ~(tileSize - 1);
            if (settings.PadToPowerOf2)
            {
                strideWidth = 2 << (int)Math.Log(strideWidth - 1, 2);
                strideHeight = 2 << (int)Math.Log(strideHeight - 1, 2);
            }
            else if (settings.padWidth != 0 || settings.padHeight != 0)
            {
                strideWidth = (settings.padWidth != 0) ? settings.padWidth : strideWidth;
                strideHeight = (settings.padHeight != 0) ? settings.padHeight : strideHeight;
            }

            var innerTile = GetInnerTile(settings);
            for (int i = 0; i < strideWidth * strideHeight; i += powTileSize)
            {
                var outerPoint = new Point(i / powTileSize % (strideWidth / tileSize), i / (tileSize * strideWidth));
                foreach (var swizzle in settings.OuterSwizzle)
                    outerPoint = swizzle.Get(outerPoint, strideWidth / tileSize, strideHeight / tileSize);

                foreach (var innerPoint in innerTile)
                    yield return new Point(outerPoint.X * tileSize + innerPoint.X, outerPoint.Y * tileSize + innerPoint.Y);
            }
        }

        /// <summary>
        /// Gives back a sequence of points in a tile, modified by inner Swizzles
        /// </summary>
        static IEnumerable<Point> GetInnerTile(ImageSettings settings)
        {
            int tileSize = settings.TileSize;
            int powTileSize = tileSize * tileSize;

            for (int i = 0; i < powTileSize; i++)
            {
                var point = new Point(i % powTileSize % tileSize, i % powTileSize / tileSize);
                foreach (var swizzle in settings.InnerSwizzle)
                    point = swizzle.Get(point, tileSize, tileSize);
                yield return point;
            }
        }

        /// <summary>
        /// Loads the binary data with given settings as an image
        /// </summary>
        /// <param name="tex">
        /// Bytearray containing the binary image data
        /// </param>
        /// <param name="settings">
        /// The settings determining the final image output
        /// </param>
        public static Bitmap Load(byte[] tex, ImageSettings settings)
        {
            int width = settings.Width, height = settings.Height;

            var points = GetPointSequence(settings);

            var bmp = new Bitmap(width, height);
            var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                var ptr = (int*)data.Scan0;
                foreach (var pair in points.Zip(settings.Format.Load(tex), Tuple.Create))
                {
                    int x = pair.Item1.X, y = pair.Item1.Y;
                    if (0 <= x && x < width && 0 <= y && y < height)
                    {
                        var color = pair.Item2;
                        if (settings.PixelShader != null) color = settings.PixelShader(color);
                        ptr[data.Stride * y / 4 + x] = color.ToArgb();
                    }
                }
            }
            bmp.UnlockBits(data);

            return bmp;
        }

        /// <summary>
        /// Converts a given Bitmap, modified by given settings, in binary data
        /// </summary>
        /// <param name="bmp">
        /// the bitmap, which will be converted
        /// </param>
        /// <param name="settings">
        /// Settings like Format, Dimensions and Swizzles
        /// </param>
        public static byte[] Save(Bitmap bmp, ImageSettings settings)
        {
            //settings.Width = bmp.Width;
            //settings.Height = bmp.Height;

            var points = GetPointSequence(settings);

            var colors = new List<Color>();
            foreach (var point in points)
            {
                int x = Clamp(point.X, 0, bmp.Width);
                int y = Clamp(point.Y, 0, bmp.Height);

                var color = bmp.GetPixel(x, y);
                if (settings.PixelShader != null) color = settings.PixelShader(color);

                colors.Add(color);
            }

            return settings.Format.Save(colors);
        }

        /*static IEnumerable<Color> GetColorsFromTexture(byte[] tex, ImageSettings settings)
        {
            var format = settings.Format;

            using (var br = new BinaryReaderX(new MemoryStream(tex)))
            {
                var etc1decoder = new ETC1.Decoder();

                Enum.TryParse<DXT.Formats>(format.ToString(), false, out var dxtFormat);
                var dxtdecoder = new DXT.Decoder(dxtFormat);

                Enum.TryParse<ATI.Formats>(format.ToString(), false, out var atiFormat);
                var atidecoder = new ATI.Decoder(atiFormat);

                while (true)
                {
                    int a = 255, r = 255, g = 255, b = 255;
                    switch (format)
                    {
                        case Format.L8:
                            b = g = r = br.ReadByte();
                            break;
                        case Format.A8:
                            a = br.ReadByte();
                            break;
                        case Format.LA44:
                            a = br.ReadNibble() * 17;
                            b = g = r = br.ReadNibble() * 17;
                            break;
                        case Format.LA88:
                            if (settings.ByteOrder == ByteOrder.LittleEndian)
                            {
                                a = br.ReadByte();
                                b = g = r = br.ReadByte();
                            }
                            else
                            {
                                b = g = r = br.ReadByte();
                                a = br.ReadByte();
                            }
                            break;
                        case Format.HL88:
                            g = br.ReadByte();
                            r = br.ReadByte();
                            break;
                        case Format.RGB565:
                            var s = br.ReadUInt16();
                            b = (s % 32) * 33 / 4;
                            g = (s >> 5) % 64 * 65 / 16;
                            r = (s >> 11) * 33 / 4;
                            break;
                        case Format.RGB888:
                            b = br.ReadByte();
                            g = br.ReadByte();
                            r = br.ReadByte();
                            break;
                        case Format.RGBA5551:
                            var s2 = br.ReadUInt16();
                            a = (s2 & 1) * 255;
                            b = (s2 >> 1) % 32 * 33 / 4;
                            g = (s2 >> 6) % 32 * 33 / 4;
                            r = (s2 >> 11) % 32 * 33 / 4;
                            break;
                        case Format.RGBA4444:
                            a = br.ReadNibble() * 17;
                            b = br.ReadNibble() * 17;
                            g = br.ReadNibble() * 17;
                            r = br.ReadNibble() * 17;
                            break;
                        case Format.RGBA8888:
                            a = br.ReadByte();
                            b = br.ReadByte();
                            g = br.ReadByte();
                            r = br.ReadByte();
                            break;
                        case Format.RGBA1010102:
                            var pack = br.ReadUInt32();
                            r = (int)((pack >> 22) / 4);
                            g = (int)(((pack >> 12) & 0x3FF) / 4);
                            b = (int)(((pack >> 2) & 0x3FF) / 4);
                            a = (int)((pack & 0x3) * 85);
                            break;
                        case Format.ETC1:
                        case Format.ETC1A4:
                            yield return etc1decoder.Get(() =>
                            {
                                var etc1Alpha = format == Format.ETC1A4 ? br.ReadUInt64() : ulong.MaxValue;
                                return new ETC1.PixelData { Alpha = etc1Alpha, Block = br.ReadStruct<ETC1.Block>() };
                            });
                            continue;
                        case Format.DXT1:
                        case Format.DXT3:
                        case Format.DXT5:
                            yield return dxtdecoder.Get(() =>
                            {
                                if (br.BaseStream.Position == br.BaseStream.Length) return (0, 0);
                                var dxt5Alpha = format == Format.DXT3 || format == Format.DXT5 ? br.ReadUInt64() : 0;
                                return (dxt5Alpha, br.ReadUInt64());
                            });
                            continue;
                        case Format.ATI1L:
                        case Format.ATI1A:
                        case Format.ATI2:
                            yield return atidecoder.Get(() =>
                            {
                                if (br.BaseStream.Position == br.BaseStream.Length) return (0, 0);
                                return (br.ReadUInt64(), format == Format.ATI2 ? br.ReadUInt64() : 0);
                            });
                            continue;
                        case Format.L4:
                            b = g = r = br.ReadNibble() * 17;
                            break;
                        case Format.A4:
                            a = br.ReadNibble() * 17;
                            break;
                        case Format.PVRTC:
                            var bmp = PVRTC.PvrtcDecompress.DecodeRgb4Bpp(tex, settings.Width);
                            for (int y = 0; y < settings.Height; y++)
                                for (int x = 0; x < settings.Width; x++)
                                    yield return bmp.GetPixel(x, y);
                            continue;
                        case Format.PVRTCA:
                            bmp = PVRTC.PvrtcDecompress.DecodeRgba4Bpp(tex, settings.Width);
                            for (int y = 0; y < settings.Height; y++)
                                for (int x = 0; x < settings.Width; x++)
                                    yield return bmp.GetPixel(x, y);
                            continue;
                        default:
                            throw new NotSupportedException($"Unknown image format {format}");
                    }
                    yield return Color.FromArgb(a, r, g, b);
                }
            }
        }*/

        /*static IEnumerable<Point> GetPointSequence(ImageSettings settings)
        {
            switch (settings.Format)
            {
                case Format.ATI1A:
                case Format.ATI1L:
                case Format.ATI2:
                case Format.ETC1:
                case Format.ETC1A4:
                case Format.DXT1:
                case Format.DXT3:
                case Format.DXT5:
                    settings.TileSize = settings.TileSize + 3 & ~0x3;
                    break;
            }

            int strideWidth = (settings.Width + 7) & ~7;
            int strideHeight = (settings.Height + 7) & ~7;
            if (settings.PadToPowerOf2)
            {
                strideWidth = 2 << (int)Math.Log(strideWidth - 1, 2);
                strideHeight = 2 << (int)Math.Log(strideHeight - 1, 2);
            }

            //stride TileSize
            var tileSize = 0;
            if (settings.ZOrder)
                tileSize = 2 << (int)(Math.Log(((settings.TileSize + 7) & ~7) - 1, 2));
            else
                tileSize = settings.TileSize;
            int powTileSize = (int)Math.Pow(tileSize, 2);

            int stride = strideWidth;
            switch (settings.Orientation)
            {
                case Orientation.Rotate90:
                case Orientation.Transpose:
                    stride = strideHeight;
                    break;
            }

            for (int i = 0; i < strideWidth * strideHeight; i++)
            {
                //in == order inside a tile
                //out == order of tiles themselves
                int x_out = 0, y_out = 0, x_in = 0, y_in = 0;
                if (settings.ZOrder)
                {
                    x_out = (i / powTileSize % (stride / tileSize)) * tileSize;
                    y_out = (i / powTileSize / (stride / tileSize)) * tileSize;
                    x_in = ZOrderX(tileSize, i);
                    y_in = ZOrderY(tileSize, i);
                }
                else
                {
                    x_out = (i / powTileSize % (stride / tileSize)) * tileSize;
                    y_out = (i / powTileSize / (stride / tileSize)) * tileSize;

                    switch (settings.Format)
                    {
                        case Format.ATI1A:
                        case Format.ATI1L:
                        case Format.ATI2:
                        case Format.ETC1:
                        case Format.ETC1A4:
                        case Format.DXT1:
                        case Format.DXT3:
                        case Format.DXT5:
                            x_in = (i % 4 + i % powTileSize / 16 * 4) % tileSize;
                            y_in = (i % 16 / 4 + i / (tileSize * 4) * 4) % tileSize;
                            break;
                        default:
                            x_in = i % powTileSize % tileSize;
                            y_in = i % powTileSize / tileSize;
                            break;
                    }
                }

                switch (settings.Orientation)
                {
                    case Orientation.Default:
                        yield return new Point(x_out + x_in, y_out + y_in);
                        break;
                    case Orientation.HorizontalFlip:
                        yield return new Point(stride - 1 - (x_out + x_in), y_out + y_in);
                        break;
                    case Orientation.Rotate90:
                        yield return new Point(y_out + y_in, stride - 1 - (x_out + x_in));
                        break;
                    case Orientation.Transpose:
                        yield return new Point(y_out + y_in, x_out + x_in);
                        break;
                    case Orientation.TransposeTile:
                        yield return new Point(x_out + y_in, y_out + x_in);
                        break;
                    default:
                        throw new NotSupportedException($"Unknown orientation format {settings.Orientation}");
                }
            }
        }
        static int ZOrderX(int tileSize, int count)
        {
            var div = tileSize / 2;
            var x_in = count / div & div;

            while (div > 1)
            {
                div /= 2;
                x_in |= count / div & div;
            }

            return x_in;
        }
        static int ZOrderY(int tileSize, int count)
        {
            var div = tileSize;
            var div2 = tileSize / 2;
            var y_in = count / div & div2;

            while (div2 > 1)
            {
                div /= 2;
                div2 /= 2;
                y_in |= count / div & div2;
            }

            return y_in;
        }

        public static Bitmap Load(byte[] tex, ImageSettings settings)
        {
            int width = settings.Width, height = settings.Height;
            var colors = GetColorsFromTexture(tex, settings);

            var points = GetPointSequence(settings);

            // Now we just need to merge the points with the colors
            var bmp = new Bitmap(width, height);
            var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                var ptr = (int*)data.Scan0;
                foreach (var pair in points.Zip(colors, Tuple.Create))
                {
                    int x = pair.Item1.X, y = pair.Item1.Y;
                    if (0 <= x && x < width && 0 <= y && y < height)
                    {
                        var color = pair.Item2;
                        if (settings.PixelShader != null) color = settings.PixelShader(color);
                        ptr[data.Stride * y / 4 + x] = color.ToArgb();
                    }
                }
            }
            bmp.UnlockBits(data);
            return bmp;
        }

        public static byte[] Save(Bitmap bmp, ImageSettings settings)
        {
            settings.Width = bmp.Width;
            settings.Height = bmp.Height;
            var points = GetPointSequence(settings);

            var ms = new MemoryStream();
            var etc1encoder = new ETC1.Encoder();
            Enum.TryParse<DXT.Formats>(settings.Format.ToString(), false, out var dxtFormat);
            var dxtencoder = new DXT.Encoder(dxtFormat);

            using (var bw = new BinaryWriterX(ms))
            {
                foreach (var point in points)
                {
                    int x = Clamp(point.X, 0, bmp.Width);
                    int y = Clamp(point.Y, 0, bmp.Height);

                    var color = bmp.GetPixel(x, y);
                    if (settings.PixelShader != null) color = settings.PixelShader(color);

                    switch (settings.Format)
                    {
                        case Format.L8:
                            bw.Write(color.G);
                            break;
                        case Format.A8:
                            bw.Write(color.A);
                            break;
                        case Format.LA44:
                            bw.WriteNibble(color.A / 16);
                            bw.WriteNibble(color.G / 16);
                            break;
                        case Format.LA88:
                            bw.Write(color.A);
                            bw.Write(color.G);
                            break;
                        case Format.HL88:
                            bw.Write(color.G);
                            bw.Write(color.R);
                            break;
                        case Format.RGB565:
                            bw.Write((short)((color.R / 8 << 11) | (color.G / 4 << 5) | (color.B / 8)));
                            break;
                        case Format.RGB888:
                            bw.Write(color.B);
                            bw.Write(color.G);
                            bw.Write(color.R);
                            break;
                        case Format.RGBA5551:
                            bw.Write((short)((color.R / 8 << 11) | (color.G / 8 << 6) | (color.B / 8 << 1) | color.A / 128));
                            break;
                        case Format.RGBA4444:
                            bw.WriteNibble(color.A / 16);
                            bw.WriteNibble(color.B / 16);
                            bw.WriteNibble(color.G / 16);
                            bw.WriteNibble(color.R / 16);
                            break;
                        case Format.RGBA8888:
                            bw.Write(color.A);
                            bw.Write(color.B);
                            bw.Write(color.G);
                            bw.Write(color.R);
                            break;
                        case Format.ETC1:
                        case Format.ETC1A4:
                            etc1encoder.Set(color, data =>
                            {
                                if (settings.Format == Format.ETC1A4) bw.Write(data.Alpha);
                                bw.WriteStruct(data.Block);
                            });
                            break;
                        case Format.DXT1:
                        case Format.DXT5:
                            dxtencoder.Set(color, data =>
                            {
                                if (settings.Format == Format.DXT5) bw.Write(data.alpha);
                                bw.Write(data.block);
                            });
                            break;
                        case Format.L4:
                            bw.WriteNibble(color.G / 16);
                            break;
                        case Format.A4:
                            bw.WriteNibble(color.A / 16);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
            }

            return ms.ToArray();
        }*/
    }
}