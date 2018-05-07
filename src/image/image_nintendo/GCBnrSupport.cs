using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract;
using Kontract.Interface;
using Kontract.Image.Swizzle;
using Kontract.Image.Support;
using System.Drawing;
using Kontract.IO;
using System.IO;

namespace image_nintendo
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1c)]
        public byte[] zeroPad;
    }

    class GCBnrSupport
    {
        public class GCSwizzle : IImageSwizzle
        {
            MasterSwizzle _tiler;

            public int Width { get; }
            public int Height { get; }

            public GCSwizzle(int width, int height)
            {
                Width = width;
                Height = height;

                _tiler = new MasterSwizzle(Width, new Point(0, 0), new[] { (1, 0), (2, 0), (0, 1), (0, 2) });
            }

            public Point Get(Point point) => _tiler.Get(point.Y * Width + point.X);
        }

        public class GC_5A3 : IImageFormat
        {
            public int BitDepth { get; } = 16;

            public string FormatName { get; } = "GC ABGR1555/ABGR3444";

            public IEnumerable<Color> Load(byte[] input)
            {
                using (var br = new BinaryReaderX(new MemoryStream(input), ByteOrder.BigEndian))
                {
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        var value = br.ReadUInt16();

                        if ((value & 0x8000) == 0x8000)
                        {
                            yield return Color.FromArgb(
                                255,
                                Support.ChangeBitDepth((value >> 10) & 0x1f, 5, 8),
                                Support.ChangeBitDepth((value >> 5) & 0x1f, 5, 8),
                                Support.ChangeBitDepth(value & 0x1f, 5, 8)
                                );
                        }
                        else
                        {
                            yield return Color.FromArgb(
                                Support.ChangeBitDepth((value >> 12) & 0x7, 3, 8),
                                Support.ChangeBitDepth((value >> 8) & 0xf, 4, 8),
                                Support.ChangeBitDepth((value >> 4) & 0xf, 4, 8),
                                Support.ChangeBitDepth(value & 0xf, 4, 8)
                                );
                        }
                    }
                }
            }

            public byte[] Save(IEnumerable<Color> colors)
            {
                var ms = new MemoryStream();
                using (var bw = new BinaryWriterX(ms, true, ByteOrder.BigEndian))
                    foreach (var color in colors)
                    {
                        ushort val = 0;
                        if (color.A == 255)
                        {
                            val |= 1 << 15;
                            val |= (ushort)(Support.ChangeBitDepth(color.R, 8, 5) << 10);
                            val |= (ushort)(Support.ChangeBitDepth(color.G, 8, 5) << 5);
                            val |= (ushort)Support.ChangeBitDepth(color.B, 8, 5);
                        }
                        else
                        {
                            val |= (ushort)(Support.ChangeBitDepth(color.A, 8, 3) << 12);
                            val |= (ushort)(Support.ChangeBitDepth(color.R, 8, 4) << 8);
                            val |= (ushort)(Support.ChangeBitDepth(color.G, 8, 4) << 4);
                            val |= (ushort)Support.ChangeBitDepth(color.B, 8, 4);
                        }

                        bw.Write(val);
                    }

                return ms.ToArray();
            }
        }
    }
}
