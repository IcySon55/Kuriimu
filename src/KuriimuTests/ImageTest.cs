using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Kontract.Image;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;
using System.IO;
using System.Drawing;

namespace KuriimuTests
{
    [TestClass]
    public class ImageTest
    {
        [TestMethod]
        public void RGBA8888_ZOrder()
        {
            var settings = new ImageSettings
            {
                Width = 4,
                Height = 4,
                Format = new RGBA(8, 8, 8, 8),
                TileSize = 4,
                InnerSwizzle = new ZOrder(),
                OuterSwizzle = new ZOrder()
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
