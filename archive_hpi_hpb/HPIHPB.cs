using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.Contract;
using Kuriimu.IO;
using Cetera.Compression;

namespace archive_hpi_hpb
{
    public sealed class HPIHPB : List<HPIHPB.Node>, IDisposable
    {
        public class Node
        {
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            struct AcmpHeader
            {
                public Magic magic;
                public int compressedSize;
                public int headerSize;
                public int zero;
                public uint uncompressedSize;
                public int padding0, padding1, padding2; // equal to 0x01234567
            }

            public String filename;
            public Entry entry;
            public Stream fileData;

            public Stream GetUncompressedStream()
            {
                fileData.Position = 0;
                if (entry.uncompressedSize == 0) return fileData;
                using (var br = new BinaryReaderX(fileData, true))
                {
                    var header = br.ReadStruct<AcmpHeader>();
                    return new MemoryStream(RevLZ77.Decompress(br.ReadBytes(header.compressedSize), header.uncompressedSize));
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HpiHeader
        {
            public Magic magic;
            public int zero0;
            public int headerSize;  //without magic
            public int zero1;
            public short zero3;
            public short infoCount;
            public int entryCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Entry
        {
            public uint stringOffset;
            public uint fileOffset;
            public uint fileSize;
            public uint uncompressedSize;
        }

        FileStream hpb;

        public HPIHPB(string hpiFilename, string hpbFilename)
        {
            hpb = File.OpenRead(hpbFilename);
            using (var br = new BinaryReaderX(File.OpenRead(hpiFilename)))
            {
                //Header
                var header = br.ReadStruct<HpiHeader>();

                //infoList??? - not mapped
                br.ReadBytes(header.infoCount * 4);

                //Entries
                AddRange(br.ReadMultiple<Entry>(header.entryCount).OrderBy(e => e.stringOffset).Select(entry => new Node
                {
                    entry = entry,
                    filename = br.ReadCStringA(),
                    fileData = new SubStream(hpb, entry.fileOffset, entry.fileSize)
                }));
            }
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Dispose() => hpb.Dispose();
    }
}
