using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.IO;
using Kontract;

namespace text_fmg
{
    public class FMG
    {
        public List<Label> Labels = new List<Label>();

        private Header header;
        private byte[] unkSec;
        private int[] textOffsets;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Entry
        {
        }


        public FMG(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Header
                header = br.ReadStruct<Header>();

                //unknown section
                br.BaseStream.Position = 0x20;
                unkSec = br.ReadBytes(header.offsetSecOffset - 0x20);

                //text Offsets
                textOffsets = br.ReadMultiple<int>(header.offsetCount).ToArray();

                //Texts
                var count = 0;
                foreach (var offset in textOffsets)
                    if (offset != 0)
                    {
                        br.BaseStream.Position = offset;
                        Labels.Add(new Label
                        {
                            Text = br.ReadCStringW(),
                            TextID = count,
                            Name = "Entry" + count++
                        });
                    }
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                var textOffset = header.offsetSecOffset + header.offsetCount * 0x4;

                //Write Text and update Offsets
                bw.BaseStream.Position = textOffset;
                var labelCount = 0;
                for (int i = 0; i < textOffsets.Count(); i++)
                {
                    if (textOffsets[i] != 0)
                    {
                        textOffsets[i] = (int)bw.BaseStream.Position;
                        bw.Write(Encoding.GetEncoding("UTF-16").GetBytes(Labels[labelCount++].Text));
                        bw.WritePadding(2);
                    }
                }
                bw.WriteAlignment(0x20);

                //Write textOffsets
                bw.BaseStream.Position = header.offsetSecOffset;
                foreach (var offset in textOffsets)
                    bw.Write(offset);

                //Write unk section
                bw.BaseStream.Position = 0x20;
                bw.Write(unkSec);

                //Header
                bw.BaseStream.Position = 0x0;
                bw.WriteStruct(header);
            }
        }
    }
}
