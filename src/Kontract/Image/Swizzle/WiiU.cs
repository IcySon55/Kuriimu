using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;

namespace Kontract.Image.Swizzle
{
    public class WiiU : IImageSwizzle
    {
        byte swizzleTileMode;

        public WiiU(byte swizzleTileMode)
        {
            this.swizzleTileMode = swizzleTileMode;
        }

        public Point Get(Point point, int width, int height)
        {
            var pointCount = point.Y * width + point.X;
            switch (swizzleTileMode)
            {
                case 0x84:
                    //Get Y
                    int n = pointCount / width;
                    int s = n ^ (pointCount * 2) ^ pointCount;
                    var y = (n & -2) | (s >> 2 & 1);

                    //Get X
                    var x = 0;
                    switch (pointCount % (width * 8) / (width * 2))
                    {
                        case 0:
                            x = (pointCount % 2 == 0) ?
                                width / 2 - (pointCount % (width * 2) / 8 + 1) * 2 + pointCount % width / 4 :
                                width - (pointCount % (width * 2) / 8 + 1) * 2 + pointCount % width / 4;
                            break;
                        case 1:
                            x = (pointCount % 2 == 0) ?
                                0 + (pointCount % (width * 2) / 8) * 2 + pointCount % width / 4 :
                                width / 2 + (pointCount % (width * 2) / 8) * 2 + pointCount % width / 4;
                            break;
                        case 2:
                            x = (pointCount % 2 == 0) ?
                                width / 2 - (pointCount % (width * 2) / 8 + 1) * 2 + (7 - pointCount % width) / 4 :
                                width - (pointCount % (width * 2) / 8 + 1) * 2 + (7 - pointCount % width) / 4;
                            break;
                        case 3:
                            x = (pointCount % 2 == 0) ?
                                0 + (pointCount % (width * 2) / 8) * 2 + (7 - pointCount % width) / 4 :
                                width / 2 + (pointCount % (width * 2) / 8) * 2 + (7 - pointCount % width) / 4;
                            break;
                    }

                    return new Point(
                        x,
                        y);
                default:
                    return point;
            }
        }
    }
}
