using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_nintendo.NUS3
{
    public class NUS3FileInfo : ArchiveFileInfo
    {
        public TONE.ToneEntry Entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic;
        public int fileSize; //without magic
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BankToc
    {
        Magic8 magic;
        public int size;
        public int entryCount;
    }

    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class BankTocEntry
    {
        public BankTocEntry(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                magic = br.ReadStruct<Magic>();
                secSize = br.ReadInt32();
            }
        }
        public Magic magic;
        public int secSize;
        public int offset = 0;
    }

    public class PROP
    {
        public PROP(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                unk1 = br.ReadInt32();
                unk2 = br.ReadInt32();
                unk3 = br.ReadInt32();
                projectNameSize = br.ReadByte();
                projectName = br.ReadCStringA() + Encoding.ASCII.GetString(new byte[] { 0 });

                int paddingTmp = 0;
                br.BaseStream.Position++;
                paddingTmp++;
                while (br.BaseStream.Position % 4 > 0)
                {
                    br.BaseStream.Position++;
                    paddingTmp++;
                }
                padding = new byte[paddingTmp];
                for (int i = 0; i < paddingTmp; i++) padding[i] = 0;

                unk4 = br.ReadInt32();
                dateSize = br.ReadByte();
                date = br.ReadCStringA() + Encoding.ASCII.GetString(new byte[] { 0 });
            }
        }
        public int unk1;
        public int unk2;
        public int unk3;
        public byte projectNameSize;
        public string projectName;
        public byte[] padding;
        public int unk4;
        public byte dateSize;
        public string date;
        public byte[] padding2 = { 0, 0, 0 };
    }

    public class BINF
    {
        public BINF(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                unk1 = br.ReadInt32();
                unk2 = br.ReadInt32();
                nameSize = br.ReadByte();
                name = br.ReadCStringA() + Encoding.ASCII.GetString(new byte[] { 0 });

                int paddingTmp = 0;
                br.BaseStream.Position++;
                paddingTmp++;
                while (br.BaseStream.Position % 4 > 0)
                {
                    br.BaseStream.Position++;
                    paddingTmp++;
                }
                padding = new byte[paddingTmp];
                for (int i = 0; i < paddingTmp; i++) padding[i] = 0;

                ID = br.ReadInt32();
            }
        }
        public int unk1;
        public int unk2;
        public byte nameSize;
        public string name;
        public byte[] padding;
        public int ID;
    }

    public class TONE
    {
        public TONE(Stream input, int toneOffset, int packOffset)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                toneCount = br.ReadInt32();

                toneEntries = new ToneEntry[toneCount];
                for (int i = 0; i < toneCount; i++)
                    toneEntries[i] = new ToneEntry
                    {
                        offset = br.ReadInt32() + toneOffset + 8,
                        metaSize = br.ReadInt32()
                    };
                for (int i = 0; i < toneCount; i++)
                {
                    if (toneEntries[i].metaSize > 0xc)
                    {
                        br.BaseStream.Position = toneEntries[i].offset + 6;
                        byte tmp = br.ReadByte();
                        if (tmp == 0 || tmp > 9)
                        {
                            br.BaseStream.Position--;
                            toneEntries[i].unk1 = br.ReadBytes(6);
                        }
                        else
                        {
                            br.BaseStream.Position--;
                            toneEntries[i].unk1 = br.ReadBytes(1);
                        }

                        toneEntries[i].nameSize = br.ReadByte();
                        toneEntries[i].name = br.ReadCStringA();

                        int paddingTmp = 0;
                        br.BaseStream.Position++;
                        paddingTmp++;
                        while (br.BaseStream.Position % 4 > 0)
                        {
                            br.BaseStream.Position++;
                            paddingTmp++;
                        }
                        toneEntries[i].padding = new byte[paddingTmp];
                        for (int j = 0; j < paddingTmp; j++) toneEntries[i].padding[j] = 0;

                        if (br.ReadInt32() != 0)
                        {
                            br.BaseStream.Position -= 4;
                        }
                        else
                        {
                            toneEntries[i].zero0 = 0;
                        }
                        br.BaseStream.Position += 4;
                        toneEntries[i].packOffset = packOffset + 8 + br.ReadInt32();
                        toneEntries[i].size = br.ReadInt32();
                        int restSize = toneEntries[i].metaSize -
                                       ((int)br.BaseStream.Position - toneEntries[i].offset);
                        toneEntries[i].rest = br.ReadBytes(restSize);
                    }
                }
            }
        }
        public int toneCount;
        public ToneEntry[] toneEntries;

        public class ToneEntry
        {
            public int offset;
            public int metaSize;
            public byte[] unk1;
            public byte nameSize;
            public string name;
            public byte[] padding;
            public int zero0 = -1;
            public int packOffset;
            public int size;
            public byte[] rest;
        }
    }
}
