using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using Kuriimu.IO;
using Kuriimu.Compression;

namespace text_xs
{
    public sealed class XS
    {
        public List<Label> Labels = new List<Label>();

        public Header header;
        public List<T2Entry> entries = new List<T2Entry>();

        public XS(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Table1
                //skip

                //Table2
                br.BaseStream.Position = header.table2Offset << 2;
                using (var table2 = new BinaryReaderX(new MemoryStream(Level5.Decompress(br.BaseStream))))
                {
                    while (table2.BaseStream.Position < table2.BaseStream.Length)
                        entries.Add(table2.ReadStruct<T2Entry>());
                }
                br.BaseStream.Position = (br.BaseStream.Position + 3) & ~3;

                //Text
                if (br.BaseStream.Position < br.BaseStream.Length)
                    using (var text = new BinaryReaderX(new MemoryStream(Level5.Decompress(br.BaseStream))))
                    {
                        var count = 0;
                        foreach (var entry in entries)
                        {
                            if (entry.ident == 0x18)
                            {
                                text.BaseStream.Position = entry.textOffset;
                                Labels.Add(new Label
                                {
                                    Name = $"text{count:000000}",
                                    TextID = count++,
                                    Text = text.ReadCStringSJIS()
                                });
                            }
                        }
                    }
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.OpenWrite(filename)))
            {

            }
        }
    }
}
