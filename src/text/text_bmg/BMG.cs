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
    public sealed class BMG
    {
        public List<Label> Labels = new List<Label>();

        MESGHeader header;
        List<Item> items = new List<Item>();
        private readonly bool midUsed = false;
        private readonly short midUnk1;

        // Constants
        private const int align = 0x1F;
        private static readonly int headerSize = Marshal.SizeOf<MESGHeader>();

        private readonly ByteOrder byteOrder;

        private Encoding encoding;

        public BMG(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                header = br.ReadStruct<MESGHeader>();
                if (header.fileSize == br.BaseStream.Length)
                    br.ByteOrder = byteOrder = ByteOrder.LittleEndian;
                else
                {
                    br.ByteOrder = byteOrder = ByteOrder.BigEndian;
                    br.BaseStream.Position = 0;
                    header = br.ReadStruct<MESGHeader>(); // Reload the header now that the byte order is set.
                }

                br.BaseStream.Position = headerSize;

                switch (header.encoding)
                {
                    case BmgEncoding.ASCII:
                        encoding = Encoding.ASCII;
                        break;

                    case BmgEncoding.ShiftJIS:
                        encoding = Encoding.GetEncoding("shift-jis");
                        break;

                    case BmgEncoding.UTF16:
                        encoding = byteOrder == ByteOrder.BigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode;
                        break;

                    default:
                        encoding = Encoding.Unicode;
                        break;
                }

                // INF1 (Info) section must be loaded first to properly calculate the size of each DAT1 (data) entry.
                // I'm fairly certain the specification guarantees that the INF1 section will immediately follow the BMG header,
                // but better safe than sorry. It won't cause performance issues for files that follow the specification.

                do
                {
                    // Since sections are 32-byte aligned, we can just search in 32 byte increments.
                    if (br.ReadString(4) != "INF1")
                    {
                        br.BaseStream.Position += 0x1C;
                    }
                    else
                    {
                        break;
                    }
                } while (br.BaseStream.Position < br.BaseStream.Length);

                // Parse the INF1 items.
                var infSectionSize = br.ReadUInt32();
                var itemCount = br.ReadUInt16();
                var itemSize = br.ReadUInt16();
                br.BaseStream.Position += 4;

                for (var i = 0; i < itemCount; i++)
                {
                    items.Add(new Item { strOffset = br.ReadInt32(), attr = br.ReadBytes(itemSize - 4) });
                }

                // Reset the stream position to immediately after the header.
                br.BaseStream.Position = headerSize;

                var strs = new List<string>();
                for (var i = 0; i < header.sectionCount - 1; i++)
                {
                    br.BaseStream.Position = (br.BaseStream.Position + align) & ~align;

                    var magic = br.ReadString(4);
                    var secSize = br.ReadInt32();
                    switch (magic)
                    {
                        case "DAT1":
                            using (var br2 = new BinaryReaderX(new MemoryStream(br.ReadBytes(secSize - 8))))
                            {
                                for (var j = 0; j < items.Count; j++)
                                {
                                    br2.BaseStream.Position = items[j].strOffset;
                                    var currentItem = items[j];
                                    Item nextItem = null;

                                    // Skip any 0 offset entries.
                                    var idx = j + 1;
                                    while (idx++ < items.Count)
                                    {
                                        nextItem = items[j + 1];

                                        if (nextItem.strOffset != 0) break;
                                        nextItem = null;
                                    }

                                    var size = (nextItem?.strOffset ?? (int) br.BaseStream.Length) - currentItem.strOffset;
                                    strs.Add(GetString(br2.ReadBytes(size)));
                                }
                            }

                            break;

                        case "MID1":
                            midUsed = true;
                            var idCount = br.ReadInt16();
                            midUnk1 = br.ReadInt16();
                            br.BaseStream.Position += 4;

                            for (var j = 0; j < idCount; j++)
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
                {
                    for (int i = 0; i < strs.Count; i++)
                    {
                        Labels.Add(new Label
                        {
                            Name = "Entry" + i,
                            Text = strs[i],
                            TextID = i
                        });
                    }
                }
            }
        }

        // TODO: Not all games use a standard encoding scheme. Example: Doubutsu no Mori e+.
        // TODO: It also uses custom binary data characters (0x7F for ControlCodes & 0x80 for MessageTags.)
        // TODO: Should this be supported somehow? I'm pretty certain those formats are unique to those game(s).
        // In the meantime I'll be disabling command parsing all together until a solution is determined.

        // It's also worth mentioning that not all BMG files have a 0x00 string-end character.
        // How should that be handled?

        private string GetString(byte[] input)
        {
            using (var br = new BinaryReaderX(new MemoryStream(input)))
            {
                string res = "";
                switch (header.encoding)
                {
                    case BmgEncoding.ASCII:
                        while (br.BaseStream.Position < br.BaseStream.Length)
                        {
                            /*var peek = br.PeekByte();
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

                                res += encoding.GetString(new[] { br.ReadByte() });
                            }*/

                            res += encoding.GetString(new[] { br.ReadByte() });
                        }

                        break;

                    case BmgEncoding.ShiftJIS:
                        while (br.BaseStream.Position < br.BaseStream.Length)
                        {
                            var peek = br.PeekByte();
                            /*if (peek == 0x1a)
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

                                if (peek < 0x80)
                                    res += encoding.GetString(new[] { br.ReadByte() });
                                else
                                    res += encoding.GetString(br.ReadBytes(2));
                            }*/

                            if (peek < 0x80)
                                res += encoding.GetString(new[] { br.ReadByte() });
                            else
                                res += encoding.GetString(br.ReadBytes(2));
                        }

                        break;

                    case BmgEncoding.UTF16:
                        while (br.BaseStream.Position < br.BaseStream.Length)
                        {
                            /*var peek = br.ReadInt16();
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
                                res += encoding.GetString(br.ReadBytes(2));*/

                            res += encoding.GetString(br.ReadBytes(2));
                        }

                        break;

                    default:
                        return string.Empty;
                }

                return res.Replace("\0", "");
            }
        }

        public void Save(string filename)
        {
            var parStr = new List<byte[]>();
            foreach (var label in Labels)
                parStr.Add(ParseString(label.Text));

            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename), byteOrder))
            {
                bw.BaseStream.Position = headerSize;

                //INF1
                bw.Write(Encoding.ASCII.GetBytes("INF1"));
                bw.Write((0x10 + items.Count * (0x4 + items[0].attr.Length) + align) & ~align);
                bw.Write((short) items.Count);
                bw.Write((short) (0x4 + items[0].attr.Length));
                bw.BaseStream.Position += 4;

                var strOffset = items[0].strOffset;
                for (int i = 0; i < items.Count; i++)
                {
                    bw.Write(strOffset);
                    bw.Write(items[i].attr);
                    strOffset += parStr[i].Length;
                }

                bw.WriteAlignment(align + 1);

                //DAT1
                bw.Write(Encoding.ASCII.GetBytes("DAT1"));
                bw.Write((0x8 + items[0].strOffset + parStr.Aggregate(0, (o, i) => o += i.Length) + align) & ~align);
                bw.Write(new byte[items[0].strOffset]);

                foreach (var str in parStr)
                    bw.Write(str);

                bw.WriteAlignment(align + 1);

                //MID1
                if (midUsed)
                {
                    bw.Write(Encoding.ASCII.GetBytes("MID1"));
                    bw.Write((0x10 + Labels.Count * 0x4 + align) & ~align);
                    bw.Write((short) Labels.Count);
                    bw.Write(midUnk1);
                    bw.BaseStream.Position += 4;

                    foreach (var label in Labels)
                        bw.Write(label.TextID);

                    bw.WriteAlignment(align + 1);
                }

                //Header
                bw.BaseStream.Position = 0;
                header.fileSize = (int) bw.BaseStream.Length;
                bw.WriteStruct(header);
            }
        }

        private byte[] ParseString(string input)
        {
            /*if (input.Count(e => e == '{') != input.Count(e => e == '}'))
                throw new InvalidOperationException("Not all code parts are correctly opened or closed.");*/

            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, true, byteOrder))
                switch (header.encoding)
                {
                    case BmgEncoding.ASCII:
                    case BmgEncoding.ShiftJIS:
                        for (int i = 0; i < input.Length; i++)
                        {
                            /*if (input[i] == '{')
                            {
                                i++;
                                var paraStr = "";
                                while (input[i] != '}')
                                {
                                    paraStr += input[i];
                                    i++;
                                }
                                var paras = paraStr.Split(',');

                                bw.Write((byte)0x1a);
                                bw.Write((byte)(1 + 1 + 1 + (paras.Length - 1)));
                                bw.Write(Convert.ToByte(paras[0]));
                                for (int j = 1; j < paras.Length; j++)
                                    bw.Write(Convert.ToByte(paras[j]));
                            }
                            else*/
                            {
                                bw.Write(encoding.GetBytes(input[i].ToString()));
                            }
                        }

                        bw.Write((byte)0);
                        break;

                    case BmgEncoding.UTF16:
                        for (int i = 0; i < input.Length; i++)
                        {
                            /*if (input[i] == '{')
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
                                bw.Write((byte)(2 + 1 + 1 + (paras.Length - 1) * 2));
                                bw.Write(Convert.ToByte(paras[0]));
                                for (int j = 1; j < paras.Length; j++)
                                    bw.Write(Convert.ToInt16(paras[j]));
                            }
                            else*/
                            {
                                bw.Write(encoding.GetBytes(input[i].ToString()));
                            }
                        }

                        bw.Write((short)0);
                        break;
                }

            return ms.ToArray();
        }
    }
}
