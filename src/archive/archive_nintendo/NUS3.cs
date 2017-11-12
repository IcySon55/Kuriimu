using System;
using System.Collections.Generic;
using System.IO;
using Kontract.Compression;
using Kontract.IO;

namespace archive_nintendo.NUS3
{
    public sealed class NUS3
    {
        public List<NUS3FileInfo> Files = new List<NUS3FileInfo>();
        Stream _stream = null;

        public Header header;
        public BankToc banktocHeader;
        public List<BankTocEntry> banktocEntries;
        public PROP prop;
        public BINF binf;
        public byte[] grp;
        public byte[] dton;
        public TONE tone;
        public byte[] junk;

        public NUS3(string filename, bool isZlibCompressed = false)
        {
            if (isZlibCompressed)
            {
                using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
                {
                    byte[] decomp = ZLib.Decompress(new MemoryStream(br.ReadBytes((int)br.BaseStream.Length)));
                    File.OpenWrite(filename + ".decomp").Write(decomp, 0, decomp.Length);
                }

                _stream = File.OpenRead(filename + ".decomp");
            }
            else
            {
                _stream = File.OpenRead(filename);
            }

            using (var br = new BinaryReaderX(_stream, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Banktoc
                banktocHeader = br.ReadStruct<BankToc>();

                int offset = 0x18 + banktocHeader.entryCount * 0x8;
                banktocEntries = new List<BankTocEntry>();
                for (int i = 0; i < banktocHeader.entryCount; i++)
                {
                    banktocEntries.Add(new BankTocEntry(br.BaseStream));
                    banktocEntries[i].offset = offset;
                    offset += banktocEntries[i].secSize + 8;
                }

                //PROP
                if (br.ReadStruct<Header>().magic != "PROP") throw new Exception();
                prop = new PROP(br.BaseStream);

                //BINF
                br.BaseStream.Position = banktocEntries[1].offset;
                if (br.ReadStruct<Header>().magic != "BINF") throw new Exception();
                binf = new BINF(br.BaseStream);

                //GRP - not yet mapped
                br.BaseStream.Position = banktocEntries[2].offset;
                if (br.ReadStruct<Header>().magic != "GRP ") throw new Exception();
                grp = br.ReadBytes(banktocEntries[2].secSize);

                //DTON - not yet mapped
                br.BaseStream.Position = banktocEntries[3].offset;
                if (br.ReadStruct<Header>().magic != "DTON") throw new Exception();
                dton = br.ReadBytes(banktocEntries[3].secSize);

                //TONE
                br.BaseStream.Position = banktocEntries[4].offset;
                if (br.ReadStruct<Header>().magic != "TONE") throw new Exception();
                tone = new TONE(br.BaseStream, banktocEntries[4].offset, banktocEntries[6].offset);

                //JUNK - not yet mapped
                br.BaseStream.Position = banktocEntries[5].offset;
                if (br.ReadStruct<Header>().magic != "JUNK") throw new Exception();
                junk = br.ReadBytes(banktocEntries[5].secSize);

                //PACK and finishing
                br.BaseStream.Position = banktocEntries[6].offset;
                if (br.ReadStruct<Header>().magic != "PACK") throw new Exception();
                for (int i = 0; i < tone.toneCount; i++)
                {
                    br.BaseStream.Position = tone.toneEntries[i].packOffset;

                    Files.Add(new NUS3FileInfo
                    {
                        FileName = tone.toneEntries[i].name + ".idsp",
                        Entry = tone.toneEntries[i],
                        FileData = new SubStream(_stream, tone.toneEntries[i].packOffset, tone.toneEntries[i].size)
                    });
                }
            }
        }

        public void Save(string filename, bool isZLibCompressed = false)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                //Header
                bw.WriteASCII("NUS3");
                bw.BaseStream.Position += 4;

                //BankToc
                bw.WriteASCII("BANKTOC ");
                bw.Write(0x3c);
                bw.Write(0x7);

                //BankTocEntries
                int propSize = 13 + prop.projectNameSize + prop.padding.Length + 5 + prop.dateSize +
                               prop.padding2.Length;
                int binfSize = 9 + binf.nameSize + binf.padding.Length + 4;
                int tmp = 0; for (int i = 0; i < tone.toneCount; i++) tmp += Files[i].Entry.metaSize;
                int toneSize = 4 + tone.toneCount * 0x8 + tmp;
                int packSize = 0;
                for (int i = 0; i < tone.toneCount; i++) packSize += (int)Files[i].FileData.Length;

                string[] banktocList = new string[7] { "PROP", "BINF", "GRP ", "DTON", "TONE", "JUNK", "PACK" };
                for (int i = 0; i < 7; i++)
                {
                    bw.WriteASCII(banktocList[i]);
                    switch (banktocList[i])
                    {
                        case "PROP":
                            bw.Write(propSize);
                            break;
                        case "BINF":
                            bw.Write(binfSize);
                            break;
                        case "GRP ":
                            bw.Write(grp.Length);
                            break;
                        case "DTON":
                            bw.Write(dton.Length);
                            break;
                        case "TONE":
                            bw.Write(toneSize);
                            break;
                        case "JUNK":
                            bw.Write(junk.Length);
                            break;
                        case "PACK":
                            bw.Write(packSize);
                            break;
                    }
                }

                //PROP
                bw.WriteASCII("PROP");
                bw.Write(propSize);
                bw.Write(prop.unk1);
                bw.Write(prop.unk2);
                bw.Write(prop.unk3);
                bw.Write(prop.projectNameSize);
                bw.WriteASCII(prop.projectName);
                bw.Write(prop.padding);
                bw.Write(prop.unk4);
                bw.Write(prop.dateSize);
                bw.WriteASCII(prop.date);
                bw.Write(prop.padding2);

                //BINF
                bw.WriteASCII("BINF");
                bw.Write(binfSize);
                bw.Write(binf.unk1);
                bw.Write(binf.unk2);
                bw.Write(binf.nameSize);
                bw.WriteASCII(binf.name);
                bw.Write(binf.padding);
                bw.Write(binf.ID);

                //GRP
                bw.WriteASCII("GRP ");
                bw.Write(grp.Length);
                bw.Write(grp);

                //DTON
                bw.WriteASCII("DTON");
                bw.Write(dton.Length);
                bw.Write(dton);

                //TONE
                bw.WriteASCII("TONE");
                bw.Write(toneSize);
                bw.Write(tone.toneCount);
                for (int i = 0; i < tone.toneCount; i++)
                {
                    bw.Write(Files[i].Entry.offset - banktocEntries[4].offset - 8);
                    bw.Write(Files[i].Entry.metaSize);
                }
                int packOffset = 0;
                for (int i = 0; i < tone.toneCount; i++)
                {
                    if (Files[i].Entry.metaSize > 0xc)
                    {
                        bw.Write(0);
                        bw.Write((short)-1);
                        bw.Write(Files[i].Entry.unk1);
                        bw.Write(Files[i].Entry.nameSize);
                        bw.WriteASCII(Files[i].Entry.name);
                        bw.Write((byte)0);
                        bw.Write(Files[i].Entry.padding);
                        if (Files[i].Entry.zero0 == 0) bw.Write(0);
                        bw.Write(8);
                        bw.Write(packOffset);
                        bw.Write((int)Files[i].FileData.Length);
                        packOffset += (int)Files[i].FileData.Length;
                        bw.Write(Files[i].Entry.rest);
                    }
                    else
                    {
                        bw.Write(0);
                        bw.Write(0xff);
                        bw.Write(1);
                    }
                }

                //JUNK
                bw.WriteASCII("JUNK");
                bw.Write(junk.Length);
                bw.Write(junk);

                //PACK
                for (int i = 0; i < tone.toneCount; i++)
                    bw.Write(new BinaryReaderX(Files[i].FileData).ReadBytes((int)Files[i].FileData.Length));

                //update fileSize in NUS3 Header
                bw.BaseStream.Position = 4;
                bw.Write((int)bw.BaseStream.Length);
            }

            if (isZLibCompressed)
            {
                FileStream origFile = File.OpenRead(filename);
                byte[] decomp = new byte[(int)origFile.Length];
                origFile.Read(decomp, 0, (int)origFile.Length);
                byte[] comp = ZLib.Compress(new MemoryStream(decomp));

                origFile.Close();
                File.Delete(filename);
                File.OpenWrite(filename).Write(comp, 0, comp.Length);
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
