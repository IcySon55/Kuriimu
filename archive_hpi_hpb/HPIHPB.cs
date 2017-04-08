using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.Contract;
using Kuriimu.IO;
using Cetera.Compression;
using Cetera.Hash;

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

            public Stream GetCompressedStream(Stream fileData)
            {
                fileData.Position = 0;
                if (entry.uncompressedSize == 0) return fileData;
                using (var br = new BinaryReaderX(fileData))
                {
                    Stream compData = new MemoryStream(RevLZ77.Compress(br.ReadBytes((int)fileData.Length)));
                    return compData;
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
            public short zero2;
            public short infoCount;
            public int entryCount;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HashEntry
        {
            public short entryOffset;
            public ushort entryCount;
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
        //0x25

        public HPIHPB(string hpiFilename, string hpbFilename)
        {
            hpb = File.OpenRead(hpbFilename);
            using (var br = new BinaryReaderX(File.OpenRead(hpiFilename)))
            {
                //Header
                var header = br.ReadStruct<HpiHeader>();

                //hashList
                List<HashEntry> hashList = new List<HashEntry>();
                hashList.AddRange(br.ReadMultiple<HashEntry>(header.infoCount));

                //Entries
                AddRange(br.ReadMultiple<Entry>(header.entryCount).OrderBy(e => e.stringOffset).Select(entry => new Node
                {
                    entry = entry,
                    filename = br.ReadCStringA(),
                    fileData = new SubStream(hpb, entry.fileOffset, entry.fileSize)
                }));
            }
        }

        class HashSort
        {
            public Entry entry;
            public string filename;
            public uint hash;
        }
        public void Save(Stream hpi, Stream hpb, IEnumerable<ArchiveFileInfo> Files)
        {
            BinaryWriterX hpbBw = new BinaryWriterX(hpb);
            using (BinaryWriterX hpiBw = new BinaryWriterX(hpi))
            {
                //HPIH Header
                hpiBw.WriteASCII("HPIH");
                hpiBw.Write(0);
                hpiBw.Write(0x10);
                hpiBw.Write(0);
                hpiBw.Write((short)0);
                hpiBw.Write((short)0x1000);
                hpiBw.Write(this.Count());

                //hashList
                List<HashSort> hashSort = new List<HashSort>();
                for (int i = 0; i < this.Count(); i++) hashSort.Add(new HashSort() { entry = this[i].entry, filename = this[i].filename, hash = SimpleHash.Create(this[i].filename, 0x25, 0x1000) });
                hashSort = hashSort.OrderBy(e => e.hash).ToList();

                int hash = 0;
                int hashes = 0;
                int count = 0;
                Console.WriteLine(hashSort.Count());
                for (int i = 0; i < hashSort.Count(); i++)
                {
                    if (hashSort[i].hash == hash)
                    {
                        hashes++;
                        count++;
                    }
                    else
                    {
                        hpiBw.Write((short)(count - hashes));
                        hpiBw.Write((short)hashes);
                        for (int j = 0; j < hashSort[i].hash - hash - 1; j++)
                        {
                            hpiBw.Write((short)count);
                            hpiBw.Write((short)0);
                        }
                        hashes = 1;
                        hash = (int)hashSort[i].hash;
                        count++;
                    }
                }
                hpiBw.Write((short)(count - hashes));
                hpiBw.Write((short)hashes);

                //entryList
                int nameListOffset = 0x18 + 0x4000 + this.Count() * 0x10;
                count = 0;
                foreach (var part in Files)
                {
                    //nameOffset + nameList entry
                    int nameOffset = (hpiBw.BaseStream.Length >= nameListOffset) ? nameOffset = (int)hpiBw.BaseStream.Length - nameListOffset : nameOffset = 0;
                    hpiBw.Write(nameOffset);
                    hpiBw.BaseStream.Position = nameListOffset + nameOffset;
                    hpiBw.WriteASCII(part.FileName); hpiBw.Write((byte)0);

                    //fileOffset
                    hpiBw.BaseStream.Position = 0x4018 + count * 0x10;
                    hpiBw.Write((uint)hpbBw.BaseStream.Length);

                    if (part.State == ArchiveFileState.Added)
                    {
                        byte[] data = new BinaryReaderX(part.FileData).ReadBytes((int)part.FileData.Length);
                        hpbBw.Write(data, 0, data.Count());

                        //fileLength
                        hpiBw.Write(data.Count());

                        //uncompressed Size
                        hpiBw.Write(0);
                    }
                    else
                    {
                        Stream compData;
                        if (part.State == ArchiveFileState.Replaced)
                        {
                            compData = part.node.GetCompressedStream(part.FileData);

                            //ACMP Header
                            hpbBw.WriteASCII("ACMP");
                            hpbBw.Write((int)compData.Length + 0x20);
                            hpbBw.Write(0x20);
                            hpbBw.Write(0);
                            hpbBw.Write((int)part.FileData.Length);
                            for (int j = 0; j < 3; j++) hpbBw.Write(0x01234567);
                        }
                        else
                            compData = part.node.fileData;

                        byte[] data = new BinaryReaderX(compData).ReadBytes((int)compData.Length);
                        hpbBw.Write(data, 0, data.Count());

                        //fileLength
                        if (part.State == ArchiveFileState.Replaced) hpiBw.Write(data.Count() + 0x20); else hpiBw.Write(data.Count());

                        //uncompressed Size
                        if (part.State == ArchiveFileState.Replaced) hpiBw.Write((int)part.FileData.Length); else hpiBw.Write((int)part.node.fileData.Length);
                    }
                    count++;
                }
            }
            hpbBw.Close();
        }

        public void Dispose() => hpb.Dispose();
    }
}
