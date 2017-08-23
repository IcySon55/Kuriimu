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
                /*var entries = new List<TextClass>();
                for (int i = 0; i < header.entryCount; i++)
                {
                    entries.Add(new TextClass
                    {
                        labelOffset = br.ReadUInt32(),
                        textOffset = br.ReadUInt32()
                    });
                    br.BaseStream.Position += 8;
                }

                //Text
                for (int i = 0; i < entries.Count; i++)
                {
                    int labelSize = (int)((entries[i].textOffset - 1) - entries[i].labelOffset);
                    var textSize = (i + 1 == entries.Count) ?
                        (int)((br.BaseStream.Length - 1) - entries[i].textOffset) :
                        (int)((entries[i + 1].labelOffset - 1) - entries[i].textOffset);

                    br.BaseStream.Position = entries[i].labelOffset;
                    var label = br.ReadString(labelSize, Encoding.UTF8);
                    br.BaseStream.Position = entries[i].textOffset;
                    var text = br.ReadString(textSize, Encoding.UTF8);

                    Labels.Add(new Label
                    {
                        Text = text,
                        Name = label,
                        TextID = i,
                        textEntry = entries[i]
                    });
                }*/
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