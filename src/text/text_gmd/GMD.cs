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

        bool xored = false;

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

                    temp = br.ReadByte();
                    while (temp == 0) temp = br.ReadByte();
                    br.BaseStream.Position--;

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
                if (XOR.IsXORed(br.BaseStream))
                {
                    xored = true;
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
            
        }
    }
}