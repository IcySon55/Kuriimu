using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Kuriimu.IO;

namespace archive_pck
{
    public sealed class PCK : List<PCK.Node>
    {
        public class Node
        {
            public String filename;
            public Entry entry;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Entry
        {
            public uint crc32;
            public uint offset;
            public uint length;
        }

        public PCK(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input))
            {
                int pckEntryCount = br.ReadInt32();

                for (int i = 0; i < pckEntryCount; i++)
                {
                    br.BaseStream.Position = 4 + i * 3 * 4;
                    Entry entry = br.ReadStruct<Entry>();

                    br.BaseStream.Position = entry.offset;
                    short blocks = (short)(br.ReadUInt32() >> 16);
                    br.BaseStream.Position = entry.offset;
                    byte[] crcBlocks = br.ReadBytes((blocks + 1) * 4);

                    Add(new Node()
                    {
                        entry = entry,
                        filename = "File" + i.ToString(),
                    });
                }
            }
        }

        public void Save(Stream input)
        {
            int t = 0;
        }
    }
}
