using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using image_xi;
using Kuriimu.IO;

namespace image_xf
{
    public class XF
    {
        public Bitmap bmp;

        List<FileEntry> fileEntries;

        public XF(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //parse Header
                var header = br.ReadStruct<XpckHeader>();
                header.CorrectHeader();

                //parse FileEntries
                fileEntries = new List<FileEntry>();
                for (int i = 0; i < header.fileEntries; i++)
                {
                    fileEntries.Add(br.ReadStruct<FileEntry>());
                }

                //get xi image
                BinaryWriterX xi = new BinaryWriterX(new MemoryStream());
                br.BaseStream.Position = header.dataOffset + fileEntries[0].offset;
                xi.Write(br.ReadBytes(fileEntries[0].fileSize));
                xi.BaseStream.Position = 0;

                //convert xi image to bmp
                bmp = XI.Load(xi.BaseStream);
            }
        }
    }
}
