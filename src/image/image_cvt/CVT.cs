using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_cvt
{
   public class CVT
    {
        public Header header;

        public List<Bitmap> bmps = new List<Bitmap>();
        
        public CVT(Stream input)
        {            
            using (var br = new BinaryReaderX(input))
            {
                header = br.ReadStruct<Header>();

                var settings = new ImageSettings
                {
                    Width = header.width,
                    Height = header.height,
                    Format = ImageSettings.ConvertFormat(Format.ETC1A4),
                    PadToPowerOf2 = false
                };

                bmps.Add(Common.Load(br.ReadBytes((int)br.BaseStream.Length - 0x50), settings));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                             
                bw.WriteStruct(header);
                
                var settings = new ImageSettings
                {
                    Width = header.width,
                    Height = header.height,
                    Format = ImageSettings.ConvertFormat(header.format)
                };
                
                bw.Write(Common.Save(bmps[0], settings));
            }
        }
   }
}
