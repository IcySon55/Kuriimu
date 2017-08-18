using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kuriimu.IO;

namespace text_gmd
{
    public sealed class GMD
    {
        public List<Label> Labels = new List<Label>();

        private Header Header;
        private int HeaderLength = 0x28;
        private int EntryV1Length = 0x8;
        private int EntryV2Length = 0x14;
        private ByteOrder ByteOrder = ByteOrder.LittleEndian;
        private string Name;

        private byte[] UnknownV2;
        private List<EntryV1> EntriesV1 = new List<EntryV1>();
        private List<EntryV2> EntriesV2 = new List<EntryV2>();
        private List<String> Names = new List<String>();

        public GMD(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                // Set endianess
                if (br.PeekString() == "\0DMG")
                    br.ByteOrder = ByteOrder = ByteOrder.BigEndian;

                // Header
                Header = br.ReadStruct<Header>();
                Name = br.ReadCStringA();

                // Entries
                switch (Header.Version)
                {
                    case Version.Version1:
                        EntriesV1 = br.ReadMultiple<EntryV1>((int)Header.LabelCount);
                        break;
                    case Version.Version2:
                        EntriesV2 = br.ReadMultiple<EntryV2>((int)Header.LabelCount);
                        break;
                }

                // Unknown Version 2 Section
                if (Header.Version == Version.Version2)
                {
                    var bk = br.BaseStream.Position;
                    uint temp = br.ReadUInt32();
                    while (temp < 0x100000 || temp == 0xffffffff) temp = br.ReadUInt32();
                    br.BaseStream.Position -= 4;

                    temp = br.ReadByte();
                    while (temp == 0) temp = br.ReadByte();
                    br.BaseStream.Position--;

                    var unkSize = br.BaseStream.Position - bk;
                    br.BaseStream.Position = bk;

                    UnknownV2 = br.ReadBytes((int)unkSize);
                }

                // Labels
                for (int i = 0; i < Header.LabelCount; i++) Names.Add(br.ReadCStringA());

                // Text
                long dataOffset = 0;
                switch (Header.Version)
                {
                    case Version.Version1:
                        dataOffset = HeaderLength + Name.Length + 1 + Header.LabelCount * EntryV1Length + Header.LabelSize;
                        break;
                    case Version.Version2:
                        dataOffset = HeaderLength + Name.Length + 1 + Header.LabelCount * EntryV2Length + UnknownV2.Length + Header.LabelSize;
                        break;
                }

                byte[] text = br.ReadBytes((int)Header.SectionSize);

                using (var brt = new BinaryReaderX(new MemoryStream(text)))
                {
                    int counter = 0;
                    for (int i = 0; i < Header.SectionCount; i++)
                    {
                        var bk = brt.BaseStream.Position;
                        byte tmp = brt.ReadByte();
                        while (tmp != 0) tmp = brt.ReadByte();
                        var textSize = brt.BaseStream.Position - bk;
                        brt.BaseStream.Position = bk;

                        Labels.Add(new Label
                        {
                            Name = i < Header.LabelCount ? Names[i] : "no_name_" + counter.ToString("000"),
                            Text = brt.ReadString((int)textSize, Encoding.UTF8),
                            TextID = i
                        });

                        if (i >= Header.LabelCount)
                            counter++;
                    }
                }
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                bw.BaseStream.Position = HeaderLength + Header.NameSize + 1;

                // Section Entries
                switch (Header.Version)
                {
                    case Version.Version1:
                        for (int i = 0; i < EntriesV1.Count; i++) bw.WriteStruct(EntriesV1[i]);
                        break;
                    case Version.Version2:
                        for (int i = 0; i < EntriesV2.Count; i++) bw.WriteStruct(EntriesV2[i]);
                        break;
                }

                // Unknown Version 2 Section
                if (Header.Version == Version.Version2) bw.Write(UnknownV2);

                // Labels
                uint labelSize = 0;
                for (int i = 0; i < Header.LabelCount; i++)
                {
                    bw.WriteASCII(Names[i]);
                    bw.Write((byte)0);
                    labelSize += (uint)Names[i].Length + 1;
                }
                Header.LabelSize = labelSize;

                // Sections
                List<byte> text = new List<byte>();
                for (int i = 0; i < Labels.Count; i++)
                {
                    text.AddRange(Encoding.UTF8.GetBytes(Labels[i].Text));
                    text.Add(0);
                }
                Header.SectionSize = (uint)text.Count;
                bw.Write(text.ToArray());

                // Header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(Header);
                bw.WriteASCII(Name);
                bw.Write((byte)0);
            }
        }
    }
}