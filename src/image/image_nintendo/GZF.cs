using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Cetera.Image;
using Kontract.IO;

namespace image_nintendo.GZF
{
    public sealed class GZF
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public GZF(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                var font = new Cetera.Font.GZF(br.BaseStream);
                bmps = font.bmps;
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {

            }
        }
    }
}
