using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Kuriimu.IO;
using Cetera.Hash;

namespace image_xi.ANMC
{
    class ANMC
    {
        public static Dictionary<string, uint> hashName = new Dictionary<string, uint>();

        public static Bitmap Load(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                var header = br.ReadStruct<Header>();

                //HashName Dictionary
                br.BaseStream.Position = header.stringOffset;
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var name = br.ReadCStringA();
                    try { hashName.Add(name, Crc32.Create(name)); } catch { }
                }
                br.BaseStream.Position = 0x14;

                //Tables
                var tmp = br.ReadStruct<Entry>();
                br.BaseStream.Position -= 8;
                var tables = br.ReadMultiple<Entry>((tmp.offset - 0x14) / 8);

                //First table contains as much elements as images exist in the appropriate archive
                //Files table
                var fileInfo = br.ReadMultiple<FileMeta>(tables[0].entryCount);

                //seems to part the files in sub parts for better grouping
                //Subparts
                var subParts = new List<SubPart>();
                for (int i = 0; i < tables[1].entryCount; i++)
                {
                    subParts.Add(new SubPart
                    {
                        subPart = br.ReadStruct<SubPartT>(),
                        floats = br.ReadMultiple<float>(0x33).ToArray()
                    });
                }

                //Unknown table; Size == 0 until now

                //unknown table
                var unk1 = br.ReadMultiple<ulong>(tables[3].entryCount);

                //infoMeta1 - Position ingame?
                var infoMeta1 = new List<InfoMeta1>();
                for (int i = 0; i < tables[4].entryCount; i++)
                {
                    infoMeta1.Add(new InfoMeta1
                    {
                        infoMeta = br.ReadStruct<InfoMeta1T>(),
                        floats = br.ReadMultiple<float>(0xa).ToArray()
                    });
                }

                //nameHashes
                var hashes = br.ReadMultiple<uint>(tables[5].entryCount);

                //InfoMeta2
                var infoMeta2 = new List<InfoMeta2>();
                for (int i = 0; i < tables[6].entryCount; i++)
                {
                    infoMeta2.Add(new InfoMeta2
                    {
                        infoMeta = br.ReadStruct<InfoMeta2T>(),
                        floats = br.ReadMultiple<float>(0x6).ToArray()
                    });
                }

                //InfoMeta3
                var infoMeta3 = new List<InfoMeta3>();
                for (int i = 0; i < tables[6].entryCount; i++)
                {
                    infoMeta3.Add(new InfoMeta3
                    {
                        infoMeta = br.ReadStruct<InfoMeta3T>(),
                        floats = br.ReadMultiple<float>(0xa).ToArray()
                    });
                }

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
