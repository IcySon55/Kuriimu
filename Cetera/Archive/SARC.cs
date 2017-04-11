using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Kuriimu.IO;
using Kuriimu.Contract;
using Cetera.Hash;

namespace Cetera.Archive
{
    public sealed class SARC
    {
        public List<SARCFileInfo> Files = new List<SARCFileInfo>();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SARCHeader
        {
            Magic magic;
            public short headerSize;
            ByteOrder byteOrder;
            public int fileSize;
            public int dataOffset;
            int unk1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SFATHeader
        {
            Magic magic;
            public short headerSize;
            public short nodeCount;
            public int hashMultiplier;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SFATEntry
        {
            public uint nameHash;
            public short SFNTOffset;
            public short unk1;
            public int dataStart;
            public int dataEnd;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SFNTHeader
        {
            Magic magic;
            public short headerSize;
            public short unk1;
        }

        public class SARCFileInfo : ArchiveFileInfo
        {
            public SFATEntry Entry;
        }

        public SARC(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                SARCHeader sarcHeader = br.ReadStruct<SARCHeader>();
                SFATHeader sfatHeader = br.ReadStruct<SFATHeader>();

                List<SFATEntry> SFATEntries = new List<SFATEntry>();
                SFATEntries.AddRange(br.ReadMultiple<SFATEntry>(sfatHeader.nodeCount));

                SFNTHeader sfntHeader = br.ReadStruct<SFNTHeader>();

                List<String> nameList = new List<String>();
                for (int i = 0; i < sfatHeader.nodeCount; i++)
                {
                    nameList.Add(br.ReadCStringA());

                    while (br.ReadByte() == 0) ;
                    br.BaseStream.Position -= 1;
                }

                for (int i = 0; i < sfatHeader.nodeCount; i++)
                    Files.Add(new SARCFileInfo()
                    {
                        Entry = new SFATEntry()
                        {
                            nameHash = SFATEntries[i].nameHash,
                            SFNTOffset = SFATEntries[i].SFNTOffset,
                            unk1 = SFATEntries[i].unk1,
                            dataStart = SFATEntries[i].dataStart,
                            dataEnd = SFATEntries[i].dataEnd
                        },
                        FileName = nameList[i],
                        FileData = new SubStream(
                            input,
                            sarcHeader.dataOffset + SFATEntries[i].dataStart,
                            SFATEntries[i].dataEnd - SFATEntries[i].dataStart),
                        State = ArchiveFileState.Archived
                    });
            }
        }

        public void Save(Stream input)
        {
            using (BinaryWriterX bw = new BinaryWriterX(input))
            {
                //SARCHeader
                bw.WriteASCII("SARC");
                bw.Write((short)0x14);
                bw.Write((ushort)0xfeff);
                bw.Write(0); //Filesize will be added later
                bw.Write(0);
                bw.Write(0x100);

                //SFATHeader
                bw.WriteASCII("SFAT");
                bw.Write((short)0xc);
                bw.Write((short)Files.Count);
                bw.Write(0x65);

                //SFAT List + nameList
                int nameOffset = 0x14 + 0xc + Files.Count * 0x10 + 8;
                int dataOffset = 0;
                for (int i = 0; i < Files.Count; i++)
                {
                    if (Files[i].State == ArchiveFileState.Added)
                        bw.Write(SimpleHash.Create(Files[i].FileName, 0x65));
                    else
                        bw.Write(Files[i].Entry.nameHash);

                    bw.Write((short)((nameOffset - 0x14 - 0xc - 8 - Files.Count * 0x10) / 4));
                    long bk = bw.BaseStream.Position; bw.BaseStream.Position = nameOffset;
                    bw.WriteASCII(Files[i].FileName); bw.Write((byte)0);
                    bw.BaseStream.Position = bk;
                    int nameOffsetTmp = Files[i].FileName.Length + 1;
                    while (nameOffsetTmp % 4 != 0) nameOffsetTmp++;
                    nameOffset += nameOffsetTmp;

                    bw.Write((short)0x100);

                    bw.Write(dataOffset);
                    bw.Write(dataOffset + (int)Files[i].FileData.Length);
                    dataOffset += (int)Files[i].FileData.Length;
                }
                bw.WriteASCII("SFNT");
                bw.Write(8);

                //Add dataOffset to SARCHeader
                bw.BaseStream.Position = 0xc;
                int pos = (bw.BaseStream.Length % 0x100 > 0) ? (int)((bw.BaseStream.Length / 0x100) + 1) * 0x100 : (int)bw.BaseStream.Length;
                bw.Write(pos);
                bw.BaseStream.Position = pos;

                //Add Files
                for (int i = 0; i < Files.Count; i++) bw.Write(new BinaryReaderX(Files[i].FileData).ReadBytes((int)Files[i].FileData.Length));

                //Add FileSize to SARC Header
                bw.BaseStream.Position = 0x8;
                bw.Write((int)bw.BaseStream.Length);
            }
        }
    }
}
