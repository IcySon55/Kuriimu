using System.Collections.Generic;
using System.IO;
using Kontract.IO;
using Kontract.Compression;
using System.Text;

namespace text_xs
{
    public sealed class XS
    {
        public List<Label> Labels = new List<Label>();

        public Header header;
        public byte[] table1;
        public List<T2Entry> entries = new List<T2Entry>();
        public List<uint> offsets = new List<uint>();

        public XS(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Table1
                table1 = br.ReadBytes((header.table2Offset << 2) - (header.table1Offset << 2));

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
                            if (entry.ident == 0x18 && !offsets.Contains(entry.textOffset))
                            {
                                offsets.Add(entry.textOffset);
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
            //Save available after finding out the comp table Size or if changes work without this comp table Size
            var sjis = Encoding.GetEncoding("SJIS");

            using (BinaryWriterX bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                //Header
                bw.WriteStruct(header);

                //Table 1
                bw.Write(table1);

                //Table2
                uint relOffset = 0;
                var count = 1;
                foreach (var label in Labels)
                {
                    if (count == offsets.Count)
                    {
                        var byteCount = (uint)sjis.GetByteCount(label.Text) + 1;

                        foreach (var entry in entries)
                            if (entry.ident == 0x18 && entry.textOffset == offsets[count])
                                entry.textOffset = relOffset + byteCount;

                        relOffset += byteCount;
                        count++;
                    }
                }

                var ms = new MemoryStream();
                using (var bw2 = new BinaryWriterX(ms, true))
                    foreach (var entry in entries)
                        bw2.WriteStruct(entry);
                bw.Write(Level5.Compress(ms, Level5.Method.NoCompression));
                bw.BaseStream.Position = (bw.BaseStream.Position + 0x3) & ~0x3;

                //Text
                ms = new MemoryStream();
                using (var bw2 = new BinaryWriterX(ms, true))
                    foreach (var label in Labels)
                    {
                        bw2.Write(sjis.GetBytes(label.Text));
                        bw2.Write((byte)0);
                    }
                bw.Write(Level5.Compress(ms, Level5.Method.NoCompression));
            }
        }
    }
}
