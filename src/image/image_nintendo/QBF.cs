using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Cetera.Image;
using Kontract.IO;

namespace image_nintendo.QBF
{
    public sealed class QBF
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public QBF(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                var font = new Cetera.Font.QBF(br.BaseStream);
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
