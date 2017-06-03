using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Kuriimu.IO;
using Kuriimu.Contract;

namespace image_nintendo.CGFX
{
    public sealed class CGFX
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public CGFX(Stream input)
        {
            using (var br=new BinaryReaderX(input,true))
            {
                var cgfxHeader = br.ReadStruct<CgfxHeader>();

                br.BaseStream.Position = 0x24;
                var texDataEntry = br.ReadStruct<DataEntry>();

                br.BaseStream.Position += texDataEntry.offset - 4;
                var dictHeader = br.ReadStruct<DictHeader>();

                List<DictEntry> dictEntryList = new List<DictEntry>();
                for (int i=0;i<=dictHeader.entryCount;i++)
                {
                    dictEntryList.Add(br.ReadStruct<DictEntry>());
                }

                
            }
        }

        public void Save(string filename)
        {
            
        }
    }
}
