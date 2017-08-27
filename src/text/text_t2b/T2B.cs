using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using Kuriimu.IO;
using Kuriimu.Compression;
using System.Linq;
using System.Text;

namespace text_t2b
{
    public sealed class T2B
    {
        public List<Label> Labels = new List<Label>();

        public Header header;
        public List<StringEntry> entries = new List<StringEntry>();
        public List<uint> offsets = new List<uint>();
        byte[] sig;

        public T2B(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
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
                foreach (var entry in entries)
                {
                    foreach (var data in entry.data)
                    {
                        if (data.type == 0)
                        {
                            if (data.value != 0xffffffff)
                            {
                                if (!offsets.Contains(data.value))
                                {
                                    br.BaseStream.Position = header.stringSecOffset + data.value;
                                    Labels.Add(new Label
                                    {
                                        Text = Encoding.UTF8.GetString(GetStringBytes(br.BaseStream)).Replace("\\n", "\n"),
                                        TextID = id,
                                        Name = $"text{id++:0000}",
                                        relOffset = data.value
                                    });
                                    offsets.Add(data.value);
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

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                var utf8 = Encoding.UTF8;

                //Update entries
                uint relOffset = 0;
                var count = 1;
                foreach (var label in Labels)
                {
                    uint byteCount = (uint)utf8.GetByteCount(label.Text.Replace("\n", "\\n").Replace("\xa", "\\n")) + 1;

                    if (count < offsets.Count)
                    {
                        foreach (var entry in entries)
                            foreach (var data in entry.data)
                                if (data.type == 0 && data.value == offsets[count]) data.value = relOffset + byteCount;
                    }

                    relOffset += byteCount;
                    count++;
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
                    bw.Write(utf8.GetBytes(label.Text.Replace("\n", "\\n").Replace("\xa", "\\n")));
                    bw.Write((byte)0);
                }
                bw.WriteAlignment(0x10, 0xff);

                //Signature
                bw.Write(sig);
            }
        }
    }
}
