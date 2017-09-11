using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kuriimu.IO;

namespace image_iobj
{
    public class IOBJ
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        List<byte[]> table1Entries = new List<byte[]>();

        public IOBJ(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                var header = br.ReadStruct<Header>();

                //Table1
                br.BaseStream.Position = header.table1Offset;
                var t1entryCount = br.ReadInt32();
                var offsets = br.ReadMultiple<int>(t1entryCount);
                for (int i = 0; i < offsets.Count; i++)
                {
                    br.BaseStream.Position = header.table1Offset + offsets[i];
                    table1Entries.Add(br.ReadBytes((i + 1 < offsets.Count - 1) ? offsets[i + 1] - offsets[i] : header.PtgtOffset - offsets[i]));
                }

                //PTGT
                br.BaseStream.Position = header.PtgtOffset;
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
