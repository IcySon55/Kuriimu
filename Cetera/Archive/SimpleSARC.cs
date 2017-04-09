using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace Cetera.Archive
{
    public class SimpleSARC
    {
        public List<SimpleSARCFileInfo> Files = new List<SimpleSARCFileInfo>();

        public class SimpleSARCFileInfo : ArchiveFileInfo
        {
            public SimpleSFATEntry Entry;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SimpleSARCHeader
        {
            Magic magic;
            public int nodeCount;
            public int unk1;
            int unk2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SimpleSFATEntry
        {
            public uint hash;
            public int dataStart;
            public int dataSize;
            public int zero0;
        }

        SimpleSARCHeader ssarcHeader;

        public SimpleSARC(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                ssarcHeader = br.ReadStruct<SimpleSARCHeader>();

                List<SimpleSFATEntry> entries = new List<SimpleSFATEntry>();
                entries.AddRange(br.ReadMultiple<SimpleSFATEntry>(ssarcHeader.nodeCount));

                for (int i = 0; i < ssarcHeader.nodeCount; i++)
                    Files.Add(new SimpleSARCFileInfo()
                    {
                        FileName = "File " + i,
                        FileData = new SubStream(input, entries[i].dataStart, entries[i].dataSize),
                        State = ArchiveFileState.Archived,
                        Entry = entries[i]
                    });
            }
        }

        public void Save(Stream input)
        {
            using (BinaryWriterX bw = new BinaryWriterX(input))
            {
                bw.WriteASCII("SARC");
                bw.Write(Files.Count);
                bw.Write(ssarcHeader.unk1);
                bw.Write(0x100);

                uint dataOffset = 0x10 + (uint)Files.Count * 0x10;
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.Write(Files[i].Entry.hash);
                    bw.Write(dataOffset);
                    bw.Write((uint)Files[i].FileData.Length);
                    bw.Write(0);

                    bw.BaseStream.Position = dataOffset;
                    bw.Write(new BinaryReaderX(Files[i].FileData).ReadBytes((int)Files[i].FileData.Length));
                    bw.BaseStream.Position = 0x10 + (i + 1) * 0x10;
                    dataOffset += (uint)Files[i].FileData.Length;
                }
            }
        }

        /*
        public void SimplerSARC(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input))
            {
                ssarcHeader = br.ReadStruct<SimplerSARCHeader>();

                for (int i = 0; i < ssarcHeader.nodeCount; i++)
                {
                    Add(new Node());
                    this[i].state = State.Simpler;
                    this[i].sNodeEntry = new SimplerSFATNode(br.BaseStream);
                }

                for (int i = 0; i < ssarcHeader.nodeCount; i++)
                {
                    br.BaseStream.Position = this[i].sNodeEntry.dataStart;
                    this[i].fileName = "File" + i.ToString();
                }
            }
        }

        public void Save(Stream input)
        {
            int t = 0;
        }*/
    }
}
