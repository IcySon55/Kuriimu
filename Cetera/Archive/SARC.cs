using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Kuriimu.IO;
using Kuriimu.Contract;
using Cetera.Hash;
using System.Linq;

namespace Cetera.Archive
{
    public sealed class SARC
    {
        public List<ArchiveFileInfo> Files;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class SARCHeader
        {
            Magic magic = "SARC";
            public short headerSize = 20;
            ByteOrder byteOrder = ByteOrder.LittleEndian;
            public int fileSize;
            public int dataOffset;
            int unk1 = 0x100;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class SFATHeader
        {
            Magic magic = "SFAT";
            public short headerSize = 12;
            public short nodeCount;
            public int hashMultiplier = 0x65;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class SFATEntry
        {
            public uint nameHash;
            public short SFNTOffset;
            public short filenameFlags; // 0x100 means it uses the SFNT table
            public int dataStart;
            public int dataEnd;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class SFNTHeader
        {
            Magic magic = "SFNT";
            public int headerSize = 8;
        }

        bool usesSFNT;

        int padding = 1;
        int Pad(int pos) => (pos + padding - 1) & -padding;

        public SARC(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var sarcHeader = br.ReadStruct<SARCHeader>();
                var sfatHeader = br.ReadStruct<SFATHeader>();
                var sfatEntries = br.ReadMultiple<SFATEntry>(sfatHeader.nodeCount).ToList();
                var sfntHeader = br.ReadStruct<SFNTHeader>();

                usesSFNT = sfatEntries.Any(entry => entry.filenameFlags == 0x100);

                Files = sfatEntries.Select(entry =>
                {
                    var filename = usesSFNT ? br.ReadCStringA() : $"0x{entry.nameHash:X8}.bin";
                    br.BaseStream.Position = (br.BaseStream.Position + 3) & ~3;
                    return new ArchiveFileInfo
                    {
                        FileName = filename,
                        FileData = new SubStream(input, sarcHeader.dataOffset + entry.dataStart, entry.dataEnd - entry.dataStart),
                        State = ArchiveFileState.Archived
                    };
                }).ToList();

                // heuristically determine the padding
                var tmp = sfatEntries.Aggregate(sarcHeader.dataOffset, (i, e) => i | e.dataStart);
                while ((tmp & padding) == 0) padding <<= 1;
            }
        }

        public void Save(Stream output, bool leaveOpen = false)
        {
            using (var bw = new BinaryWriterX(output, leaveOpen))
            {
                //SARCHeader
                var header = new SARCHeader { dataOffset = Pad(40 + Files.Sum(afi => usesSFNT ? afi.FileName.Length / 4 * 4 + 20 : 16)) };
                bw.WriteStruct(header); // filesize is added later

                //SFATHeader
                bw.WriteStruct(new SFATHeader { nodeCount = (short)Files.Count });

                //SFAT List + nameList
                int nameOffset = 0;
                int dataOffset = 0;
                foreach (var afi in Files)
                {
                    var fileLen = (int)afi.FileData.Length;
                    var sfatEntry = new SFATEntry
                    {
                        nameHash = usesSFNT ? SimpleHash.Create(afi.FileName, 0x65) : Convert.ToUInt32(afi.FileName.Substring(2, 8), 16),
                        SFNTOffset = (short)(usesSFNT ? nameOffset : 0),
                        filenameFlags = (short)(usesSFNT ? 0x100 : 0),
                        dataStart = dataOffset,
                        dataEnd = dataOffset + fileLen
                    };
                    bw.WriteStruct(sfatEntry);
                    nameOffset += afi.FileName.Length / 4 + 1;
                    dataOffset = Pad(sfatEntry.dataEnd);
                }

                bw.WriteStruct(new SFNTHeader());
                if (usesSFNT)
                {
                    foreach (var afi in Files)
                    {
                        bw.WriteASCII(afi.FileName.PadRight(afi.FileName.Length / 4 * 4 + 4, '\0'));
                    }
                }

                foreach (var afi in Files)
                {
                    bw.Write(new byte[Pad((int)bw.BaseStream.Length) - (int)bw.BaseStream.Length]); // padding
                    afi.FileData.CopyTo(bw.BaseStream);
                }

                bw.BaseStream.Position = 0;
                header.fileSize = (int)bw.BaseStream.Length;
                bw.WriteStruct(header);
            }
        }
    }
}
