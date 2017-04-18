using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Kuriimu.IO;

namespace file_tdt
{
    public sealed class TDT
    {
        public List<Label> Labels = new List<Label>();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public int entryCount;
            public int dataOffset;
        }

        public class TDTEntry
        {
            public TDTEntry(Stream input)
            {
                using (BinaryReaderX br = new BinaryReaderX(input, true))
                {
                    entrySize = br.ReadInt32();
                    dataOffset = br.ReadInt32();
                    nameSize = br.ReadInt32() - dataOffset;

                    name = readASCII(br.BaseStream);
                    text = readSJIS(br.BaseStream);
                }
            }
            public int entrySize;
            public int dataOffset;
            public int nameSize;
            public String name;
            public String text;
        }

        public static String readASCII(Stream input)
        {
            Encoding encode = Encoding.GetEncoding("ascii");
            String result = "";

            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                var letters = br.ReadBytes(1);
                bool nul;
                do
                {
                    nul = true;
                    result += encode.GetString(letters);
                    letters = br.ReadBytes(1);
                    for (int i = 0; i < 1; i++) if (letters[i] != 0) nul = false;
                } while (nul == false);

                return result;
            }
        }
        public static String readSJIS(Stream input)
        {
            Encoding encode = Encoding.GetEncoding("sjis");
            String result = "";

            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                var letters = br.ReadBytes(2).Reverse().ToArray();
                bool nul;
                do
                {
                    nul = true;
                    result += (letters[0] < 0x80) ? encode.GetString(new byte[] { letters[1] }) : encode.GetString(letters);
                    letters = br.ReadBytes(2).Reverse().ToArray();
                    for (int i = 0; i < 2; i++) if (letters[i] != 0) nul = false;
                } while (nul == false);

                return result;
            }
        }
        public static byte[] writeText(String input, String method)
        {
            Encoding encode = Encoding.GetEncoding(method);
            return encode.GetBytes(input);
        }

        public static Header header;
        public static TDTEntry[] entries;

        public TDT(String filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read), true))
            {
                header = br.ReadStruct<Header>();
                entries = new TDTEntry[header.entryCount];

                for (int i = 0; i < header.entryCount; i++)
                {
                    entries[i] = new TDTEntry(br.BaseStream);
                }

                int id = 0;

                foreach (TDTEntry part in entries)
                {
                    Label label = new Label()
                    {
                        Name = part.name,
                        Text = part.text,
                        TextID = id++
                    };
                    Labels.Add(label);
                }
            }
        }

        public void Save(String filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Write)))
            {
                bw.WriteStruct(header);

                for (int i = 0; i < header.entryCount; i++)
                {
                    long bk = bw.BaseStream.Length;
                    bw.BaseStream.Position += 0xc;

                    byte[] name = writeText(Labels[i].Name, "ascii");
                    bw.Write(name);
                    byte[] text = writeText(Labels[i].Text, "sjis");
                    bw.Write(text);

                    long bk2 = bw.BaseStream.Length;
                    bw.BaseStream.Position = bk;
                    bw.Write((int)(bw.BaseStream.Length - bk));
                    bw.Write(0xc);
                    bw.Write(0xc + name.Length);

                    bw.BaseStream.Position = bk2;
                }
            }
        }
    }
}
