using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_999
{
    public class A999
    {
        public List<A999FileInfo> Files;

        const uint headXORpad = 0xFABACEDA;

        public A999(Stream input)
        {
            using (var br = new BinaryReaderX(new XorStream(input, headXORpad), true))
            {
                //Header
                var header = br.ReadStruct<Header>();

                //Directories
                var directoryTop = br.ReadStruct<TableHeader>();
                var directoryHashes = br.ReadMultiple<uint>(directoryTop.entryCount);
                while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;
                var directoryEntries = br.ReadMultiple<DirectoryEntry>(directoryTop.entryCount);

                //FileEntries
                var entryTop = br.ReadStruct<TableHeader>();
                var XORs = br.ReadMultiple<uint>(entryTop.entryCount);
                while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;

                //Files
                Files = directoryEntries.SelectMany(dirEntry =>
                {
                    if (!A999Support.foldernames.TryGetValue(dirEntry.directoryHash, out var path))
                        path = $"/UNK/0x{dirEntry.directoryHash:X8}";

                    return br.ReadMultiple<Entry>(dirEntry.fileCount).Select(entry =>
                    {
                        if (!A999Support.filenames.TryGetValue(entry.XORpad, out var filename))
                            filename = $"{path}/0x{entry.XORpad:X8}.unk";

                        return new A999FileInfo
                        {
                            Entry = entry,
                            FileName = filename,
                            State = ArchiveFileState.Archived,
                            FileData = new XorStream(new SubStream(input, header.dataOffset + entry.fileOffset, entry.fileSize), entry.XORpad)
                        };
                    });
                }).ToList();
            }
        }
    }
}
