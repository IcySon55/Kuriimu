using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Kuriimu.IO;

namespace image_xi.ANMC
{
    class ANMC
    {
        public static Bitmap Load(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                var header = br.ReadStruct<Header>();

                //Tables
                var tmp = br.ReadStruct<Entry>();
                br.BaseStream.Position -= 8;
                var tables = br.ReadMultiple<Entry>((tmp.offset - 0x14) / 8);

                //First table contains as much elements as images exist in the appropriate archive
                //Files table
                var fileInfo = br.ReadMultiple<FileMeta>(tables[0].entryCount);

                //seems to part the files in sub parts for better grouping
                //Subparts
                var subParts = br.ReadMultiple<SubPart>(tables[1].entryCount);

                //Unknown table; Size == 0 until now

                //unknown table
                var unk1 = br.ReadMultiple<ulong>(tables[3].entryCount);

                //infoMeta1 - Position ingame?
                var inforMeta1 = br.ReadMultiple<InfoMeta1>(tables[4].entryCount);

                //nameHashes
                var hashes = br.ReadMultiple<uint>(tables[5].entryCount);

                //InfoMeta2


                return null;
            }
        }

        public static void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {

            }
        }
    }
}
