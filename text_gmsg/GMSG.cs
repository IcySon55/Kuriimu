using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace text_gmsg
{
    public sealed class GMSG
    {
        public List<Label> Labels = new List<Label>();

        private static Dictionary<ushort, int> _knownCommands = new Dictionary<ushort, int>
        {
            [0] = 0,
            [1] = 0,
            [2] = 0,
            [3] = 0,
            [4] = 0,
            [5] = 0,
            [6] = 0,
            [7] = 0,
            [8] = 0,
            [9] = 0,
            [10] = 0,
            [11] = 0,
            [12] = 0,
            [13] = 0,
            [14] = 0,
            [15] = 0,
            [16] = 0,
            [17] = 1,
            [18] = 0,
            [19] = 0,
            [20] = 0,
            [21] = 1,
            [22] = 0,
            [23] = 0,
            [24] = 0,
            [25] = 0,

            [27] = 0,
            [28] = 1,
            [29] = 0,
            [30] = 0,

            [32] = 1,
            [33] = 0,
            [34] = 1,
            [35] = 0,
            [36] = 0,
            [37] = 1,
            [38] = 1,
            [39] = 0,
            [40] = 0,
            [41] = 1,
            [42] = 1,
            [43] = 0,
            [44] = 0,
            [45] = 1,
            [46] = 0,
            [47] = 1,

            [49] = 1,
            [50] = 2,
            [51] = 0,
            [52] = 1,
            [53] = 0,
            [54] = 0,
            [55] = 0,
            [56] = 0,
            [57] = 0,
            [58] = 1,
        };

        Header header;
        List<GMSGEntry> entries = new List<GMSGEntry>();

        List<ushort> unk = new List<ushort>();

        public GMSG(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                header = br.ReadStruct<Header>();

                entries = br.ReadMultiple<GMSGEntry>(header.entryCount);

                //Add Labels
                for (int i = 0; i < header.entryCount; i++)
                {
                    br.BaseStream.Position = entries[i].stringOffset;
                    Labels.Add(new Label
                    {
                        Name = "Text" + i.ToString(),
                        Text = ReadString(br.ReadBytes(entries[i].stringLength)),
                        TextID = i
                    });
                }
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                //Header
                bw.WriteStruct(header);

                //Entries + Text
                uint dataOffset = 0x10 + (uint)entries.Count() * 0x14;
                for (int i = 0; i < entries.Count; i++)
                {
                    var bytes = ConvString(Labels[i].Text);
                    entries[i].stringOffset = dataOffset;
                    dataOffset += (uint)bytes.Count();
                    entries[i].stringLength = bytes.Count();

                    //Entry
                    bw.WriteStruct(entries[i]);

                    //Text
                    var bk = bw.BaseStream.Position;
                    bw.BaseStream.Position = entries[i].stringOffset;
                    bw.Write(bytes);
                    bw.WritePadding(4);
                    bw.BaseStream.Position = bk;

                    while (dataOffset % 4 != 0) dataOffset++;
                }
            }
        }

        #region ReadString
        public string ReadString(byte[] byteString)
        {
            var utf8 = Encoding.GetEncoding("UTF-8");
            var unicode = Encoding.Unicode;
            string result = "";

            bool control = false;
            for (int i = 0; i < byteString.Count(); i++)
            {
                if (byteString[i] == 0x7f)
                {
                    result += unicode.GetString(new byte[] { 0, 0x7f });
                    if (i % 2 == 0) i++;

                    control = true;
                    continue;
                }

                if (control)
                {
                    var identB = new byte[] { byteString[i], byteString[i + 1] };
                    result += unicode.GetString(identB.Reverse().ToArray());
                    ushort identI = (ushort)(byteString[i + 1] << 8 | byteString[i]);

                    if (_knownCommands.ContainsKey(identI))
                    {
                        for (int j = 0; j < _knownCommands[identI]; j++)
                        {
                            if (j == 0 && identI == 50 && unicode.GetString(new byte[] { byteString[i + 3], byteString[i + 2] }) == "\0")
                                j--;

                            result += unicode.GetString(new byte[] { byteString[i + 3], byteString[i + 2] });
                            i += 2;
                        }
                    }
                    else
                    {
                        if (!unk.Contains(identI))
                        {
                            unk.Add(identI);
                        }
                        //throw new System.Exception(identB[0].ToString() + "   " + identB[1].ToString());
                    }

                    i++;
                    control = false;
                }
                else
                {
                    if (byteString[i] < 0x7f)
                        result += utf8.GetString(new byte[] { byteString[i] });
                    else if (byteString[i] < 0xe0)
                    {
                        result += utf8.GetString(new byte[] { byteString[i], byteString[i + 1] });
                        i++;
                    }
                    else if (byteString[i] < 0xf0)
                    {
                        result += utf8.GetString(new byte[] { byteString[i], byteString[i + 1], byteString[i + 2] });
                        i += 2;
                    }
                    else
                    {
                        result += utf8.GetString(new byte[] { byteString[i], byteString[i + 1], byteString[i + 2], byteString[i + 3] });
                        i += 3;
                    }
                }
            }

            return result;
        }
        #endregion

        #region ConvString
        public byte[] ConvString(string text)
        {
            var utf8 = Encoding.GetEncoding("UTF-8");
            var unicode = Encoding.Unicode;

            List<byte> result = new List<byte>();

            for (int i = 0; i < text.Length; i++)
            {
                var bytes = unicode.GetBytes(new char[] { text[i] });

                if (bytes.SequenceEqual(new byte[] { 0, 0x7f }))
                {
                    result.Add(0x7f);
                    if (result.Count() % 2 == 1) result.Add(0);
                    i++;

                    bytes = unicode.GetBytes(new char[] { text[i] });
                    result.AddRange(bytes.Reverse().ToArray());

                    var val = (ushort)(bytes[0] << 8 | bytes[1]);
                    for (int j = 0; j < _knownCommands[val]; j++)
                    {
                        i++;
                        result.AddRange(unicode.GetBytes(new char[] { text[i] }).Reverse().ToArray());
                        if (result[result.Count() - 2] == 0xff && result[result.Count() - 1] == 0xfd)
                        {
                            result[result.Count() - 2] = 0xda;
                            result[result.Count() - 1] = 0x6;
                        }

                        if (j == 0 && val == 50 && result[result.Count() - 2] == 0 && result[result.Count() - 1] == 0)
                            j--;
                    }
                }
                else
                {
                    result.AddRange(utf8.GetBytes(new char[] { text[i] }));
                }
            }

            return result.ToArray();
        }
        #endregion
    }
}
