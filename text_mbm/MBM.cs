using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Kuriimu.Kontract;
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
                            Text = ReadString(br.ReadBytes(entry.stringSize)),
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
                            long bk = bw.BaseStream.Position;

                            bw.BaseStream.Position = offset;
                            var byteText = ConvString(Labels[count].Text);
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

        private string ReadString(byte[] input)
        {
            using (var br = new BinaryReaderX(new MemoryStream(input), ByteOrder.BigEndian))
            {
                var sjis = Encoding.GetEncoding("sjis");
                var unicode = Encoding.GetEncoding("unicode");

                string result = "";

                ushort symbol = br.ReadUInt16();
                while (symbol != 0xffff)
                {
                    br.BaseStream.Position -= 2;

                    var bytes = br.ReadBytes(2);
                    var conv = sjis.GetString(bytes);

                    if (bytes[1] == 0)
                    {
                        result += unicode.GetString(bytes.Reverse().ToArray());
                    }
                    else
                    {
                        if (conv == "\u30fb")
                        {
                            result += unicode.GetString(bytes.Reverse().ToArray());
                        }
                        else
                        {
                            if (bytes[0] < 0x81)
                            {
                                result += sjis.GetString(new byte[] { bytes[0] });
                                br.BaseStream.Position--;
                            }
                            else
                            {
                                result += conv;
                            }
                        }
                    }

                    symbol = br.ReadUInt16();
                }

                return result;
            }
        }

        private byte[] ConvString(string text)
        {
            var sjis = Encoding.GetEncoding("SJIS");
            var unicode = Encoding.Unicode;

            List<byte> result = new List<byte>();

            foreach (var letter in text)
            {
                var conv = sjis.GetBytes(new char[] { letter });

                if (conv.Length < 2)
                {
                    if (conv[0] == 0x3f)
                    {
                        result.AddRange(unicode.GetBytes(new char[] { letter }).Reverse().ToArray());
                    }
                    else
                    {
                        if (conv[0] == 0)
                            result.AddRange(new byte[] { 0, 0 });
                        else
                            result.Add(conv[0]);
                    }
                }
                else
                {
                    result.AddRange(conv);
                }
            }

            result.AddRange(new byte[] { 0xff, 0xff });

            return result.ToArray();
        }
    }
}
