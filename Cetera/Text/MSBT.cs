using Cetera.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cetera.Text
{
    public class MSBT : List<MSBT.Item>
    {
        [DebuggerDisplay("{Label,nq}: {Text,nq}")]
        public class Item
        {
            public string Label { get; set; }
            public string Text { get; set; }
            public uint Hash => Label.Aggregate(0u, (n, c) => 1170 * n + c) % 101;
        }

        enum MsbtEncoding : byte { UTF8, Unicode }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class Header
        {
            public String8 magic;
            public ByteOrder byteOrder;
            private short zeroes1;
            private MsbtEncoding encoding;
            private byte alwaysEqualTo3;
            public short sectionCount;
            private short zeroes2;
            public int fileSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            private byte[] padding;

            public Encoding Encoding => encoding == MsbtEncoding.UTF8 ? Encoding.UTF8 : Encoding.Unicode;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class SectionHeader
        {
            public String4 magic;
            public int size;
            private long padding;
        }

        public class Atom
        {
            public enum Type { Char, ControlCode, EndCode };
            public Type type { get; }
            public char character { get; }
            public int id1 { get; }
            public int id2 { get; }
            public byte[] bytes { get; }

            public Atom(char c)
            {
                type = Type.Char;
                character = c;
            }

            public Atom(int id1_, int id2_, byte[] bytes_)
            {
                type = Type.ControlCode;
                id1 = id1_;
                id2 = id2_;
                bytes = bytes_;
            }

            public Atom(int id1_, int id2_)
            {
                type = Type.EndCode;
                id1 = id1_;
                id2 = id2_;
            }

            public override string ToString() => ToReadableString();

            public string ToRawString()
            {
                switch (type)
                {
                    case Type.Char:
                        return character.ToString();
                    case Type.ControlCode:
                        return $"\xE{(char)id1}{(char)id2}{(char)bytes.Length}{string.Concat(bytes.Select(b => (char)b))}";
                    case Type.EndCode:
                        return $"\xF{(char)id1}{(char)id2}";
                }
                throw new ArgumentException($"Unknown atom type {type}");
            }

            public string ToReadableString()
            {
                switch (type)
                {
                    case Type.Char:
                        return character.ToString();
                    case Type.ControlCode:
                        return $"<n{id1}.{id2}:{BitConverter.ToString(bytes)}>";
                    case Type.EndCode:
                        return $"</{id1}.{id2}>";
                }
                throw new ArgumentException($"Unknown atom type {type}");
            }
        }

        Header header;
        List<string> sections = new List<string>();
        List<int> tsy1 = null;
        byte[] atr1 = null;

        public MSBT(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                header = br.ReadStruct<Header>();
                if (header.magic != "MsgStdBn") throw new Exception("Not MSBT");

                List<Tuple<string, int>> lbl1 = null;
                List<string> txt2 = null;

                for (int i = 0; i < header.sectionCount; i++)
                {
                    var section = br.ReadStruct<SectionHeader>();
                    sections.Add(section.magic);

                    switch (section.magic)
                    {
                        case "LBL1":
                            if (br.ReadInt32() != 101) throw new InvalidDataException("Expecting hastable size of 101");
                            var labelCount = br.ReadMultiple(101, _ => (int)br.ReadInt64()).Sum();
                            lbl1 = br.ReadMultiple(labelCount, _ => Tuple.Create(Encoding.ASCII.GetString(br.ReadBytes(br.ReadByte())), br.ReadInt32()));
                            break;
                        case "ATR1":
                            atr1 = br.ReadBytes(section.size);
                            break;
                        case "TSY1":
                            tsy1 = br.ReadMultiple(section.size / 4, _ => br.ReadInt32());
                            break;
                        case "TXT2":
                            var textCount = br.ReadInt32();
                            var offsets = Enumerable.Range(0, textCount).Select(_ => br.ReadInt32()).Concat(new[] { section.size }).ToList();
                            txt2 = offsets.Skip(1).Zip(offsets, (o1, o2) => ReadMSBTString(br.ReadBytes(o1 - o2))).ToList();
                            break;
                        default:
                            throw new Exception("Unknown section");
                    }

                    while (br.BaseStream.Position % 16 != 0 && br.BaseStream.Position != br.BaseStream.Length)
                    {
                        br.ReadByte();
                    }
                }

                var labels = lbl1 == null
                           ? Enumerable.Range(0, txt2.Count).Select(i => $"Label_{i}")
                           : from z in lbl1 orderby z.Item2 select z.Item1;
                AddRange(labels.Zip(txt2, (lbl, txt) => new Item { Label = lbl, Text = txt }));
            }
        }

        string ReadMSBTString(byte[] bytes)
        {
            var sb = new StringBuilder();
            using (var br = new BinaryReader(new MemoryStream(bytes), header.Encoding))
            {
                char c;
                while ((c = br.ReadChar()) != 0)
                {
                    sb.Append(c);
                    if (c == 0xE)
                    {
                        sb.Append((char)br.ReadInt16());
                        sb.Append((char)br.ReadInt16());
                        int count = br.ReadInt16();
                        sb.Append((char)count);
                        for (int i = 0; i < count; i++)
                        {
                            sb.Append((char)br.ReadByte());
                        }
                    }
                    else if (c == 0xF)
                    {
                        sb.Append((char)br.ReadInt16());
                        sb.Append((char)br.ReadInt16());
                    }
                }
            }
            return sb.ToString();
        }

        public static IEnumerable<Atom> ToAtoms(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == 0xE)
                {
                    int len = str[i + 3];
                    yield return new Atom(str[i + 1], str[i + 2], str.Substring(i + 4, len).Select(c => (byte)c).ToArray());
                    i += 3 + len;
                }
                else if (str[i] == 0xF)
                {
                    yield return new Atom(str[i + 1], str[i + 2]);
                    i += 2;
                }
                else
                {
                    yield return new Atom(str[i]);
                }
            }
        }

        // A quick test to check that the hashes are returned in the same order as that originally stored in the MSBT file
        public IEnumerable<Item> HashTest => this.OrderBy(item => item.Hash);
    }
}
