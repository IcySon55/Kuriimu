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

                //infoMeta1
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

                //InfoMeta2 - image positioning, calculated from center of screen
                var infoMeta2 = new List<InfoMeta2>();
                for (int i = 0; i < tables[6].entryCount; i++)
                {
                    infoMeta2.Add(new InfoMeta2
                    {
                        infoMeta = br.ReadStruct<InfoMeta2T>(),
                        floats = br.ReadMultiple<float>(0x6).ToArray()
                    });
                    infoMeta2[infoMeta2.Count - 1].width = (int)(infoMeta2[infoMeta2.Count - 1].floats[3] + -1 * infoMeta2[infoMeta2.Count - 1].floats[0]);
                    infoMeta2[infoMeta2.Count - 1].height = (int)(infoMeta2[infoMeta2.Count - 1].floats[4] + -1 * infoMeta2[infoMeta2.Count - 1].floats[1]);
                }

                //InfoMeta3
                var infoMeta3 = new List<InfoMeta3>();
                for (int i = 0; i < tables[7].entryCount; i++)
                {
                    infoMeta3.Add(new InfoMeta3
                    {
                        infoMeta = br.ReadStruct<InfoMeta3T>(),
                        floats = br.ReadMultiple<float>((tables[7].entryLength - 8) / 4).ToArray()
                    });
                }

                //creating relative List
                var relList = new List<RootElement>();
                foreach (var file in fileInfo)
                    relList.Add(new RootElement
                    {
                        name = hashName.ToList().Find(x => x.Value == file.nameHash).Key,
                        fileMeta = file
                    });
                foreach (var sub in subParts)
                    relList.Find(x => x.fileMeta.nameHash == sub.subPart.refHash).subParts.Add(new SubPartElement
                    {
                        name = hashName.ToList().Find(x => x.Value == sub.subPart.nameHash).Key,
                        subPart = sub
                    });

                return null;
            }
        }

        public class RootElement
        {
            public string name;
            public FileMeta fileMeta;
            public List<SubPartElement> subParts = new List<SubPartElement>();
        }

        public class SubPartElement
        {
            public string name;
            public SubPart subPart;
        }

        public static void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {

            }
        }
    }
}
