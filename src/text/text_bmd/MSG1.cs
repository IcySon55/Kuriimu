using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kontract.IO;

namespace text_bmd.msg1
{
    public sealed class MSG1
    {
        public List<Label> Labels = new List<Label>();

        Header header;
        TextHeader textHeader;

        public MSG1(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Entries
                var entries = br.ReadMultiple<EntryTableEntry>(header.entryCount);

                //TextHeader
                textHeader = br.ReadStruct<TextHeader>();

                //Labels
                uint count = 0;
                foreach (var entry in entries)
                {
                    br.BaseStream.Position = 0x20 + entry.offset;

                    var name = br.ReadString(0x18);

                    switch (entry.type)
                    {
                        case 0:
                            var stringCount = br.ReadInt16();
                            var nameIndex = br.ReadUInt16();

                            for (int i = 0; i < stringCount; i++)
                            {
                                var stringOffset = br.ReadInt32();
                                var stringSize = br.ReadInt32();
                                var startStringBlock = br.ReadBytes(6);

                                Labels.Add(new Label
                                {
                                    Name = $"{name}/text{i}",
                                    TextID = count++,
                                    Text = br.ReadCStringSJIS(),
                                    StartStringBlock = startStringBlock,
                                    nameIndex = nameIndex,
                                    type = entry.type
                                });

                                br.BaseStream.Position = (br.BaseStream.Position + 0x3) & ~0x3;
                            }
                            break;
                        case 1:
                            break;
                        default:
                            throw new Exception($"Don't support Entry Type {entry.type}");
                    }
                }
            }
        }

        public void Save(Stream input, bool leaveOpen = false)
        {
            using (var bw = new BinaryWriterX(input, leaveOpen))
            {

            }
        }
    }
}
