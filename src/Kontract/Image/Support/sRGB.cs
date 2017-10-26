using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kontract.Image.Support
{
    public class sRGB
    {
        public class sRGBComponent
        {

        }

        public class XYZComponent
        {
            public double X;
            public double Y;
            public double Z;
        }
        public class RGBComponent
        {
            public int R;
            public int G;
            public int B;
        }

        #region ToXYZ
        static public XYZComponent RGBToXYZ(RGBComponent rgb)
        {
            var r = PivotRgb(rgb.R / 255.0);
            var g = PivotRgb(rgb.G / 255.0);
            var b = PivotRgb(rgb.B / 255.0);

            // (Observer = 2°, Illuminant = D65)
            return new XYZComponent
            {
                X = r * 0.4124 + g * 0.3576 + b * 0.1805,
                Y = r * 0.2126 + g * 0.7152 + b * 0.0722,
                Z = r * 0.0193 + g * 0.1192 + b * 0.9505,
            };
        }

        static double PivotRgb(double c) => (c > 0.04045 ? Math.Pow((c + 0.055) / 1.055, 2.4) : c / 12.92) * 100.0;
        #endregion

        #region ToRGB
        static public RGBComponent XYZToRGB(XYZComponent xyz)
        {
            // (Observer = 2°, Illuminant = D65)
            var x = xyz.X / 100.0;
            var y = xyz.Y / 100.0;
            var z = xyz.Z / 100.0;

            var r = TovipRgb(x * 3.2406 + y * -1.5372 + z * -0.4986);
            var g = TovipRgb(x * -0.9689 + y * 1.8758 + z * 0.0415);
            var b = TovipRgb(x * 0.0557 + y * -0.2040 + z * 1.0570);

            return new RGBComponent
            {
                R = ToRGB(r),
                G = ToRGB(g),
                B = ToRGB(b),
            };
        }

        static double TovipRgb(double c) => c > 0.0031308 ? 1.055 * Math.Pow(c, 1 / 2.4) - 0.055 : 12.92 * c;

        static int ToRGB(double n)
        {
            var res = n * 255.0;
            return (res < 0) ? 0 : (res > 255) ? 255 : (int)Math.Round(res);
        }
        #endregion
    }
}