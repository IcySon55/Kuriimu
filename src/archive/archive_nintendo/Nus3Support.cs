using System.IO;
using System.Runtime.InteropServices;
using Kontract;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.NUS3
{
    public class NUS3FileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic Magic;
        public int FileSize; // Minus header size 0x8
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class BankTOC
    {
        Magic8 Magic;
        public int Size;
        public int SectionCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SectionHeader
    {
        public Magic Magic;
        public int SectionSize;
    }

    public class PROP
    {
        public Magic Magic;
        public int SectionSize;
        public int Unk1;
        public int Unk2;
        public int Unk3;
        public byte PropertyNameSize;
        public string PropertyName; // Pad to 0x4 alignment
        public int Unk4;
        public int Unk5;

        public PROP(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                Magic = br.ReadString(4);
                SectionSize = br.ReadInt32();
                Unk1 = br.ReadInt32();
                Unk2 = br.ReadInt32();
                Unk3 = br.ReadInt32();
                PropertyNameSize = br.ReadByte();
                PropertyName = br.ReadCStringA();
                br.SeekAlignment(4);
                Unk4 = br.ReadInt32();
                Unk5 = br.ReadInt32();
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, true))
            {
                bw.Write(Unk1);
                bw.Write(Unk2);
                bw.Write(Unk3);
                PropertyNameSize = (byte)(PropertyName.Length + 1);
                bw.Write(PropertyNameSize);
                bw.WriteASCII(PropertyName);
                bw.Write((byte)0);
                bw.WriteAlignment(4);
                bw.Write(Unk4);
                bw.Write(Unk5);
            }
        }
    }

    public class BINF
    {
        public Magic Magic;
        public int SectionSize;
        public int Unk1;
        public int Unk2;
        public byte NameSize;
        public string Name;
        public int Unk3;

        public BINF(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                Magic = br.ReadString(4);
                SectionSize = br.ReadInt32();
                Unk1 = br.ReadInt32();
                Unk2 = br.ReadInt32();
                NameSize = br.ReadByte();
                Name = br.ReadCStringA();
                br.SeekAlignment(4);
                Unk3 = br.ReadInt32();
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, true))
            {
                bw.Write(Unk1);
                bw.Write(Unk2);
                NameSize = (byte)(Name.Length + 1);
                bw.Write(NameSize);
                bw.WriteASCII(Name);
                bw.Write((byte)0);
                bw.WriteAlignment(4);
                bw.Write(Unk3);
            }
        }
    }

    public class GRP
    {
        public Magic Magic;
        public int SectionSize;
        public byte[] Content;

        public GRP(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                Magic = br.ReadString(4);
                SectionSize = br.ReadInt32();
                Content = br.ReadBytes(SectionSize);
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, true))
            {
                bw.Write(Content);
            }
        }
    }

    public class DTON
    {
        public Magic Magic;
        public int SectionSize;
        public byte[] Content;

        public DTON(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                Magic = br.ReadString(4);
                SectionSize = br.ReadInt32();
                Content = br.ReadBytes(SectionSize);
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, true))
            {
                bw.Write(Content);
            }
        }
    }

    public class TONE
    {
        public Magic Magic;
        public int SectionSize;
        public int ToneCount;
        public int MetaSize;
        public byte[] MetaContent;
        public int Unk1;
        public byte NameSize;
        public string Name;
        public int Unk2;
        public int Unk3;
        public int Unk4;
        public int PackSize;
        public byte[] Unk5;

        public TONE(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                Magic = br.ReadString(4);
                SectionSize = br.ReadInt32();
                var startOffset = (int)br.BaseStream.Position;
                ToneCount = br.ReadInt32();
                MetaSize = br.ReadInt32();
                MetaContent = br.ReadBytes(MetaSize);
                Unk1 = br.ReadInt32();
                NameSize = br.ReadByte();
                Name = br.ReadCStringA();
                br.SeekAlignment(4);
                Unk2 = br.ReadInt32();
                Unk3 = br.ReadInt32();
                Unk4 = br.ReadInt32();
                PackSize = br.ReadInt32();
                Unk5 = br.ReadBytes(SectionSize - ((int)br.BaseStream.Position - startOffset));
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, true))
            {
                bw.Write(ToneCount);
                bw.Write(MetaSize);
                bw.Write(MetaContent);
                bw.Write(Unk1);
                NameSize = (byte)(Name.Length + 1);
                bw.Write(NameSize);
                bw.WriteASCII(Name);
                bw.Write((byte)0);
                bw.WriteAlignment(4);
                bw.Write(Unk2);
                bw.Write(Unk3);
                bw.Write(Unk4);
                bw.Write(PackSize);
                bw.Write(Unk5);
            }
        }
    }

    public class JUNK
    {
        public Magic Magic;
        public int SectionSize;
        public byte[] Content;

        public JUNK(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                Magic = br.ReadString(4);
                SectionSize = br.ReadInt32();
                Content = br.ReadBytes(SectionSize);
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, true))
            {
                bw.Write(Content);
            }
        }
    }
}
