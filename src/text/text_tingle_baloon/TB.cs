using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.IO;

namespace text_tb
{
    public sealed class TB
    {
        public List<Label> Labels = new List<Label>();

        public Header header;
        public List<Block> blocks = new List<Block>();

        public TB(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Tables
                for (int i = 0; i < header.tableInfoCount; i++) blocks.Add(new Block());

                br.BaseStream.Position = 0x10;
                foreach (var block in blocks) block.info = br.ReadStruct<TableInfo>();

                //Only search through block[0]
                var stringCount = 0;
                bool ident = true;
                foreach (var block in blocks)
                {
                    if (ident)
                    {
                        br.BaseStream.Position = block.info.tableOffset;
                        block.entries = br.ReadMultiple<TableEntry>((block.info.textOffset - block.info.tableOffset) / 0x8);

                        foreach (var str in block.entries)
                        {
                            if (str.length != 0)
                                Labels.Add(new Label
                                {
                                    Name = $"text{stringCount:000000}",
                                    Text = TBSupport.GetString(br.BaseStream, str.length),
                                    TextID = stringCount++
                                });
                        }

                        ident = false;
                    }
                    else
                    {
                        br.BaseStream.Position = block.info.tableOffset;
                        block.block = br.ReadBytes(block.info.blockLength);
                    }
                }
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                //Header
                bw.WriteStruct(header);

                //get texts (they will all be written in block[0] later)
                List<byte> bytes = new List<byte>();
                var count = 0;
                foreach (var label in Labels)
                {
                    var res = TBSupport.GetBytes(label.Text);
                    bytes.AddRange(res);

                    //update tableEntry
                    blocks[0].entries[count++].length = (short)res.Length;
                }

                //Add blocks + update tableInfo
                bw.BaseStream.Position = 0x80;
                int diff = 0;
                bool ident = true;
                foreach (var block in blocks)
                {
                    if (ident)
                    {
                        //tableEntries
                        foreach (var tableEntry in block.entries) bw.WriteStruct(tableEntry);

                        //strings
                        bw.Write(bytes.ToArray());
                        bw.WriteAlignment();

                        //calculate diff
                        int curOff = (int)bw.BaseStream.Position;
                        int prevOff = 0x80 + block.info.blockLength;
                        diff = curOff - prevOff;

                        //update tableInfo
                        block.info.blockLength = (int)bw.BaseStream.Position - 0x80;

                        ident = false;
                    }
                    else
                    {
                        //Write unused block
                        bw.Write(block.block);

                        //update tableInfo
                        block.info.tableOffset += diff;
                        block.info.textOffset += diff;
                    }
                }

                //Write tableInfo
                bw.BaseStream.Position = 0x10;
                foreach (var block in blocks) bw.WriteStruct(block.info);
            }
        }
    }
}
