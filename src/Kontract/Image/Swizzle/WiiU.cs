using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;

namespace Kontract.Image.Swizzle
{
    public class WiiU
    {
        public class General : IImageSwizzle
        {
            byte swizzle;
            byte pipeSwizzle;
            byte bankSwizzle;
            byte tileMode;
            byte format;
            public int Width { get; }
            public int Height { get; }

            MasterSwizzle swizzle5;
            MasterSwizzle swizzle4;
            MasterSwizzle swizzle2;
            MasterSwizzle swizzle0;

            public General(byte swizzleTileMode, byte format, int width, int height)
            {
                //swizzle is only 3bit
                //tileMode is only 5bits, everything until now depends on tilemode 4
                swizzle = (byte)(swizzleTileMode >> 5);
                pipeSwizzle = (byte)(swizzle & 0x1);
                bankSwizzle = (byte)(swizzle >> 1 & 0x3);
                tileMode = (byte)(swizzleTileMode & 0x1F);
                this.format = format;

                Width = width;
                Height = height;

                swizzle5 = new MasterSwizzle(new[] { (1, 0), (2, 0), (0, 1), (0, 2), (0, 4), (4, 0), (8, 0), (16, 0), (0, 32), (32, 32), (64, 0), (0, 8), (0, 16) }, width, 128, 64);
                swizzle4 = new MasterSwizzle(new[] { (1, 0), (2, 0), (4, 0), (0, 1), (0, 2), (0, 4), (32, 0), (0, 8), (8, 8), (16, 0) }, width, 64, 16);
                swizzle2 = new MasterSwizzle(new[] { (1, 0), (2, 0), (0, 1), (4, 0), (0, 2), (0, 4), (0, 8), (8, 8), (16, 0) }, width, 32, 16);
                if (format == 15 || format == 21)
                    swizzle0 = new MasterSwizzle(new[] { (1, 0), (2, 0), (0, 1), (0, 2), (4, 0), (0, 4), (8, 0), (16, 0), (0, 8), (0, 32), (32, 32), (64, 0), (0, 16) }, width, 128, 64);
                else if (format == 17)
                    swizzle0 = new MasterSwizzle(new[] { (1, 0), (2, 0), (0, 1), (0, 2), (0, 4), (4, 0), (8, 0), (16, 0), (64, 0), (0, 32), (32, 32), (0, 8), (0, 16) }, width, 128, 64);
                else if (format == 20)
                    swizzle0 = new MasterSwizzle(new[] { (1, 0), (2, 0), (0, 1), (4, 0), (0, 2), (0, 4) }, width, 8, 8);
            }

            public Point Get(Point point)
            {
                var pointCount = point.Y * Width + point.X;

                if (tileMode == 4)
                    switch (swizzle)
                    {
                        case 0x7:
                            var initX = new[] { 96, 32, 64, 0 };
                            var initY = new[] { 0, 0, 32, 32 };

                            var newP = swizzle0.Get(
                                pointCount,
                                new Point(
                                    initX[pointCount / (Width * 64) % 4],
                                    initY[pointCount / (Width * 64) % 4]));

                            return newP;
                        case 0x6:
                            return swizzle0.Get(pointCount, new Point(0, 0));

                            initX = new[] { 96, 32, 64, 0 };
                            initY = new[] { 32, 32, 0, 0 };

                            return swizzle0.Get(
                                pointCount,
                                new Point(
                                    initX[pointCount / (Width * 64) % 4],
                                    initY[pointCount / (Width * 64) % 4]));
                        case 0x5:
                            initX = new[] { 64, 0, 96, 32 };
                            initY = new[] { 32, 32, 0, 0 };

                            return swizzle5.Get(
                                pointCount,
                                new Point(
                                    initX[pointCount / (Width * 64) % 4],
                                    initY[pointCount / (Width * 64) % 4]));
                        case 0x4:
                            initX = new[] { 16, 0, 24, 8 };
                            initY = new[] { 0, 0, 8, 8 };

                            return swizzle4.Get(
                                pointCount,
                                new Point(
                                    initX[pointCount / (Width * 16) % 4],
                                    initY[pointCount / (Width * 16) % 4]));
                        case 0x2:
                            initX = new[] { 8, 24, 0, 16 };
                            initY = new[] { 8, 8, 0, 0 };

                            return swizzle2.Get
                                (pointCount,
                                new Point(
                                    initX[pointCount / (Width * 16) % 4],
                                    initY[pointCount / (Width * 16) % 4]));
                        case 0x0:
                            if (format == 20)
                            {
                                initX = new[] { 0, 16, 8, 24 };
                                initY = new[] { 0, 0, 8, 8 };

                                return swizzle2.Get(
                                    pointCount,
                                    new Point(
                                        initX[pointCount / (Width * 16) % 4],
                                        initY[pointCount / (Width * 16) % 4]));
                            }
                            else
                            {
                                initX = new[] { 0, 64, 32, 96 };
                                initY = new[] { 0, 0, 32, 32 };

                                return swizzle0.Get(
                                    pointCount,
                                    new Point(
                                        initX[pointCount / (Width * 64) % 4],
                                        initY[pointCount / (Width * 64) % 4]));
                            }
                        default:
                            return point;
                    }
                else
                    return point;
            }
        }

        /*public class Color16BlockFormats : IImageSwizzle
        {
            public Point Get(Point point, int width, int height)
            {
                var pointCount = point.Y * width + point.X;
                return new Point(
                    pointCount % 4 + pointCount % (width * 4) / 16 * 4,
                    pointCount % 16 / 4 + pointCount / (width * 4) * 4);
            }
        }*/
    }
}
