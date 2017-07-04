using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_tmx
{
    public enum TMXWrapMode : short
    {
        HorizontalRepeat = 0x0000,
        VerticalRepeat = 0x0000,
        HorizontalClamp = 0x0100,
        VerticalClamp = 0x0400,
    }

    public enum TMXPixelFormat : byte
    {
        PSMCT32 = 0x00,
        PSMCT24 = 0x01,
        PSMCT16 = 0x02,
        PSMCT16S = 0x0A,
        PSMT8 = 0x13,
        PSMT4 = 0x14,
        PSMT8H = 0x1B,
        PSMT4HL = 0x24,
        PSMT4HH = 0x2C
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public int unk1;
        public int fileSize;
        public Magic magic;
        public int unk2;
        public byte unk3;
        public TMXPixelFormat paletteFormat;
        public ushort width;
        public ushort height;
        public TMXPixelFormat imageFormat;
        public byte mipmapCount;
        public byte mipmapKValue;
        public byte mipmapLValue;
        public TMXWrapMode texWrap;
        public int texID;
        public int CLUTID;
    }

    public class TmxSupport
    {
        public static Bitmap ConvertIndexed4(BinaryReaderX br, Header header, out Color[] Palette)
        {
            Palette = SetPalette(br, header.paletteFormat, 16);

            Bitmap bmp = new Bitmap(header.width, header.height);
            for (int y = 0; y < header.height; y++)
            {
                for (int x = 0; x < header.width; x += 2)
                {
                    byte pixels = br.ReadByte();
                    bmp.SetPixel(x, y, Palette[pixels & 0x0F]);
                    bmp.SetPixel(x + 1, y, Palette[pixels >> 4]);
                }
            }

            return bmp;
        }

        public static Bitmap ConvertIndexed8(BinaryReaderX br, Header header, out Color[] Palette)
        {
            Palette = SetPalette(br, header.paletteFormat, 256);

            Bitmap bmp = new Bitmap(header.width, header.height);
            for (int y = 0; y < header.height; y++)
                for (int x = 0; x < header.width; x++)
                    bmp.SetPixel(x, y, Palette[br.ReadByte()]);

            return bmp;
        }

        public static Bitmap Convert16(BinaryReaderX br, Header header)
        {
            Bitmap bmp = new Bitmap(header.width, header.height, PixelFormat.Format32bppArgb);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
                bmp.PixelFormat);
            byte[] pixelData = new byte[bmpData.Height * bmpData.Stride];
            Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);

            for (int i = 0;
                i < (bmpData.Width * bmpData.Height) * (Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8);
                i += 4)
            {
                ushort rgba = br.ReadUInt16();
                pixelData[i + 2] = ResampleChannel((rgba >> 0), 5, 8);
                pixelData[i + 1] = ResampleChannel((rgba >> 5), 5, 8);
                pixelData[i + 0] = ResampleChannel((rgba >> 10), 5, 8);
                /* TODO: verify alpha */
                pixelData[i + 3] = ScaleAlpha(ResampleChannel((rgba >> 15), 1, 8));
            }

            Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static Bitmap Convert24(BinaryReaderX br, Header header)
        {
            Bitmap bmp = new Bitmap(header.width, header.height, PixelFormat.Format32bppArgb);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
                bmp.PixelFormat);
            byte[] pixelData = new byte[bmpData.Height * bmpData.Stride];
            Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);

            for (int i = 0;
                i < (bmpData.Width * bmpData.Height) * (Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8);
                i += 4)
            {
                pixelData[i + 2] = br.ReadByte();
                pixelData[i + 1] = br.ReadByte();
                pixelData[i + 0] = br.ReadByte();
                pixelData[i + 3] = ScaleAlpha(0x80);
            }

            Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static Bitmap Convert32(BinaryReaderX br, Header header)
        {
            Bitmap bmp = new Bitmap(header.width, header.height, PixelFormat.Format32bppArgb);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
                bmp.PixelFormat);
            byte[] pixelData = new byte[bmpData.Height * bmpData.Stride];
            Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);

            for (int i = 0;
                i < (bmpData.Width * bmpData.Height) * (Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8);
                i += 4)
            {
                pixelData[i + 2] = br.ReadByte();
                pixelData[i + 1] = br.ReadByte();
                pixelData[i + 0] = br.ReadByte();
                byte a = br.ReadByte();
                pixelData[i + 3] = ScaleAlpha(a);
            }

            Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static byte[] CreateIndexed4(Bitmap bmp, Color[] Palette)
        {
            List<byte> picData = new List<byte>();

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x += 2)
                {
                    bool found = false;
                    byte count = 0;
                    Color color = bmp.GetPixel(x, y);
                    while (!found)
                    {
                        if (color == Palette[count])
                        {
                            found = true;
                            picData.Add(count);
                        }
                        count++;
                    }

                    found = false;
                    count = 0;
                    color = bmp.GetPixel(x + 1, y);
                    while (!found)
                    {
                        if (color == Palette[count])
                        {
                            found = true;
                            picData[picData.Count - 1] |= (byte)(count << 4);
                        }
                        count++;
                    }
                }
            }

            return picData.ToArray();
        }

        public static byte[] CreateIndexed8(Bitmap bmp, Color[] Palette)
        {
            List<byte> picData = new List<byte>();

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    bool found = false;
                    byte count = 0;
                    Color color = bmp.GetPixel(x, y);
                    while (!found)
                    {
                        if (color == Palette[count])
                        {
                            found = true;
                            picData.Add(count);
                        }
                        count++;
                    }
                }
            }

            return picData.ToArray();
        }

        public static byte[] Create32(Bitmap bmp)
        {
            List<byte> pixelData = new List<byte>();

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Height; x++)
                {
                    Color color = bmp.GetPixel(x, y);
                    pixelData.Add(color.B);
                    pixelData.Add(color.G);
                    pixelData.Add(color.R);
                    pixelData.Add(color.A);
                }
            }

            return pixelData.ToArray();
        }

        private static byte ResampleChannel(int value, int sourceBits, int targetBits)
        {
            byte sourceMask = (byte)((1 << sourceBits) - 1);
            byte targetMask = (byte)((1 << targetBits) - 1);
            return (byte)((((value & sourceMask) * targetMask) + (sourceMask >> 1)) / sourceMask);
        }

        private static byte ScaleAlpha(byte a)
        {
            /* Required for indexed AND non-indexed formats */
            return (byte)Math.Min((255.0f * (a / 128.0f)), 0xFF);
        }

        private static Color[] SetPalette(BinaryReaderX br, TMXPixelFormat paletteFormat, int colorCount)
        {
            Color[] tmpPalette = new Color[colorCount];

            for (int i = 0; i < colorCount; i++)
            {
                byte r, g, b, a;
                if (paletteFormat == TMXPixelFormat.PSMCT32)
                {
                    uint color = br.ReadUInt32();
                    r = (byte)color;
                    g = (byte)(color >> 8);
                    b = (byte)(color >> 16);
                    a = (byte)(color >> 24);
                }
                else
                {
                    ushort color = br.ReadUInt16();
                    r = (byte)((color & 0x001F) << 3);
                    g = (byte)(((color & 0x03E0) >> 5) << 3);
                    b = (byte)(((color & 0x7C00) >> 10) << 3);
                    a = (byte)(i == 0 ? 0 : 0x80);
                }

                tmpPalette[i] = Color.FromArgb(a, r, g, b);
            }

            Color[] newPalette = new Color[colorCount];
            if (colorCount == 256)
            {
                for (int i = 0; i < newPalette.Length; i += 32)
                {
                    Array.Copy(tmpPalette, i, newPalette, i, 8);
                    Array.Copy(tmpPalette, i + 8, newPalette, i + 16, 8);
                    Array.Copy(tmpPalette, i + 16, newPalette, i + 8, 8);
                    Array.Copy(tmpPalette, i + 24, newPalette, i + 24, 8);
                }
            }
            else
                Array.Copy(tmpPalette, newPalette, tmpPalette.Length);

            return newPalette;
        }

        public static Color[] GetPalette(Bitmap bmp)
        {
            List<Color> colors = new List<Color>();

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    bool found = false;
                    int count = 0;
                    Color color = bmp.GetPixel(x, y);
                    while (!found && count < colors.Count)
                    {
                        if (color == colors[count])
                        {
                            found = true;
                        }
                        count++;
                    }
                    if (!found)
                        colors.Add(color);
                }
            }

            if (colors.Count < 16)
            {
                while (colors.Count < 16) colors.Add(new Color());
            }
            else if (colors.Count > 16 && colors.Count < 256)
            {
                while (colors.Count < 256) colors.Add(new Color());
            }

            return colors.ToArray();
        }

        public static byte[] GetPaletteBytes(Color[] Palette)
        {
            if (Palette.Length == 256)
            {
                for (int i = 0; i < 256; i += 32)
                {
                    Color[] tmp = new Color[8];
                    Array.Copy(Palette, i + 8, tmp, 0, 8);
                    Array.Copy(Palette, i + 16, Palette, i + 8, 8);
                    Array.Copy(tmp, 0, Palette, i + 16, 8);
                }
            }

            byte[] paletteBytes = new byte[Palette.Length * 4];
            for (int i = 0; i < Palette.Length; i++)
            {
                paletteBytes[i * 4] = Palette[i].R;
                paletteBytes[i * 4 + 1] = Palette[i].G;
                paletteBytes[i * 4 + 2] = Palette[i].B;
                paletteBytes[i * 4 + 3] = Palette[i].A;
            }

            return paletteBytes;
        }
    }
}
