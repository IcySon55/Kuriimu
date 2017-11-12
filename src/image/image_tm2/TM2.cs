using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.IO;

namespace image_tm2
{
    public sealed class TM2
    {
        public List<Bitmap> bmpList;
        public Bitmap bmp;

        public TM2(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                int count = br.ReadInt32();
                br.BaseStream.Position = (count - 1) * 0x4;
                int off = br.ReadInt32();
                br.BaseStream.Position = off + 8;

                //get Images
                bmpList = new List<Bitmap>();
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var info = br.ReadStruct<FileHeader>();
                    br.BaseStream.Position += 0x60;
                    var png = br.ReadBytes(info.pngFileSize - 0x7c);
                    bmpList.Add(new Bitmap(Image.FromStream(new MemoryStream(png))));
                }

                bmp = bmpList[2];
            }
        }

        public void Save(Stream input)
        {

        }
    }
}
