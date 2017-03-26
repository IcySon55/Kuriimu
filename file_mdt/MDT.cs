using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;
using System.IO;

namespace file_mdt
{
    public sealed class MDT
    {
        public List<Label> Labels = new List<Label>();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public Magic magic;
            public int unk1;
            public int unk2;
            public int entryCount;
            public int headerSize;
            public int dataOffset;
        }

        public Header header;

        public MDT(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                header = br.ReadStruct<Header>();

                br.BaseStream.Position = header.headerSize;

                int[] offsetList = new int[header.entryCount];
                for (int i = 0; i < header.entryCount; i++) offsetList[i] = br.ReadInt32();

                br.BaseStream.Position = header.dataOffset;

                for (int i = 0; i < header.entryCount; i++)
                {
                    Label label = new Label();
                    ushort part = br.ReadUInt16();
                    String result = "";
                    do
                    {
                        result += (char)(part ^ 0xff73);
                        part = br.ReadUInt16();
                    } while (part != 0xff73);

                    label.Name = "MDTEntry" + i.ToString();
                    label.Text = result;
                    label.TextID = i;

                    Labels.Add(label);
                }
            }
        }

        public void Save(String filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Write)))
            {
                List<List<ushort>> chars = new List<List<ushort>>();
                for (int i = 0; i < Labels.Count; i++)
                {
                    List<ushort> list = new List<ushort>();
                    for (int j = 0; j < Labels[i].Text.Length; j++) list.Add(Labels[i].Text[j]);
                    chars.Add(list);
                }

                //Header
                bw.WriteASCII("MSGD");
                bw.Write(header.unk1);
                bw.Write(header.unk2);
                bw.Write(Labels.Count);
                bw.Write(0x20);

                //write offsetlist
                bw.BaseStream.Position = 0x20; int offset = 0;
                for (int i = 0; i < chars.Count; i++)
                {
                    bw.Write(offset);
                    offset += chars[i].Count * 2 + 2;
                }

                //pad to next 0x10
                while (bw.BaseStream.Position % 16 != 0) bw.BaseStream.Position += 1;

                //write dataOffset
                long bk = bw.BaseStream.Position;
                bw.BaseStream.Position = 0x14;
                bw.Write((int)bk);
                bw.BaseStream.Position = bk;

                //write obfuscated text
                for (int i = 0; i < chars.Count; i++)
                {
                    for (int j = 0; j < chars[i].Count; j++) bw.Write((ushort)(chars[i][j] ^ 0xff73));
                    bw.Write((ushort)0xff73);
                }
            }
        }
    }
}
