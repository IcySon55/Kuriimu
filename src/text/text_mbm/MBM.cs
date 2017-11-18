using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Kontract;
using Kontract.IO;

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
        public class MBMEntry
        {
            public int ID;
            public int stringSize;
            public int stringOffset;
            public int padding;
        }

        public Header header;
        public List<MBMEntry> entries;
        private const int headerLength = 0x20;
        

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
                    entries.Add(entry);
                    if (entry.stringOffset != 0)
                        count++;
                }

                
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

        private byte[] ConvertStringToBytes(string input)
        {
            var sjis = Encoding.GetEncoding("sjis");

            return Regex.Split(input, "<([0-9A-F]{2})([0-9A-F]{2})>").SelectMany((s, i) => i % 3 == 0 ? sjis.GetBytes(s) : new[] { Convert.ToByte(s, 16) }).ToArray();
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                int startOffset = entries[0].stringOffset;
                bw.BaseStream.Position = startOffset;

                var entryIndex = 0;
                foreach (var lbl in Labels)
                {
                    while (entries[entryIndex].stringOffset == 0 && entryIndex < entries.Count)
                        entryIndex++;

                    var bytes = ConvertStringToBytes(lbl.Text);
                    entries[entryIndex].stringSize = bytes.Length + 2;
                    entries[entryIndex].stringOffset = (int)bw.BaseStream.Position;
                    bw.Write(bytes);
                    bw.Write((byte)0xFF);
                    bw.Write((byte)0xFF);
                    entryIndex++;
                } 

                //Update info
                header.fileSize = (int)bw.BaseStream.Length - (startOffset - 0x20 - header.entryCount * 0x10);
                bw.BaseStream.Position = 0x0;
                bw.WriteStruct(header);
                bw.BaseStream.Position = headerLength;
                foreach (var entry in entries)
                {
                    bw.WriteStruct(entry);
                }

            }
        } 

        private string ReadString(byte[] input)
        {
            using (var br = new BinaryReaderX(new MemoryStream(input), ByteOrder.BigEndian))
            {
                var sjis = Encoding.GetEncoding("sjis");
                                
                string result = "";
                var bytes = br.ReadBytes(2);
                while (bytes[0] != 0xFF && bytes[1] != 0xFF)
                {
                    var sjisbytes = sjis.GetBytes(sjis.GetString(bytes));
                    var initialString = bytes[0].ToString("X2") + bytes[1].ToString("X2");
                    var convString = sjis.GetString(bytes);
                    if (!bytes.SequenceEqual(sjisbytes))
                    {
                        result += $"<{initialString}>";
                    }
                    else
                    {
                        result += convString;
                    }
                    bytes = br.ReadBytes(2);
                }
                return result;
            }
        }
    }
}
