using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_999
{
    public class A999
    {
        public List<A999FileInfo> Files=new List<A999FileInfo>();

        List<uint> unk1=new List<uint>();
        List<unkEntry> unkEntries=new List<unkEntry>();
        List<uint> XORs=new List<uint>();

        private uint headXORpad=0xFABACEDA;

        public A999(Stream input)
        {
            using (var br0 = new BinaryReaderX(input, true))
            {
                var tmpHeader = A999Support.deXOR(br0.BaseStream, headXORpad, 0x20);
                var tmpBr = new BinaryReaderX(new MemoryStream(tmpHeader));
                tmpBr.BaseStream.Position = 0x14;
                var inputTopSize = tmpBr.ReadInt64();

                br0.BaseStream.Position = 0;
                var inputTop = A999Support.deXOR(br0.BaseStream, headXORpad, inputTopSize);

                using (var br = new BinaryReaderX(new MemoryStream(inputTop)))
                {
                    //Header
                    var header = br.ReadStruct<Header>();

                    //unknown
                    var unkTop = br.ReadStruct<TableHeader>();
                    unk1 = br.ReadMultiple<uint>(unkTop.entryCount);
                    while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;
                    unkEntries = br.ReadMultiple<unkEntry>(unkTop.entryCount);

                    //FileEntries
                    var entryTop = br.ReadStruct<TableHeader>();
                    XORs = br.ReadMultiple<uint>(entryTop.entryCount);
                    while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;

                    //Files
                    for (int i = 0; i < entryTop.entryCount; i++)
                    {
                        int t;
                        if (i > 0xcdc1)
                            t = 0;
                        var entry = br.ReadStruct<Entry>();
                            Files.Add(new A999FileInfo
                            {
                                Entry = entry,
                                XORpad = XORs[i],
                                FileName = $"File {i:00000}.unk",
                                State = ArchiveFileState.Archived,
                                FileData = new SubStream(br0.BaseStream, header.dataOffset + entry.offset, entry.size)
                            });
                    }
                }
            }
        }
    }
}
