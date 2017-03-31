using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KuriimuContract;
using Cetera.Compression;

namespace archive_nus3bank
{
    public sealed class NUS3 : List<NUS3.Node>
    {
        public class Node
        {
            public String filename;
            public TONE.ToneEntry entry;
            public MemoryStream FileData;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            Magic magic;
            public int fileSize; //without magic
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BankToc
        {
            Magic8 magic;
            public int size;
            public int entryCount;
        }
        public class BankTocEntry
        {
            public BankTocEntry(Stream input)
            {
                using (Cetera.IO.BinaryReaderX br = new Cetera.IO.BinaryReaderX(input, true))
                {
                    magic = br.ReadStruct<Magic>();
                    secSize = br.ReadInt32();
                }
            }
            public Magic magic;
            public int secSize;
            public int offset;
        }

        public class PROP
        {
            public PROP(Stream input)
            {
                using (Cetera.IO.BinaryReaderX br = new Cetera.IO.BinaryReaderX(input, true))
                {
                    unk1 = br.ReadInt32();
                    unk2 = br.ReadInt32();
                    unk3 = br.ReadInt32();
                    projectNameSize = br.ReadByte();
                    projectName = readASCII(br.BaseStream);

                    br.BaseStream.Position += 1;
                    while (br.BaseStream.Position % 4 > 0)
                    {
                        br.BaseStream.Position += 1;
                    }

                    unk4 = br.ReadInt32();
                    dateSize = br.ReadByte();
                    date = readASCII(br.BaseStream);
                }
            }
            int unk1;
            int unk2;
            int unk3;
            public byte projectNameSize;
            public String projectName;
            int unk4;
            public byte dateSize;
            public String date;
        }

        public class BINF
        {
            public BINF(Stream input)
            {
                using (Cetera.IO.BinaryReaderX br = new Cetera.IO.BinaryReaderX(input, true))
                {
                    unk1 = br.ReadInt32();
                    unk2 = br.ReadInt32();
                    nameSize = br.ReadByte();
                    name = readASCII(br.BaseStream);

                    br.BaseStream.Position += 1;
                    while (br.BaseStream.Position % 4 > 0)
                    {
                        br.BaseStream.Position += 1;
                    }

                    ID = br.ReadInt32();
                }
            }
            int unk1;
            int unk2;
            public byte nameSize;
            public String name;
            public int ID;
        }

        public class TONE
        {
            public TONE(Stream input)
            {
                using (Cetera.IO.BinaryReaderX br = new Cetera.IO.BinaryReaderX(input, true))
                {
                    toneCount = br.ReadInt32();

                    toneEntries = new ToneEntry[toneCount];
                    for (int i = 0; i < toneCount; i++)
                    {
                        toneEntries[i] = new ToneEntry();
                        toneEntries[i].offset = br.ReadInt32() + banktocEntries[4].offset + 8;
                        toneEntries[i].metaSize = br.ReadInt32();

                        long bk = br.BaseStream.Position;
                        if (toneEntries[i].metaSize > 0xc)
                        {
                            br.BaseStream.Position = toneEntries[i].offset + 6;
                            byte tmp;
                            tmp = br.ReadByte();
                            if (tmp == 0 || tmp > 9)
                            {
                                br.BaseStream.Position += 5;
                            }
                            else
                            {
                                br.BaseStream.Position += 1;
                            }
                            toneEntries[i].nameSize = br.ReadByte();
                            toneEntries[i].name = readASCII(br.BaseStream);

                            br.BaseStream.Position += 1;
                            while (br.BaseStream.Position % 4 > 0)
                            {
                                br.BaseStream.Position += 1;
                            }

                            if (br.ReadInt32() != 0) br.BaseStream.Position -= 4;
                            br.BaseStream.Position += 4;
                            toneEntries[i].packOffset = banktocEntries[6].offset + 8 + br.ReadInt32();
                            toneEntries[i].size = br.ReadInt32();
                        }
                        br.BaseStream.Position = bk;
                    }
                }
            }
            public int toneCount;
            public ToneEntry[] toneEntries;

            public class ToneEntry
            {
                public int offset;
                public int metaSize;
                public byte nameSize;
                public String name;
                public int packOffset;
                public int size;
            }
        }

        public Header header;
        public BankToc banktocHeader;
        public static List<BankTocEntry> banktocEntries;
        public PROP prop;
        public BINF binf;

        public TONE tone;

        public NUS3(String filename)
        {
            Stream decomp;

            using (BinaryReaderX br = new BinaryReaderX(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                if (br.ReadString(4) != "NUS3")
                {
                    br.BaseStream.Position = 0;
                    decomp = new MemoryStream(ZLib.Decompress(br.ReadBytes((int)br.BaseStream.Length)));
                }
                else
                {
                    br.BaseStream.Position = 0;
                    decomp = new MemoryStream(br.ReadBytes((int)br.BaseStream.Length));
                }
            }

            using (BinaryReaderX br = new BinaryReaderX(decomp))
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
                br.BaseStream.Position = banktocEntries[0].offset;
                br.BaseStream.Position += 8;
                prop = new PROP(br.BaseStream);

                //BINF
                br.BaseStream.Position = banktocEntries[1].offset;
                br.BaseStream.Position += 8;
                binf = new BINF(br.BaseStream);

                //GRP - not yet mapped

                //DTON - not yet mapped

                //TONE
                br.BaseStream.Position = banktocEntries[4].offset;
                br.BaseStream.Position += 8;
                tone = new TONE(br.BaseStream);

                //JUNK - not yet mapped

                //PACK and finishing
                for (int i = 0; i < tone.toneCount; i++)
                {
                    br.BaseStream.Position = tone.toneEntries[i].packOffset;

                    Add(new Node()
                    {
                        filename = tone.toneEntries[i].name + ".idsp",
                        entry = tone.toneEntries[i],
                        FileData = new MemoryStream(br.ReadBytes(tone.toneEntries[i].size))
                    });
                }
            }
        }

        public static String readASCII(Stream input)
        {
            using (Cetera.IO.BinaryReaderX br = new Cetera.IO.BinaryReaderX(input, true))
            {
                String result = "";
                Encoding ascii = Encoding.GetEncoding("ascii");

                byte[] character = br.ReadBytes(1);
                while (character[0] != 0x00)
                {
                    result += ascii.GetString(character);
                    character = br.ReadBytes(1);
                }

                return result;
            }
        }
    }
}
