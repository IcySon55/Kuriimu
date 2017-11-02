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
            byte swizzleTileMode;
            public int Width { get; set; }
            public int Height { get; set; }

            MasterSwizzle swizzle84;
            MasterSwizzle swizzle44;
            MasterSwizzle swizzle04;

            public General(byte swizzleTileMode, int width, int height)
            {
                this.swizzleTileMode = swizzleTileMode;

                Width = width;
                Height = height;

                swizzle84 = new MasterSwizzle(new[] { (1, 0), (2, 0), (4, 0), (0, 1), (0, 2), (0, 4), (32, 0), (0, 8), (8, 8), (16, 0) }, width, 64, 16);
                swizzle44 = new MasterSwizzle(new[] { (1, 0), (2, 0), (0, 1), (4, 0), (0, 2), (0, 4), (0, 8), (8, 8), (16, 0) }, width, 32, 16);
                swizzle04 = new MasterSwizzle(new[] { (1, 0), (2, 0), (0, 1), (0, 2), (4, 0), (0, 4), (8, 0), (16, 0), (0, 8), (0, 32), (32, 32), (64, 0), (0, 16) }, width, 128, 64);
            }

            public Point Get(Point point)
            {
                var pointCount = point.Y * Width + point.X;
                switch (swizzleTileMode)
                {
                    case 0x84:
                        var initX = new[] { 16, 0, 24, 8 };
                        var initY = new[] { 0, 0, 8, 8 };

                        return swizzle84.Get(
                            pointCount,
                            new Point(
                                initX[pointCount / (Width * 16) % 4],
                                initY[pointCount / (Width * 16) % 4]));
                    case 0x44:
                        initX = new[] { 8, 24, 0, 16 };
                        initY = new[] { 8, 8, 0, 0 };

                        return swizzle44.Get
                            (pointCount,
                            new Point(
                                initX[pointCount / (Width * 16) % 4],
                                initY[pointCount / (Width * 16) % 4]));
                    case 0x04:
                        initX = new[] { 0, 64, 32, 96 };
                        initY = new[] { 0, 0, 32, 32 };

                        return swizzle04.Get(
                            pointCount,
                            new Point(
                                initX[pointCount / (Width * 64) % 4],
                                initY[pointCount / (Width * 64) % 4]));
                    default:
                        return point;
                }
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
