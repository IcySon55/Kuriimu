using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Cetera.Image;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_ctpk
{
    public sealed class CTPK
    {
        public List<CTPKFileInfo> Files = new List<CTPKFileInfo>();

        public CTPK(String filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                //Header
                Header header = br.ReadStruct<Header>();

                //TexEntries
                List<Entry> entries = new List<Entry>();
                entries.AddRange(br.ReadMultiple<Entry>(header.texCount));

                //TexInfo List
                List<int> texInfoList1 = new List<int>();
                texInfoList1.AddRange(br.ReadMultiple<int>(header.texCount));

                //Name List
                List<String> nameList = new List<String>();
                for (int i = 0; i < entries.Count; i++)
                    nameList.Add(br.ReadCStringA());

                //Hash List
                br.BaseStream.Position = header.hashSecOffset;
                List<int> hashList = new List<int>();
                hashList.AddRange(br.ReadMultiple<int>(header.texCount));

                //TexInfo List 2
                br.BaseStream.Position = header.texInfoOffset;
                List<int> texInfoList2 = new List<int>();
                texInfoList2.AddRange(br.ReadMultiple<int>(header.texCount));

                //Get FileData
                for (int i = 0; i < header.texCount; i++)
                    Files.Add(new CTPKFileInfo()
                    {
                        State = ArchiveFileState.Archived,
                        FileName = nameList[i],
                        FileData = new SubStream(br.BaseStream, entries[i].texOffset, entries[i].texDataSize),
                        Entry = entries[i]
                    });
            }
        }
    }
}
