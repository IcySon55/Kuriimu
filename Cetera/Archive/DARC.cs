using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.IO;

namespace Cetera.Archive
{
    public class DARC : List<DARC.Item>
    {
        [DebuggerDisplay("{Path}")]
        public class Item
        {
            public string Path { get; set; }
            public byte[] Data { get; set; }
            public TableEntry Entry { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            public String4 magic;
            public ByteOrder byteOrder;
            public short headerSize;
            public int version;
            public int fileSize;
            public int tableOffset;
            public int tableLength;
            public int dataOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class TableEntry
        {
            private int filenameOffset;
            public int fileOffset;
            public int size;

            public bool IsFolder => (filenameOffset >> 24) == 1;
            public int FilenameOffset => filenameOffset & 0xFFFFFF;
        }

        public DARC(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                // @todo: the header is currently unused
                br.ReadStruct<Header>();

                var lst = new List<TableEntry> { br.ReadStruct<TableEntry>() };
                for (int i = 0; i < lst[0].size - 1; i++)
                    lst.Add(br.ReadStruct<TableEntry>());

                foreach (var entry in lst)
                {
                    Add(new Item { Path = "", Entry = entry });
                }

                var basePos = br.BaseStream.Position;

                for (int i = 0; i < Count; i++)
                {
                    var entry = lst[i];
                    br.BaseStream.Position = basePos + entry.FilenameOffset;
                    var arcPath = br.ReadCStringW();
                    if (entry.IsFolder)
                    {
                        arcPath += '/';
                        for (int j = i; j < entry.size; j++)
                        {
                            this[j].Path += arcPath;
                        }
                    }
                    else
                    {
                        this[i].Path += arcPath;
                        br.BaseStream.Position = entry.fileOffset;
                        this[i].Data = br.ReadBytes(entry.size);
                    }
                }

                RemoveAll(item => item.Path.Last() == '/');
            }
        }

        public byte[] Rebuild()
        {
            throw new NotImplementedException();
        }
    }
}