using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Kontract;
using Kontract.IO;

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
        List<byte[]> items = new List<byte[]>();
        const int align = 0x20;

        ByteOrder byteOrder;
        Enc usedEncoding;

        public BMG(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                br.BaseStream.Position = 8;
                if (br.ReadInt32() == br.BaseStream.Length)
                    br.ByteOrder = byteOrder = ByteOrder.LittleEndian;
                else
                    br.ByteOrder = byteOrder = ByteOrder.BigEndian;
                br.BaseStream.Position = 0;

                header = br.ReadStruct<MESGHeader>();
                br.BaseStream.Position = 0x20;

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
                                items.Add(br.ReadBytes(itemLength));
                            }
                            break;
                        case "DAT1":
                            if (GetInt32(items[0]) == 2)
                                usedEncoding = Enc.UTF16;
                            else if (GetInt32(items[0]) == 1)
                                usedEncoding = Enc.SJIS;

                            using (var br2 = new BinaryReaderX(new MemoryStream(br.ReadBytes(secSize))))
                            {
                                for (int j = 0; j < items.Count; j++)
                                {
                                    br2.BaseStream.Position = items[j].Take(4).Aggregate(0u, (o, e) => o = (o << 8) + e);
                                    var str = GetString(br2.ReadBytes((j == items.Count - 1) ?
                                        (int)(br2.BaseStream.Length - br2.BaseStream.Position) :
                                        (int)(items[j + 1].Take(4).Aggregate(0, (o, e) => o = (o << 8) + e) - br2.BaseStream.Position)));

                                    Labels.Add(new Label
                                    {
                                        Name = "Entry" + (j + 1),
                                        Text = str,
                                        TextID = j
                                    });
                                }
                                break;
                            }
                        case "MID1":
                            break;
                    }
                }
            }
        }

        private int GetInt32(byte[] input)
        {
            switch (byteOrder)
            {
                case ByteOrder.LittleEndian:
                    return input.Take(4).Aggregate(0, (o, e) => o = (o >> 8) + (e << 24));
                case ByteOrder.BigEndian:
                    return input.Take(4).Aggregate(0, (o, e) => o = (o << 8) + e);
                default:
                    return 0;
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
                                if (peek != 0x00)
                            {
                                br.ReadByte();
                                break;
                            }
                            else
                                res += Encoding.GetEncoding("UTF16").GetString(br.ReadBytes(2));
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
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {

            }
        }
    }
}
