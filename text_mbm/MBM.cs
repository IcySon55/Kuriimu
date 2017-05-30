using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace text_mbm
{
    public sealed class MBM
    {
        public List<Label> Labels = new List<Label>();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public int zero0;
            public Magic magic;
            public int const0;
            public int fileSize; //without null stub entries
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

        public MBM(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                header = br.ReadStruct<Header>();

                entries = new List<MBMEntry>();
                var count = 0;
                while (count < header.entryCount)
                {
                    var entry = br.ReadStruct<MBMEntry>();
                    if (entry.stringOffset != 0)
                    {
                        entries.Add(entry);
                        count++;
                    }
                }
                entries = entries.OrderBy(e => e.ID).ToList();

                Encoding sjis = Encoding.GetEncoding("SJIS");

                foreach (var entry in entries)
                {
                    br.BaseStream.Position = entry.stringOffset;
                    if (entry.stringOffset != 0)
                        Labels.Add(new Label
                        {
                            Name = "Text " + (entry.ID + 1),
                            Text = sjis.GetString(br.ReadBytes(entry.stringSize)).Split('')[0],
                            TextID = entry.ID
                        });
                }
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                Encoding sjis = Encoding.GetEncoding("SJIS");

                bw.BaseStream.Position = 0x20;

                int count = 0;
                int entryCount = Labels.OrderBy(e => e.TextID).Last().TextID + 1;
                int offset = 0x20 + entryCount * 0x10;
                for (int i = 0; i < entryCount; i++)
                {
                    if (count < Labels.Count)
                        if (Labels[count].TextID == i)
                        {
                            bw.Write(i);
                            int stringSize = sjis.GetBytes(Labels[count].Text).Length + 2;
                            bw.Write(stringSize);
                            bw.Write(offset);

                            long bk = bw.BaseStream.Position;
                            bw.BaseStream.Position = offset;
                            var byteText = ReplaceBytes(sjis.GetBytes(Labels[count].Text), new byte[] { 0x81, 0x45 }, new byte[] { 0xf8, 0x01 });
                            bw.Write(byteText);
                            bw.Write((ushort)0xffff);
                            bw.BaseStream.Position = bk;

                            offset += stringSize;
                            bw.Write(0);

                            count++;
                        }
                        else
                        {
                            bw.Write(0);
                            bw.Write(0);
                            bw.Write(0);
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

                //update header
                header.fileSize = (int)bw.BaseStream.Length - (entryCount * 0x10 - Labels.Count() * 0x10);
                header.entryCount = Labels.Count();
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }

        private byte[] ReplaceBytes(byte[] input, byte[] search, byte[] replace)
        {
            if (search.Length != replace.Length) return null;

            for (int i = 0; i < input.Length - search.Length + 1; i++)
            {
                var found = true;
                for (int j = 0; j < search.Length; j++)
                {
                    if (input[i + j] != search[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    for (int j = 0; j < replace.Length; j++)
                    {
                        input[i + j] = replace[j];
                    }
                }
            }

            return input;
        }
    }
}
