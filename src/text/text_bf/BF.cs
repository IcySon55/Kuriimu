using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using text_bmd;
using Kontract;
using Kontract.IO;
using text_bmd.msg1;

namespace text_bf
{
    public sealed class BF //Implementation of classes
    {
        public List<Label> Labels = new List<Label>();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public Magic magic;
            public int fileSize; //without null stub entries
            public int MSG1Header;
            public int entryCount;
            public int entryOffset;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class TypeTable
        {
            public int textSecOffset;
            public short ttSectionsNumber;
            public List<TableEntry> entries;
        }

        public class TableEntry
        {
            public short ttSectionID;
            public int ttSectionPos;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BFEntry
        {
            public int ID;
            public int entryID;
            public int stringSize;
            public int stringOffset;
            public int padding;
        } //End of implementation of classes

        public Header header;
        public List<BFEntry> entries;

        public BF(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Type Table Types
                br.BaseStream.Position = 0x58;
                var textSecSize = br.ReadUInt32();
                br.BaseStream.Position = 0x5C;
                var textSecOffset = br.ReadUInt32();

                br.BaseStream.Position = textSecOffset;
                byte[] msg1Part = br.ReadBytes((int)textSecSize);
                var msg1 = new MSG1(new MemoryStream(msg1Part));
                for (int i = 0; i < msg1.Labels.Count; i++)
                {
                    Labels.Add(new Label
                    {
                        Name = msg1.Labels[i].Name,
                        Text = msg1.Labels[i].Text,
                        TextID = (int)msg1.Labels[i].TextID
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
                            long bk = bw.BaseStream.Position;

                            bw.BaseStream.Position = offset;
                            var byteText = "";//ConvString(Labels[count].Text);
                            bw.Write(byteText);

                            bw.BaseStream.Position = bk;

                            bw.Write(i);
                            bw.Write(byteText.Count());
                            bw.Write(offset);
                            bw.Write(0);

                            offset += byteText.Count();

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
    }
}
