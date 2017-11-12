using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kontract.IO;

namespace text_zds
{
    public sealed class ZDS
    {
        public List<Label> Labels = new List<Label>();

        public PartitionHeader partHeader;
        public Header header;

        public List<uint> structure = new List<uint>();
        public List<List<byte[]>> entries = new List<List<byte[]>>();

        public ZDS(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                partHeader = br.ReadStruct<PartitionHeader>();
                header = br.ReadStruct<Header>();

                //Struct
                structure = br.ReadMultiple<uint>(header.structEntryCount);

                //Entries
                for (int i = 0; i < header.entryCount; i++)
                {
                    entries.Add(new List<byte[]>());
                    br.BaseStream.Position = header.entryOffset + i * header.entrySize;

                    for (int j = 0; j < header.structEntryCount; j++)
                    {
                        var size = (j + 1 == header.structEntryCount) ? header.entrySize - (structure[j] >> 16) : (structure[j + 1] >> 16) - (structure[j] >> 16);
                        entries[i].Add(br.ReadBytes((int)size));
                    }
                }

                //if Text exists
                var offsets = new List<uint>();
                if (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var id = 0;
                    var labelCount = 0;
                    var textCount = 0;
                    foreach (var entry in entries)
                    {
                        var count = 0;
                        foreach (var byteA in entry)
                        {
                            if (byteA.Length == 4)
                            {
                                var offset = new BinaryReaderX(new MemoryStream(byteA)).ReadUInt32();
                                if (!offsets.Contains(offset))
                                {
                                    offsets.Add(offset);
                                    br.BaseStream.Position = offset;
                                    Labels.Add(new Label
                                    {
                                        TextID = id++,
                                        Name = (count == 1) ? $"Label{labelCount++:000}" : $"Text{textCount++:000}",
                                        Text = Encoding.UTF8.GetString(GetStringBytes(br.BaseStream))
                                    });
                                }
                            }
                            count++;
                        }
                    }
                }
                /*if (header.unk3 == 0x00040002)
                {
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var size = (i + 1 == entries.Count) ? (uint)(br.BaseStream.Length - 1) - entries[i].offset1 : (entries[i + 1].offset1 - 1) - entries[i].offset1;
                        br.BaseStream.Position = entries[i].offset1;

                        Labels.Add(new Label
                        {
                            Text = br.ReadString((int)size, Encoding.UTF8),
                            TextID = i,
                            textEntry = entries[i],
                            Name = $"text{i:00000000}"
                        });
                    }
                }
                else if (header.unk3 == 0x00040001)
                {
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var labelSize = (entries[i].offset2 - 1) - entries[i].offset1;
                        br.BaseStream.Position = entries[i].offset1;
                        var label = br.ReadString((int)labelSize, Encoding.UTF8);

                        var textSize = (i + 1 == entries.Count) ? (uint)(br.BaseStream.Length - 1) - entries[i].offset2 : (entries[i + 1].offset1 - 1) - entries[i].offset2;
                        br.BaseStream.Position = entries[i].offset2;

                        Labels.Add(new Label
                        {
                            Text = br.ReadString((int)textSize, Encoding.UTF8),
                            TextID = i,
                            textEntry = entries[i],
                            Name = label
                        });
                    }
                }*/
            }
        }

        public byte[] GetStringBytes(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var result = new List<byte>();

                var val = br.ReadByte();
                while (val != 0)
                {
                    result.Add(val);
                    val = br.ReadByte();
                }

                return result.ToArray();
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.OpenWrite(filename)))
            {

            }
        }
    }
}