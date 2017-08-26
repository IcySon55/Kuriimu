using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Kuriimu.IO;

namespace Cetera.Image
{
    public enum Format : byte
    {
        RGBA8888, RGB888,
        RGBA5551, RGB565, RGBA4444,
        LA88, HL88, L8, A8, LA44,
        L4, A4, ETC1, ETC1A4,

        // PS3
        DXT1, DXT5
    }

    public enum Orientation : byte
    {
        Default,
        TransposeTile = 1,
        Rotate90 = 4,
        Transpose = 8
    }

    public class ImageSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Format Format { get; set; }
        public Orientation Orientation { get; set; } = Orientation.Default;
        public bool PadToPowerOf2 { get; set; } = true;
        public bool ZOrder { get; set; } = true;
        public int TileSize { get; set; } = 8;

        /// <summary>
        /// This is currently a hack
        /// </summary>
        //public void SetFormat<T>(T originalFormat) where T : struct, IConvertible
        //{
        //    Format = ConvertFormat(originalFormat);
        //}

        public static Format ConvertFormat<T>(T originalFormat) where T : struct, IConvertible
        {
            return (Format)Enum.Parse(typeof(Format), originalFormat.ToString());
        }
    }

    public class Common
    {
        public static byte GetBitDepth(Format format)
        {
            switch (format)
            {
                case Format.RGBA8888:
                    return 32;
                case Format.RGB888:
                    return 24;
                case Format.RGBA5551:
                case Format.RGB565:
                case Format.RGBA4444:
                case Format.LA88:
                case Format.HL88:
                    return 16;
                case Format.L8:
                case Format.A8:
                case Format.LA44:
                case Format.ETC1A4:
                    return 8;
                case Format.L4:
                case Format.A4:
                case Format.ETC1:
                    return 4;
                default:
                    return 0;
            }
        }

        static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max - 1);

        static IEnumerable<Color> GetColorsFromTexture(byte[] tex, Format format)
        {
            using (var br = new BinaryReaderX(new MemoryStream(tex)))
            {
                var etc1decoder = new ETC1.Decoder();
                var dxtdecoder = new DXT.Decoder();

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
                            a = br.ReadByte();
                            b = g = r = br.ReadByte();
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
                        case Format.ETC1:
                        case Format.ETC1A4:
                            yield return etc1decoder.Get(() =>
                            {
                                var alpha = (format == Format.ETC1A4) ? br.ReadUInt64() : ulong.MaxValue;
                                return new ETC1.PixelData { Alpha = alpha, Block = br.ReadStruct<ETC1.Block>() };
                            });
                            continue;
                        case Format.DXT1:
                        case Format.DXT5:
                            yield return dxtdecoder.Get(() =>
                            {
                                DXT.format = (DXT.Format)Enum.Parse(typeof(DXT.Format), format.ToString());
                                var alpha2 = (format == Format.DXT5) ? br.ReadBytes(8) : new byte[8];
                                return new DXT.PixelData { Alpha = alpha2, Block = br.ReadBytes(8) };
                            });
                            continue;
                        case Format.L4:
                            b = g = r = br.ReadNibble() * 17;
                            break;
                        case Format.A4:
                            a = br.ReadNibble() * 17;
                            break;
                        default:
                            throw new NotSupportedException($"Unknown image format {format}");
                    }
                    yield return Color.FromArgb(a, r, g, b);
                }
            }
        }

        static IEnumerable<Point> GetPointSequence(ImageSettings settings)
        {
            int strideWidth = (settings.Width + 7) & ~7;
            int strideHeight = (settings.Height + 7) & ~7;
            if (settings.PadToPowerOf2)
            {
                strideWidth = 2 << (int)Math.Log(strideWidth - 1, 2);
                strideHeight = 2 << (int)Math.Log(strideHeight - 1, 2);
            }
            int stride = (int)settings.Orientation < 4 ? strideWidth : strideHeight;

            //stride TileSize
            var tileSize = 0;
            if (settings.ZOrder)
                tileSize = 2 << (int)(Math.Log(((settings.TileSize + 7) & ~7) - 1, 2));
            else
                tileSize = settings.TileSize;
            int powTileSize = (int)Math.Pow(tileSize, 2);

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
                    x_in = i % powTileSize % tileSize;
                    y_in = i % powTileSize / tileSize;
                }

                switch (settings.Orientation)
                {
                    case Orientation.Default:
                        yield return new Point(x_out + x_in, y_out + y_in);
                        break;
                    case Orientation.TransposeTile:
                        yield return new Point(x_out + y_in, y_out + x_in);
                        break;
                    case Orientation.Rotate90:
                        yield return new Point(y_out + y_in, stride - 1 - (x_out + x_in));
                        break;
                    case Orientation.Transpose:
                        yield return new Point(y_out + y_in, x_out + x_in);
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
            var colors = GetColorsFromTexture(tex, settings.Format);
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
                        ptr[data.Stride * y / 4 + x] = pair.Item2.ToArgb();
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

            using (var bw = new BinaryWriterX(ms))
            {
                foreach (var point in points)
                {
                    int x = Clamp(point.X, 0, bmp.Width);
                    int y = Clamp(point.Y, 0, bmp.Height);

                    var color = bmp.GetPixel(x, y);
                    //if (color.A == 0) color = default(Color); // daigasso seems to need this

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
        }
    }
}
