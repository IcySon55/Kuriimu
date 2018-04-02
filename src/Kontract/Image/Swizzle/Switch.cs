using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;

namespace Kontract.Image.Swizzle
{
    public class SwitchSwizzle : IImageSwizzle
    {
        public enum Format : byte
        {
            R8 = 0x02,
            RGB565 = 0x07,
            RG88 = 0x09,
            RGBA8888 = 0x0B,
            BC1 = 0x1A,
            BC2 = 0x1B,
            BC3 = 0x1C,
            BC4 = 0x1D,
            BC5 = 0x1E,
            BC6H = 0x1F,
            BC7 = 0x20,
            ASTC4x4 = 0x2d,
            ASTC5x4 = 0x2e,
            ASTC5x5 = 0x2f,
            ASTC6x5 = 0x30,
            ASTC6x6 = 0x31,
            ASTC8x5 = 0x32,
            ASTC8x6 = 0x33,
            ASTC8x8 = 0x34,
            ASTC10x5 = 0x35,
            ASTC10x6 = 0x36,
            ASTC10x8 = 0x37,
            ASTC10x10 = 0x38,
            ASTC12x10 = 0x39,
            ASTC12x12 = 0x3a,
            Empty = 0xff
        }

        #region BitFields & Meta
        Dictionary<Format, (int, int)> astcBlk = new Dictionary<Format, (int, int)>
        {
            [Format.ASTC4x4] = (4, 4),
            [Format.ASTC5x4] = (5, 4),
            [Format.ASTC5x5] = (5, 5),
            [Format.ASTC6x5] = (6, 5),
            [Format.ASTC6x6] = (6, 6),
            [Format.ASTC8x5] = (8, 5),
            [Format.ASTC8x6] = (8, 6),
            [Format.ASTC8x8] = (8, 8),
            [Format.ASTC10x5] = (10, 5),
            [Format.ASTC10x6] = (10, 6),
            [Format.ASTC10x8] = (10, 8),
            [Format.ASTC10x10] = (10, 10),
            [Format.ASTC12x10] = (12, 10),
            [Format.ASTC12x12] = (12, 12)
        };

        Dictionary<int, (int, int)[]> coordsBlock = new Dictionary<int, (int, int)[]>
        {
            [4] = new[] { (1, 0), (2, 0), (0, 1), (0, 2), (4, 0), (0, 4), (8, 0), (0, 8), (0, 16), (16, 0) },
            [8] = new[] { (1, 0), (2, 0), (0, 1), (0, 2), (0, 4), (4, 0), (0, 8), (0, 16), (8, 0) },
        };
        Dictionary<int, (int, int)[]> coordsRegular = new Dictionary<int, (int, int)[]>
        {
            [32] = new[] { (1, 0), (2, 0), (0, 1), (4, 0), (0, 2), (0, 4), (8, 0), (0, 8), (0, 16) },
        };

        int BlockMaxSize = 512;
        int RegularMaxSize = 128;
        #endregion

        #region Functions
        bool isBlockBased(Format format) => (format == Format.BC1 || format == Format.BC2 || format == Format.BC3 || format == Format.BC4 || format == Format.BC5 || format == Format.BC6H || format == Format.BC7);

        (int, int) PadDimensions(int Width, int Height, Format format)
        {
            switch (format)
            {
                case Format.BC1:
                case Format.BC2:
                case Format.BC3:
                case Format.BC4:
                case Format.BC5:
                case Format.BC6H:
                case Format.BC7:
                    return ((Width + 3) & ~3, (Height + 3) & ~3);
                case Format.ASTC4x4:
                case Format.ASTC5x4:
                case Format.ASTC5x5:
                case Format.ASTC6x5:
                case Format.ASTC6x6:
                case Format.ASTC8x5:
                case Format.ASTC8x6:
                case Format.ASTC8x8:
                case Format.ASTC10x5:
                case Format.ASTC10x6:
                case Format.ASTC10x8:
                case Format.ASTC10x10:
                case Format.ASTC12x10:
                case Format.ASTC12x12:
                    return (
                        (Width + astcBlk[format].Item1 - 1) / astcBlk[format].Item1,
                        (Height + astcBlk[format].Item2 - 1) / astcBlk[format].Item2);
                default:
                    return (Width, Height);
            }
        }

        (int, int)[] GetBitField(int Width, int Height, int bpp, bool isBlockBased)
        {
            List<(int, int)> bitField;
            if (isBlockBased)
            {
                bitField = (coordsBlock.ContainsKey(bpp) ? coordsBlock[bpp].ToList() : null);
                if (bitField == null) return null;
                for (int i = 32; i < Math.Min(Height, BlockMaxSize); i *= 2)
                    bitField.Add((0, i));
            }
            else
            {
                bitField = (coordsRegular.ContainsKey(bpp) ? coordsRegular[bpp].ToList() : null);
                if (bitField == null) return null;
                for (int i = 32; i < Math.Min(Height, RegularMaxSize); i *= 2)
                    bitField.Add((0, i));
            }
            return bitField.ToArray();
        }
        #endregion

        MasterSwizzle _swizzle;

        public int Width { get; }
        public int Height { get; }

        public SwitchSwizzle(int width, int height, int bpp, Format format, bool toPowerOf2 = true)
        {
            Width = (toPowerOf2) ? 2 << (int)Math.Log(width - 1, 2) : width;
            Height = (toPowerOf2) ? 2 << (int)Math.Log(height - 1, 2) : height;

            (Width, Height) = PadDimensions(Width, Height, format);

            var bitField = GetBitField(Width, Height, bpp, isBlockBased(format));
            _swizzle = (bitField == null) ? new Linear(Width, Height)._linear : new MasterSwizzle(Width, new Point(0, 0), bitField);
        }

        public Point Get(Point point)
        {
            int pointCount = point.Y * Width + point.X;
            return _swizzle.Get(pointCount);
        }


    }
}
