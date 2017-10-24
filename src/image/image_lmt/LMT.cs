using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kontract.Interface;
using Kontract.IO;

namespace image_lmt
{
    public class LMT
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public List<Entry> entries = new List<Entry>();

        public LMT(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Get entries
                var entryCount = br.ReadUInt64();
                entries = br.ReadMultiple<Entry>((int)entryCount);

                //get PNG's
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var pngSize = br.ReadUInt32();
                    bmps.Add((Bitmap)Image.FromStream(new MemoryStream(br.ReadBytes((int)pngSize))));
                }
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {

            }
        }
    }
}
