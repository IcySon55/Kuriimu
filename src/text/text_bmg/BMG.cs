using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Kontract;
using Kontract.IO;
using System;

namespace text_bmg
{
    public enum Enc : byte
    {
        SJIS,
        UTF16
    }

    public sealed class BMG
    {
        public List<Label> Labels = new List<Label>();

        MESGHeader header;
        List<Item> items = new List<Item>();
        bool midUsed = false;
        short midUnk1;
        const int align = 0x20;

        ByteOrder byteOrder;
        Enc usedEncoding;

        public BMG(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                br.BaseStream.Position = 8;
                if (br.ReadInt32() == br.BaseStream.Length)
                    br.ByteOrder = byteOrder = ByteOrder.LittleEndian;
                else
                    br.ByteOrder = byteOrder = ByteOrder.BigEndian;
                br.BaseStream.Position = 0;

                header = br.ReadStruct<MESGHeader>();
                br.BaseStream.Position = 0x20;

                var strs = new List<string>();
                for (int i = 0; i < header.sectionCount; i++)
                {
                    br.BaseStream.Position = (br.BaseStream.Position + align - 1) & ~(align - 1);

                    var magic = br.ReadString(4);
                    var secSize = br.ReadInt32();
                    switch (magic)
                    {
                        //Text Index Table
                        case "INF1":
                            var itemCount = br.ReadInt16();
                            var itemLength = br.ReadInt16();
                            br.BaseStream.Position += 4;

                            for (int j = 0; j < itemCount; j++)
                            {
                                items.Add(new Item { strOffset = br.ReadInt32(), attr = br.ReadBytes(itemLength - 4) });
                            }
                            break;
                        case "DAT1":
                            if (items[0].strOffset == 2)
                                usedEncoding = Enc.UTF16;
                            else if (items[0].strOffset == 1)
                                usedEncoding = Enc.SJIS;

                            using (var br2 = new BinaryReaderX(new MemoryStream(br.ReadBytes(secSize - 8))))
                            {
                                for (int j = 0; j < items.Count; j++)
                                {
                                    br2.BaseStream.Position = items[j].strOffset;
                                    var str = GetString(br2.ReadBytes((j == items.Count - 1) ?
                                        (int)(br2.BaseStream.Length - br2.BaseStream.Position) :
                                        (int)(items[j + 1].strOffset - br2.BaseStream.Position)));

                                    strs.Add(str);
                                }
                                break;
                            }
                        case "MID1":
                            midUsed = true;
                            var idCount = br.ReadInt16();
                            midUnk1 = br.ReadInt16();
                            br.BaseStream.Position += 4;

                            for (int j = 0; j < idCount; j++)
                            {
                                var id = br.ReadInt32();
                                Labels.Add(new Label
                                {
                                    Name = "Entry" + id,
                                    Text = strs[j],
                                    TextID = id
                                });
                            }
                            break;
                    }
                }

                if (Labels.Count <= 0)
                    for (int i = 0; i < strs.Count; i++)
                        Labels.Add(new Label
                        {
                            Name = "Entry" + i,
                            Text = strs[i],
                            TextID = i
                        });
            }
        }

        private string GetString(byte[] input)
        {
            using (var br = new BinaryReaderX(new MemoryStream(input)))
            {
                string res = "";
                switch (usedEncoding)
                {
                    case Enc.SJIS:
                        while (br.BaseStream.Position < br.BaseStream.Length)
                        {
                            var peek = br.PeekByte();
                            if (peek == 0x1a)
                            {
                                br.BaseStream.Position++;
                                var size = br.ReadByte();
                                res += "{";
                                res += br.ReadByte() + ",";
                                for (int i = 0; i < size - 3; i++)
                                    res += br.ReadByte() + ((i + 1 == size - 3) ? "" : ",");
                                res += "}";
                            }
                            else
                            {
                                if (peek == 0x00)
                                {
                                    br.ReadByte();
                                    break;
                                }
                                else if (peek < 0x80)
                                    res += Encoding.GetEncoding("SJIS").GetString(new[] { br.ReadByte() });
                                else
                                    res += Encoding.GetEncoding("SJIS").GetString(br.ReadBytes(2));
                            }
                        }
                        break;
                    case Enc.UTF16:
                        while (br.BaseStream.Position < br.BaseStream.Length)
                        {
                            var peek = br.ReadInt16();
                            br.BaseStream.Position -= 2;
                            if (peek == 0x1a)
                            {
                                br.BaseStream.Position += 2;
                                var size = br.ReadByte();
                                res += "{";
                                res += br.ReadByte() + ",";
                                for (int i = 0; i < size - 4; i += 2)
                                    res += br.ReadUInt16() + ((i + 2 == size - 4) ? "" : ",");
                                res += "}";
                            }
                            else
                                if (peek == 0x00)
                            {
                                br.ReadByte();
                                break;
                            }
                            else
                                res += Encoding.GetEncoding("UTF-16").GetString(br.ReadBytes(2));
                        }
                        break;
                    default:
                        return string.Empty;
                }

                return res;
            }
        }

        public void Save(string filename)
        {
            var parStr = new List<byte[]>();
            foreach (var label in Labels)
                parStr.Add(ParseString(label.Text));

            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename), byteOrder))
            {
                bw.BaseStream.Position = align;

                //INF1
                bw.Write(0x31464e49);
                bw.Write((0x10 + items.Count * (0x4 + items[0].attr.Length) + align - 1) & ~(align - 1));
                bw.Write((short)items.Count);
                bw.Write((short)(0x4 + items[0].attr.Length));
                bw.BaseStream.Position += 4;

                var strOffset = items[0].strOffset;
                for (int i = 0; i < items.Count; i++)
                {
                    bw.Write(strOffset);
                    bw.Write(items[i].attr);
                    strOffset += parStr[i].Length;
                }

                bw.WriteAlignment(align);

                //DAT1
                bw.Write(0x31544144);
                bw.Write((0x8 + items[0].strOffset + parStr.Aggregate(0, (o, i) => o += i.Length) + align - 1) & ~(align - 1));
                bw.Write(new byte[items[0].strOffset]);

                foreach (var str in parStr)
                    bw.Write(str);

                bw.WriteAlignment(align);

                //MID1
                if (midUsed)
                {
                    bw.Write(0x3144494d);
                    bw.Write((0x10 + Labels.Count * 0x4 + align - 1) & ~(align - 1));
                    bw.Write((short)Labels.Count);
                    bw.Write(midUnk1);
                    bw.BaseStream.Position += 4;

                    foreach (var label in Labels)
                        bw.Write(label.TextID);

                    bw.WriteAlignment(align);
                }

                //Header
                bw.BaseStream.Position = 0;
                header.fileSize = (int)bw.BaseStream.Length;
                bw.WriteStruct(header);
            }
        }

        private byte[] ParseString(string input)
        {
            if (input.Count(e => e == '{') != input.Count(e => e == '}'))
                throw new InvalidOperationException("Not all code parts are correctly opened or closed.");

            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, true, byteOrder))
                switch (usedEncoding)
                {
                    case Enc.UTF16:
                        for (int i = 0; i < input.Length; i++)
                        {
                            if (input[i] == '{')
                            {
                                i++;
                                var paraStr = "";
                                while (input[i] != '}')
                                {
                                    paraStr += input[i];
                                    i++;
                                }
                                var paras = paraStr.Split(',');

                                bw.Write((short)0x1a);
                                bw.Write((byte)(2 + 1 + 1 + (paras.Count() - 1) * 2));
                                bw.Write(Convert.ToByte(paras[0]));
                                for (int j = 1; j < paras.Count(); j++)
                                    bw.Write(Convert.ToInt16(paras[j]));
                            }
                            else
                            {
                                bw.Write(Encoding.GetEncoding("UTF-16").GetBytes(input[i].ToString()));
                            }
                        }
                        bw.Write((short)0);
                        break;
                }

            return ms.ToArray();
        }
    }
}
