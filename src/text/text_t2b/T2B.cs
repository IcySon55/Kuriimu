﻿using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Kuriimu.IO;

namespace text_t2b
{
    public sealed class T2B
    {
        public List<Label> Labels = new List<Label>();

        public Header header;
        public List<StringEntry> entries = new List<StringEntry>();

        private byte[] sig;
        private EncodingType encoding;

        public T2B(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Get Encoding
                br.BaseStream.Position = br.BaseStream.Length - 0xa;
                encoding = (EncodingType)br.ReadByte();
                br.BaseStream.Position = 0;

                //Header
                header = br.ReadStruct<Header>();

                //Entries
                for (int i = 0; i < header.entryCount; i++)
                {
                    br.ReadInt32();
                    var entryLength = br.ReadByte();
                    br.BaseStream.Position -= 0x5;
                    var entry = new StringEntry
                    {
                        entryTypeID = br.ReadUInt32(),
                        entryLength = br.ReadByte(),
                        typeMask = br.ReadBytes((entryLength > 10) ? 7 : 3)
                    };

                    //cultivate data
                    var mask = entry.typeMask.Reverse().ToList();
                    for (int j = 0; j < mask.Count; j++)
                    {
                        for (int count = 0; count < 8; count += 2)
                        {
                            var type = (byte)((mask[j] & (0x3 << count)) >> count);
                            if (type != 0x3)
                                entry.data.Add(new StringEntry.TypeEntry
                                {
                                    type = type,
                                    value = br.ReadUInt32()
                                });

                            if (entry.data.Count == entry.entryLength)
                                break;
                        }

                        if (entry.data.Count == entry.entryLength)
                            break;
                    }

                    entries.Add(entry);
                }

                //Text
                var id = 0;
                for (var ctry = 0; ctry < entries.Count; ctry++)
                {
                    for (var ctrx = 0; ctrx < entries[ctry].data.Count; ctrx++)
                    {
                        var d = entries[ctry].data[ctrx];

                        if (d.type == 0)
                        {
                            if (d.value != 0xffffffff)
                            {
                                int index = -1;
                                for (var i = 0; i < Labels.Count; i++)
                                {
                                    if (Labels[i].relOffset == d.value) index = i;
                                }

                                if (index == -1)
                                {
                                    br.BaseStream.Position = header.stringSecOffset + d.value;
                                    Labels.Add(new Label
                                    {
                                        Text = GetDecodedText(GetStringBytes(br.BaseStream), encoding).Replace("\\n", "\n"),
                                        TextID = id,
                                        Name = $"text{id++:0000}",
                                        relOffset = d.value
                                    });

                                    Labels[Labels.Count - 1].Points.Add(new Point
                                    {
                                        X = ctrx,
                                        Y = ctry
                                    });
                                }
                                else
                                {
                                    Labels[index].Points.Add(new Point
                                    {
                                        X = ctrx,
                                        Y = ctry
                                    });
                                }
                            }
                        }
                    }
                }

                //signature
                br.BaseStream.Position = (br.BaseStream.Position + 0xf) & ~0xf;
                sig = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
            }
        }

        public byte[] GetStringBytes(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var result = new List<byte>();
                var byteT = br.ReadByte();
                while (byteT != 0)
                {
                    result.Add(byteT);
                    byteT = br.ReadByte();
                }

                return result.ToArray();
            }
        }

        public string GetDecodedText(byte[] bytes, EncodingType encoding)
        {
            switch (encoding)
            {
                case EncodingType.SJIS:
                    return Encoding.GetEncoding("SJIS").GetString(bytes);
                case EncodingType.UTF8:
                    return Encoding.UTF8.GetString(bytes);
                default:
                    throw new System.Exception("Encoding isn't supported!");
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                //Update entries
                uint relOffset = 0;
                foreach (var label in Labels)
                {
                    foreach (var point in label.Points)
                    {
                        entries[point.Y].data[point.X].value = relOffset;
                    }

                    uint byteCount = (uint)GetEncodedText(label.Text.Replace("\n", "\\n").Replace("\xa", "\\n"), encoding).Length + 1;
                    relOffset += byteCount;
                }

                //Header
                header.stringSecSize = (int)(relOffset);
                bw.WriteStruct(header);

                //Entries
                foreach (var entry in entries)
                {
                    bw.Write(entry.entryTypeID);
                    bw.Write(entry.entryLength);
                    bw.Write(entry.typeMask);
                    foreach (var data in entry.data) bw.Write(data.value);
                }
                bw.WriteAlignment(0x10, 0xff);

                //Text
                foreach (var label in Labels)
                {
                    bw.Write(GetEncodedText(label.Text.Replace("\n", "\\n").Replace("\xa", "\\n"), encoding));
                    bw.Write((byte)0);
                }
                bw.WriteAlignment(0x10, 0xff);

                //Signature
                bw.Write(sig);
            }
        }

        public byte[] GetEncodedText(string text, EncodingType encoding)
        {
            switch (encoding)
            {
                case EncodingType.SJIS:
                    return Encoding.GetEncoding("SJIS").GetBytes(text);
                case EncodingType.UTF8:
                    return Encoding.UTF8.GetBytes(text);
                default:
                    throw new System.Exception("Encoding isn't supported!");
            }
        }
    }
}
