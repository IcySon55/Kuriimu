using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kuriimu.IO;

namespace text_zds
{
    public sealed class ZDS
    {
        public List<Label> Labels = new List<Label>();

        public PartitionHeader partHeader;
        public Header header;

        public ZDS(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                partHeader = br.ReadStruct<PartitionHeader>();
                header = br.ReadStruct<Header>();

                //Entries
                var entries = br.ReadMultiple<TextClass>((int)header.entryCount);

                //Text
                if (header.unk3 == 0x00040002)
                {
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var size = (i + 1 == entries.Count) ? (uint)(br.BaseStream.Length - 1) - entries[i].offset1 : (entries[i + 1].offset1 - 1) - entries[i].offset1;
                        br.BaseStream.Position = entries[i].offset1;

                        Labels.Add(new Label
                        {
                            Text = br.ReadString((int)size, Encoding.UTF8),
                            TextID = i,
                            textEntry = entries[i],
                            Name = $"text{i:00000000}"
                        });
                    }
                }
                else if (header.unk3 == 0x00040001)
                {
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var labelSize = (entries[i].offset2 - 1) - entries[i].offset1;
                        br.BaseStream.Position = entries[i].offset1;
                        var label = br.ReadString((int)labelSize, Encoding.UTF8);

                        var textSize = (i + 1 == entries.Count) ? (uint)(br.BaseStream.Length - 1) - entries[i].offset2 : (entries[i + 1].offset1 - 1) - entries[i].offset2;
                        br.BaseStream.Position = entries[i].offset2;

                        Labels.Add(new Label
                        {
                            Text = br.ReadString((int)textSize, Encoding.UTF8),
                            TextID = i,
                            textEntry = entries[i],
                            Name = label
                        });
                    }
                }
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.OpenWrite(filename)))
            {

            }
        }
    }
}