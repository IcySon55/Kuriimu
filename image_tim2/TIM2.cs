using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.IO;
using Cetera.Image;
using System.IO;
using System.Drawing;

namespace image_tim2
{
    public sealed class TIM2
    {
        Bitmap bmp;

        public TIM2(string filename)
        {
            using (var br=new BinaryReaderX(File.OpenRead(filename),true))
            {
                //Header
                var header = br.ReadStruct<Header>();
                while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;

                Bitmap[] bmps=new Bitmap[header.nrOfPlanes];
                for (int i=0;i<header.nrOfPlanes;i++)
                {
                    var entry = br.ReadStruct<Entry>();
                    while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;


                }
            }
        }
    }
}
