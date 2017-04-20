using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.IO;
using Kuriimu.Contract;

namespace archive_pck
{
    public sealed class PCK
    {
        public List<PckFileInfo> Files;

        public PCK(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var entries = br.ReadMultiple<PCKEntry>(br.ReadInt32()).ToList();

                Files = entries.Select(entry =>
                {
                    br.BaseStream.Position = entry.fileOffset;
                    var hashes = (br.ReadInt16() == 0x64) ? br.ReadMultiple(br.ReadInt16(), _ => br.ReadUInt32()).ToList() : null;
                    int blockOffset = hashes?.Count + 1 ?? 0;

                    return new PckFileInfo
                    {
                        FileData = new SubStream(
                            input,
                            entry.fileOffset + blockOffset * 4,
                            entry.fileLength - blockOffset * 4),
                        FileName = $"0x{entry.hash:X8}.bin",
                        State = ArchiveFileState.Archived,
                        Entry = entry,
                        Hashes = hashes
                    };
                }).ToList();
            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {
                bw.Write(Files.Count);

                // entryList
                int dataPos = 4 + Files.Count * 0xc;
                foreach (var afi in Files)
                {
                    var entry = new PCKEntry
                    {
                        hash = afi.Entry.hash,
                        fileOffset = dataPos,
                        fileLength = 4 * (afi.Hashes?.Count + 1 ?? 0) + (int)afi.FileSize
                    };
                    dataPos += entry.fileLength;
                    bw.WriteStruct(entry);
                }

                // data
                foreach (var afi in Files)
                {
                    if (afi.Hashes != null)
                    {
                        bw.Write((short)0x64);
                        bw.Write((short)afi.Hashes.Count);
                        foreach (var hash in afi.Hashes)
                            bw.Write(hash);
                    }
                    afi.FileData.CopyTo(bw.BaseStream);
                }
            }
        }
    }
}
