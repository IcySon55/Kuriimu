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

        public Encoding utf8 = Encoding.UTF8;

        public Header header;
        public String name;

        List<Entryv1> entriesv1 = new List<Entryv1>();
        List<Entryv2> entriesv2 = new List<Entryv2>();

        byte[] unkv2;

        List<String> labels = new List<String>();

        public GMD(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Header
                header = br.ReadStruct<Header>();
                name = br.ReadCStringA();

                //Entries
                switch (header.version)
                {
                    case Version.Version1:
                        entriesv1 = br.ReadMultiple<Entryv1>((int)header.labelCount);
                        break;
                    case Version.Version2:
                        entriesv2 = br.ReadMultiple<Entryv2>((int)header.labelCount);
                        break;
                }

                //Unknown part - only in Version 2
                if (header.version == Version.Version2)
                {
                    var bk = br.BaseStream.Position;
                    uint temp = br.ReadUInt32();
                    while (temp < 0x100000 || temp == 0xffffffff) temp = br.ReadUInt32();
                    br.BaseStream.Position -= 4;
                    var unkSize = br.BaseStream.Position - bk;
                    br.BaseStream.Position = bk;

                    unkv2 = br.ReadBytes((int)unkSize);
                }

                //Labels
                for (int i = 0; i < header.labelCount; i++) labels.Add(br.ReadCStringA());

                //Text
                XOR xor = new XOR(header.version);
                long dataOffset = 0;
                switch (header.version)
                {
                    case Version.Version1:
                        dataOffset = 0x28 + name.Length + 1 + header.labelCount * 0x8 + header.labelSize;
                        break;
                    case Version.Version2:
                        dataOffset = 0x28 + name.Length + 1 + header.labelCount * 0x14 + unkv2.Length + header.labelSize;
                        break;
                }

                byte[] text = br.ReadBytes((int)header.secSize);
                if (XOR.IsXORed(br.BaseStream, (uint)dataOffset))
                {
                    text = XOR.Deobfuscate(text);
                }

                using (var brt = new BinaryReaderX(new MemoryStream(text)))
                {
                    for (int i = 0; i < header.secCount; i++)
                    {
                        var bk = brt.BaseStream.Position;
                        byte tmp = brt.ReadByte();
                        while (tmp != 0) tmp = brt.ReadByte();
                        var textSize = brt.BaseStream.Position - bk;
                        brt.BaseStream.Position = bk;

                        Labels.Add(new Label
                        {
                            Name = labels[i],
                            Text = brt.ReadString((int)textSize, utf8),
                            TextID = i
                        });
                    }
                }
            }
        }

        public void Save(string filename)
        {
            /*bool result = false;

            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    BinaryWriterX bw = new BinaryWriterX(fs);

                    // Header
                    bw.WriteASCII(Header.Identifier + "\0");
                    bw.Write(Header.Unknown1);
                    bw.Write(Header.Unknown2);
                    bw.Write(Header.Unknown3);
                    bw.Write(Header.NumberOfLabels);
                    bw.Write(Header.NumberOfOffsets);
                    bw.Write(Header.Unknown4);
                    long dataSizeOffset = bw.BaseStream.Position;
                    bw.Write(Header.DataSize);
                    bw.Write(Header.NameLength);
                    bw.WriteASCII(Header.Name + "\0");

                    foreach (Label label in Labels)
                    {
                        bw.Write(label.ID);
                        bw.Write(label.Unknown1);
                        bw.Write(label.Unknown2);
                        bw.Write(label.Unknown3);
                        bw.Write(label.Unknown4);
                    }

                    bw.Write(Unknown1024);

                    // Read in the label names
                    foreach (Label label in Labels)
                    {
                        bw.WriteASCII(label.Name);
                        bw.Write(new byte[] { 0x0 });
                    }

                    // Read in the text data
                    uint dataSize = 0;
                    foreach (Label label in Labels)
                    {
                        bw.Write(label.Text);
                        bw.Write(new byte[] { 0x0 });
                        dataSize += (uint)label.Text.Length + 1;
                    }

                    // Update DataSize
                    bw.BaseStream.Seek(dataSizeOffset, SeekOrigin.Begin);
                    bw.Write(dataSize);

                    bw.Close();

                    result = true;
                }
            }
            catch (Exception)
            { }

            return result;
        }*/
        }
    }
}