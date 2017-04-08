using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cetera.Hash;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_hpi_hpb
{
    public sealed class HPIHPB : List<HpiHpbAfi>, IDisposable
    {
        public const uint HashSlotCount = 0x1000;
        public const uint PathHashMagic = 0x25;

        private FileStream _hpiStream = null;
        private FileStream _hpbStream = null;

        public HPIHPB(FileStream hpiStream, FileStream hpbStream)
        {
            _hpiStream = hpiStream;
            _hpbStream = hpbStream;
            using (var br = new BinaryReaderX(hpiStream, true))
            {
                // Header
                var header = br.ReadStruct<HpiHeader>();

                // Hash List
                List<HashEntry> hashList = new List<HashEntry>();
                hashList.AddRange(br.ReadMultiple<HashEntry>(header.infoCount));

                // Entries
                AddRange(br.ReadMultiple<Entry>(header.entryCount).OrderBy(e => e.stringOffset).Select(entry => new HpiHpbAfi
                {
                    Entry = entry,
                    FileName = br.ReadCStringA(),
                    FileData = new SubStream(hpbStream, entry.fileOffset, entry.fileSize),
                    State = ArchiveFileState.Archived
                }));
            }
        }

        public void Save(FileStream hpi, FileStream hpb)
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
                hpiBw.Write((short)HashSlotCount);
                hpiBw.Write(this.Count());

                //hashList
                List<HashSort> hashSort = new List<HashSort>();
                foreach (var entry in this)
                    hashSort.Add(new HashSort()
                    {
                        Entry = entry,
                        Hash = SimpleHash.Create(entry.FileName, PathHashMagic, HashSlotCount)
                    });
                hashSort = hashSort.OrderBy(e => e.Hash).ToList();

                int hash = 0;
                int hashes = 0;
                int count = 0;
                for (int i = 0; i < hashSort.Count(); i++)
                {
                    if (hashSort[i].Hash == hash)
                    {
                        hashes++;
                        count++;
                    }
                    else
                    {
                        hpiBw.Write((short)(count - hashes));
                        hpiBw.Write((short)hashes);
                        for (int j = 0; j < hashSort[i].Hash - hash - 1; j++)
                        {
                            hpiBw.Write((short)count);
                            hpiBw.Write((short)0);
                        }
                        hashes = 1;
                        hash = (int)hashSort[i].Hash;
                        count++;
                    }
                }
                hpiBw.Write((short)(count - hashes));
                hpiBw.Write((short)hashes);

                // Entry List
                int nameListOffset = 0x18 + 0x4000 + this.Count() * 0x10;
                count = 0;
                foreach (var entry in hashSort)
                {
                    // Name Offset + Name List Entry
                    int nameOffset = (hpiBw.BaseStream.Length >= nameListOffset) ? (int)hpiBw.BaseStream.Length - nameListOffset : 0;
                    hpiBw.Write(nameOffset);
                    hpiBw.BaseStream.Position = nameListOffset + nameOffset;
                    hpiBw.WriteASCII(entry.Entry.FileName); hpiBw.Write((byte)0);

                    // File Offset
                    hpiBw.BaseStream.Position = 0x4018 + count * 0x10 + 0x4;
                    hpiBw.Write((uint)hpbBw.BaseStream.Length);

                    if (entry.Entry.State == ArchiveFileState.Added)
                    {
                        byte[] data = new BinaryReaderX(entry.Entry.FileData).ReadBytes((int)entry.Entry.FileData.Length);
                        hpbBw.Write(data, 0, data.Count());

                        // File Length
                        hpiBw.Write(data.Count());

                        // Uncompressed Size
                        hpiBw.Write(0);
                    }
                    else
                    {
                        Stream compData = entry.Entry.FileData;

                        if (entry.Entry.State == ArchiveFileState.Replaced && entry.Entry.Entry.uncompressedSize != 0)
                        {
                            compData = entry.Entry.GetCompressedStream(entry.Entry.FileData);

                            // ACMP Header
                            hpbBw.WriteASCII("ACMP");
                            hpbBw.Write((int)compData.Length + 0x20);
                            hpbBw.Write(0x20);
                            hpbBw.Write(0);
                            hpbBw.Write((int)entry.Entry.FileData.Length);
                            for (int j = 0; j < 3; j++) hpbBw.Write(0x01234567);
                        }
                        else
                        {
                            compData = entry.Entry.FileData;
                        }

                        byte[] data = new BinaryReaderX(compData).ReadBytes((int)compData.Length);
                        hpbBw.Write(data, 0, data.Count());

                        // File Length
                        if (entry.Entry.State == ArchiveFileState.Replaced && entry.Entry.Entry.uncompressedSize != 0)
                            hpiBw.Write(data.Count() + 0x20);
                        else hpiBw.Write(data.Count());

                        // Uncompressed Size
                        if (entry.Entry.State == ArchiveFileState.Replaced && entry.Entry.Entry.uncompressedSize != 0)
                            hpiBw.Write((int)entry.Entry.FileData.Length);
                        else hpiBw.Write((int)entry.Entry.Entry.uncompressedSize);
                    }
                    count++;
                }
            }

            hpbBw.Close();
        }

        public void Dispose()
        {
            _hpiStream.Dispose();
            _hpbStream.Dispose();
        }
    }
}