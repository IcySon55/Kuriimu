using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Kuriimu.IO;
using System.IO;
using System.Drawing.Imaging;

namespace CeteraDS.Image
{
    public enum Format : byte
    {
        ABGR1555, BGR555
    }

    public enum BitLength : byte
    {
        Bit4, Bit8
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
        public BitLength BitPerIndex { get; set; }
        public Orientation Orientation { get; set; } = Orientation.Default;
        public int TileSize { get; set; } = 8;
        public bool PadToPowerOf2 { get; set; } = false;
        public Color TransparentColor = Color.FromArgb(255, 0, 0, 0);

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
        static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max - 1);

        public static IEnumerable<Color> GetPalette(byte[] palette, Format format)
        {
            using (var br = new BinaryReaderX(new MemoryStream(palette)))
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    int a = 255, r = 255, g = 255, b = 255;
                    switch (format)
                    {
                        case Format.BGR555:
                            var c = br.ReadUInt16();
                            r = (c & 31) * 255 / 31;
                            g = (c >> 5 & 31) * 255 / 31;
                            b = (c >> 10 & 31) * 255 / 31;
                            break;
                        case Format.ABGR1555:
                            var c2 = br.ReadUInt16();
                            r = (c2 & 31) * 255 / 31;
                            g = (c2 >> 5 & 31) * 255 / 31;
                            b = (c2 >> 10 & 31) * 255 / 31;
                            a = (c2 >> 15) * 255;
                            break;
                        default:
                            throw new NotSupportedException($"Unknown image format {format}");
                    }
                    yield return Color.FromArgb(a, r, g, b);
                }
            }
        }

        static IEnumerable<Color> GetColorsFromIndeces(byte[] indeces, IEnumerable<Color> palette, ImageSettings settings)
        {
            using (var br = new BinaryReaderX(new MemoryStream(indeces)))
            {
                var count = 0;
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    switch (settings.BitPerIndex)
                    {
                        case BitLength.Bit4:
                            if (count < settings.Width)
                            {
                                yield return palette.ToList()[br.ReadNibble()];
                                yield return palette.ToList()[br.ReadNibble()];
                            }
                            else
                            {
                                br.ReadByte();
                            }
                            count += 2;
                            if (count % settings.TileSize == 0)
                                count = 0;
                            break;
                        case BitLength.Bit8:
                            if (count < settings.Width)
                            {
                                yield return palette.ToList()[br.ReadByte()];
                            }
                            else
                            {
                                br.ReadByte();
                            }
                            count++;
                            if (count % settings.TileSize == 0)
                                count = 0;
                            break;
                        default:
                            throw new NotSupportedException($"Unknown bitLength format {settings.BitPerIndex}");
                    }
                }
            }
        }

        static IEnumerable<Point> GetPointSequence(ImageSettings settings)
        {
            int tileSize = settings.TileSize;
            if ((int)settings.Orientation < 4)
            {
                if (tileSize > settings.Width) tileSize = settings.Width;
            }
            else
            {
                if (tileSize > settings.Height) tileSize = settings.Height;
            }

            int strideWidth = (settings.Width + 7) & ~7;
            int strideHeight = (settings.Height + 7) & ~7;
            if (settings.PadToPowerOf2)
            {
                strideWidth = 2 << (int)Math.Log(strideWidth - 1, 2);
                strideHeight = 2 << (int)Math.Log(strideHeight - 1, 2);
            }
            int stride = (int)settings.Orientation < 4 ? strideWidth : strideHeight;
            for (int i = 0; i < strideWidth * strideHeight; i++)
            {
                int x_out = (i / (int)Math.Pow(tileSize, 2) % (stride / tileSize)) * tileSize;
                int y_out = (i / (int)Math.Pow(tileSize, 2) / (stride / tileSize)) * tileSize;
                int x_in = i % tileSize;
                int y_in = i / tileSize % tileSize;

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

        public static Bitmap Load(byte[] indeces, ImageSettings settings, IEnumerable<Color> palette)
        {
            int width = settings.Width, height = settings.Height;
            var colors = GetColorsFromIndeces(indeces, palette, settings).ToList();
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
                        ptr[data.Stride * y / 4 + x] = (pair.Item2.Equals(settings.TransparentColor)) ? Color.Transparent.ToArgb() : pair.Item2.ToArgb();
                    }
                }
            }
            bmp.UnlockBits(data);
            return bmp;
        }

        public static List<Color> CreatePalette(Bitmap bmp)
        {
            List<Color> colours = new List<Color>();

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    if (!colours.Exists(c => c == bmp.GetPixel(x, y)))
                    {
                        colours.Add(bmp.GetPixel(x, y));
                    }
                }
            }

            return colours;
        }

        public static byte[] EncodePalette(List<Color> colours, Format format)
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, true))
            {
                foreach (var c in colours)
                    switch (format)
                    {
                        case Format.BGR555:
                            short b = 0;
                            b |= (short)(c.B * 31 / 255);
                            b = (short)(b << 5 | (c.G * 31 / 255));
                            b = (short)(b << 5 | (c.R * 31 / 255));
                            bw.Write(b);
                            break;
                        case Format.ABGR1555:
                            short b2 = (short)(c.A / 255);
                            b2 |= (short)(b2 << 5 | (c.B * 31 / 255));
                            b2 = (short)(b2 << 5 | (c.G * 31 / 255));
                            b2 = (short)(b2 << 5 | (c.R * 31 / 255));
                            bw.Write(b2);
                            break;
                        default:
                            throw new NotSupportedException($"Unknown image format {format}");
                    }
            }

            return ms.ToArray();
        }

        public static byte[] Save(Bitmap bmp, ImageSettings settings, List<Color> pal)
        {
            settings.Width = bmp.Width;
            settings.Height = bmp.Height;
            var points = GetPointSequence(settings);

            var ms = new MemoryStream();

            using (var bw = new BinaryWriterX(ms))
            {
                foreach (var point in points)
                {
                    var color = bmp.GetPixel(point.X, point.Y);

                    switch (settings.BitPerIndex)
                    {
                        case BitLength.Bit4:
                            bw.WriteNibble(pal.FindIndex(c => c == color));
                            break;
                        case BitLength.Bit8:
                            bw.Write((byte)pal.FindIndex(c => c == color));
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
