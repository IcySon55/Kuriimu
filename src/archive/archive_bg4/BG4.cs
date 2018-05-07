using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.Interface;
using Kontract.IO;
using Kontract.Compression;

namespace archive_bg4
{
    public sealed class BG4
    {
        public List<BG4FileInfo> Files = new List<BG4FileInfo>();
        Stream _stream = null;

        public Header header;
        public List<FileEntry> entries;

        public BG4(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //FileEntries
                entries = br.ReadMultiple<FileEntry>(header.fileEntryCount);

                //Get Namesection
                var nameSec = br.ReadBytes((int)(header.metaSecSize - br.BaseStream.Position));

                //FileData
                br.BaseStream.Position = header.metaSecSize;
                using (var namesBr = new BinaryReaderX(new MemoryStream(nameSec)))
                    foreach (var entry in entries)
                    {
                        namesBr.BaseStream.Position = entry.relNameOffset;
                        Files.Add(new BG4FileInfo
                        {
                            State = ArchiveFileState.Archived,
                            FileName = namesBr.ReadCStringA(),
                            FileData = new SubStream(br.BaseStream, entry.fileOffset, entry.fileSize & 0x7fffffff),
                            entry = entry,
                            _compressed = (entry.fileSize & 0x80000000) == 0x80000000
                        });
                    }
            }
        }

        public void Save(string filename)
        {
            //Update nameOffsets
            List<NameEntry> redNames = new List<NameEntry>();
            int nameOffset = 0;
            foreach (var file in Files)
                if (redNames.Find(n => n.name.Equals(file.FileName)) == null)
                {
                    file.entry.relNameOffset = (short)nameOffset;

                    redNames.Add(new NameEntry
                    {
                        name = file.FileName,
                        offset = nameOffset
                    });
                    nameOffset += Encoding.ASCII.GetByteCount(file.FileName) + 1;
                }
                else
                {
                    file.entry.relNameOffset = (short)redNames.Find(n => n.name.Equals(file.FileName)).offset;
                }

            var dataOffset = 0x10 + Files.Count * 0xe + ((nameOffset + 3) & ~3);

            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                //Update fileMeta and write FileData
                bw.BaseStream.Position = dataOffset;
                int absDataOffset = dataOffset;
                foreach (var file in Files)
                    absDataOffset = file.Write(bw.BaseStream, absDataOffset);

                //Write Entries and Names
                bw.BaseStream.Position = 0x10;
                foreach (var file in Files) bw.WriteStruct(file.entry);
                foreach (var nameEntry in redNames) bw.Write(Encoding.ASCII.GetBytes(nameEntry.name + "\0"));
                bw.WriteAlignment(4, 0xff);

                //Write Header
                header.fileEntryCount = (short)Files.Count;
                header.fileEntryCountMultiplier = 1;
                header.fileEntryCountDerived = (short)(header.fileEntryCount / header.fileEntryCountMultiplier);
                header.metaSecSize = dataOffset;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }

        public void Close()
        {
            _stream?.Dispose();
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _stream = null;
        }
    }
}
