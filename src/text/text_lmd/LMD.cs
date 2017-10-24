using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kontract.IO;

namespace text_lmd
{
    public sealed class LMD
    {
        public List<Label> Labels = new List<Label>();

        public Header header;
        public List<Sec1Entry> sec1entries = new List<Sec1Entry>();
        public List<byte[]> sec2entries = new List<byte[]>();
        public string internFilename;

        public LMD(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Sec1Entries
                br.BaseStream.Position = header.entryOffset;
                sec1entries = br.ReadMultiple<Sec1Entry>((int)header.entryCount);

                //Sec2Entries
                br.BaseStream.Position = header.sec2Offset;
                for (int i = 0; i < sec1entries.Count; i++)
                {
                    br.BaseStream.Position = header.sec2Offset + sec1entries[i].offset;
                    var size = (i + 1 == sec1entries.Count) ? (ushort)header.sec2Size - sec1entries[i].offset : sec1entries[i + 1].offset - sec1entries[i].offset;
                    sec2entries.Add(br.ReadBytes(size));
                }

                //textEntries
                br.BaseStream.Position = header.textOffset;
                var textEntries = br.ReadMultiple<TextClass>((int)header.textCount);

                //Texts
                for (int i = 0; i < header.textCount; i++)
                {
                    br.BaseStream.Position = textEntries[i].offset;
                    Labels.Add(new Label
                    {
                        Text = br.ReadString((int)(textEntries[i].size1 * 2), Encoding.GetEncoding("UTF-16")),
                        Name = $"text{i:00000000}",
                        TextID = i,
                        offset = textEntries[i].offset
                    });
                }

                //Filename
                br.BaseStream.Position = header.fileNameOffset;
                internFilename = br.ReadCStringA();
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                //Sec1
                bw.BaseStream.Position = 0x24;
                foreach (var sec1entry in sec1entries) bw.WriteStruct(sec1entry);

                //Sec2
                foreach (var sec2entry in sec2entries) bw.Write(sec2entry);
                var textEntryOffset = bw.BaseStream.Position;

                //texts
                bw.BaseStream.Position += Labels.Count * 0xc;
                foreach (var text in Labels)
                {
                    text.offset = (uint)bw.BaseStream.Position;
                    bw.Write(Encoding.GetEncoding("UTF-16").GetBytes(text.Text));
                    bw.Write((ushort)0);
                    bw.WriteAlignment(4);
                }
                header.fileNameOffset = (uint)bw.BaseStream.Position;

                //TextEntries
                bw.BaseStream.Position = textEntryOffset;
                foreach (var text in Labels)
                {
                    bw.WriteStruct(text.offset);
                    var size = Encoding.GetEncoding("UTF-16").GetByteCount(text.Text) / 2;
                    bw.Write(size);
                    bw.Write(size);
                }

                //Filename
                bw.BaseStream.Position = header.fileNameOffset;
                bw.Write(Encoding.ASCII.GetBytes(internFilename));
                bw.Write((byte)0);

                //Header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }
    }
}