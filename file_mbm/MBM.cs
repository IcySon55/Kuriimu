using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Kuriimu.IO;
using Kuriimu.Contract;

namespace file_mbm
{
    public sealed class MBM
    {
        public List<Label> Labels = new List<Label>();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public int zero0;
            public Magic magic;
            public int const0;
            public int fileSize; //Not always correct?
            public int entryCount;
            public int entryOffset;
            public int zero1;
            public int zero2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MBMEntry
        {
            public int ID;
            public int stringSize;
            public int stringOffset;
            public int padding;
        }

        public Header header;
        public List<MBMEntry> entries;

        public MBM(String filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                header = br.ReadStruct<Header>();

                entries=new List<MBMEntry>();
                entries.AddRange(br.ReadMultiple<MBMEntry>(header.entryCount).OrderBy(e => e.ID));

                Encoding sjis = Encoding.GetEncoding("SJIS");

                foreach (var entry in entries)
                {
                    br.BaseStream.Position = entry.stringOffset;
                    if (entry.stringOffset!=0)
                        Labels.Add(new Label
                        {
                            Name = "Text " + (entry.ID + 1),
                            Text = sjis.GetString(br.ReadBytes(entry.stringSize)).Split('')[0],
                            TextID = entry.ID
                        });
                }
            }
        }

        public void Save(String filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                Encoding sjis = Encoding.GetEncoding("SJIS");

                bw.WriteStruct(header);

                int count = 0;
                int offset = 0x20 + header.entryCount * 0x10;
                for (int i = 0; i < header.entryCount; i++)
                {
                    if (Labels[count].TextID == i)
                    {
                        count++;
                        bw.Write(i);
                        int stringSize = sjis.GetBytes(Labels[count].Text).Length;
                        bw.Write(stringSize);
                        bw.Write(offset);

                        long bk = bw.BaseStream.Position;
                        bw.BaseStream.Position = offset;
                        bw.Write(sjis.GetBytes(Labels[i].Text));
                        bw.Write((ushort)0xffff);
                        bw.BaseStream.Position = bk;

                        offset += stringSize;
                        bw.Write(0);
                    }
                    else
                    {
                        bw.Write(0);
                        bw.Write(0);
                        bw.Write(0);
                        bw.Write(0);
                    }
                }
            }
        }
    }
}
