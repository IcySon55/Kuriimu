using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Kontract.Image;
using Kontract.Image.Support;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;
using System.IO;
using System.Drawing;

namespace KuriimuTests
{
    [TestClass]
    public class ImageTest
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        public void XYZsRGBConversion()
        {
            var r = 1;
            var g = 0;
            var b = 0;

            var xyz = sRGB.RGBToXYZ(new sRGB.RGBComponent { R = r, G = g, B = b });
            var color = sRGB.XYZToRGB(xyz);

            Assert.IsTrue(r == color.R && g == color.G && b == color.B);
        }

        [TestMethod]
        public void RGBXYZConversion()
        {
            var r = 1;
            var g = 0;
            var b = 0;

            var xyz = sRGB.RGBToXYZ(new sRGB.RGBComponent { R = r, G = g, B = b });
            var color = sRGB.XYZToRGB(xyz);

            Assert.IsTrue(r == color.R && g == color.G && b == color.B);
        }

        [TestMethod]
        public void RGBA8888_ZOrder()
        {
            var settings = new ImageSettings
            {
                Width = 4,
                Height = 4,
                Format = new RGBA(8, 8, 8, 8),
                Swizzle = new CTRSwizzle(4, 4)
            };
            var tex = new byte[] {
                255,0xff,0xff,0xff,
                255,0xff,0xff,0xff,
                255,0xff,0xff,0xff,
                255,0xff,0xff,0xff,
                255,0x30,0x30,0x30,
                255,0x30,0x30,0x30,
                255,0x30,0x30,0x30,
                255,0x30,0x30,0x30,
                255,0x80,0x80,0x80,
                255,0x80,0x80,0x80,
                255,0x80,0x80,0x80,
                255,0x80,0x80,0x80,
                255,0x00,0x00,0x00,
                255,0x00,0x00,0x00,
                255,0x00,0x00,0x00,
                255,0x00,0x00,0x00,};
            var bmp = Kontract.Image.Image.Load(tex, settings);
        }
    }
}
