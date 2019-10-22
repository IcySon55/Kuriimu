using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Kontract.IO;

namespace tt.text_ttbin
{
    public sealed class TTBIN
    {
        public List<Label> Labels = new List<Label>();

        public Header cfgHeader;
        public List<CfgEntry> entries;

        public TTBIN(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Header
                cfgHeader = br.ReadStruct<Header>();

                //Entries
                entries = new List<CfgEntry>();
                for (int i = 0; i < cfgHeader.entryCount; i++)
                    entries.Add(new CfgEntry(br.BaseStream));

                //Texts
                int textCount = 0;
                bool newPage = false;
                foreach (var entry in entries)
                    foreach (var meta in entry.metaInfo)
                    {
                        if (meta.type == 1 && (int)meta.value == 0)
                            newPage = true;
                        if (meta.type == 0)
                        {
                            br.BaseStream.Position = cfgHeader.dataOffset + (int)meta.value;
                            var label = new Label
                            {
                                Name = $"Text{textCount:0000}",
                                TextID = textCount++,
                                TextOffset = (int)br.BaseStream.Position,
                                Text = ((int)meta.value < 0) ? "" : ((newPage) ? "<NP>" : "") + br.ReadCStringSJIS()
                            };
                            Labels.Add(label);
                            entry.label = label;
                            newPage = false;
                        }
                    }
            }
        }

        public void Save(string filename)
        {
            var sjis = Encoding.GetEncoding("SJIS");

            //Update TextOffsets
            int textOffset = 0;
            int labelCount = 0;
            int pageCount = 0;
            for (int j = 0; j < entries.Count; j++)
            {
                if (labelCount < Labels.Count && Labels[labelCount].Text.Length >= 4 && Labels[labelCount].Text.Substring(0, 4) == "<NP>")
                {
                    pageCount = 0;
                }
                for (int i = 0; i < entries[j].metaInfo.Count; i++)
                {
                    var meta = entries[j].metaInfo[i];
                    if (meta.type == 1 && i == 1 && entries[j].metaInfo.Count(m => m.type == 0) > 0)
                    {
                        entries[j].metaInfo[i].value = pageCount++;
                    }
                    if (meta.type == 0)
                    {
                        entries[j].metaInfo[i].value = (Labels[labelCount].Text == String.Empty) ? -1 : textOffset;
                        textOffset += (Labels[labelCount].Text == String.Empty) ? 0 : sjis.GetByteCount(Labels[labelCount].Text.Replace("<NP>", "").TrimEnd('\n')) + 1;
                        labelCount++;
                    }
                }
            }

            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                //Write Texts
                bw.BaseStream.Position = cfgHeader.dataOffset;
                foreach (var label in Labels)
                    if (label.Text != String.Empty)
                        bw.Write(sjis.GetBytes(label.Text.Replace("<NP>", "").TrimEnd('\n') + "\0"));
                cfgHeader.dataLength = (uint)bw.BaseStream.Position - cfgHeader.dataOffset;
                bw.WriteAlignment(16, 0xff);

                //Write Entries
                bw.BaseStream.Position = 0x10;
                foreach (var entry in entries)
                    entry.Write(bw.BaseStream);
                bw.WriteAlignment(16, 0xff);

                //Write Header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(cfgHeader);
            }
        }
    }
}
