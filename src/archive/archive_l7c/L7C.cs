using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract.IO;

namespace archive_l7c
{
    public class L7C
    {
        public List<ArchiveFileInfo> Files { get; set; }

        public L7C(Stream input)
        {
            Files = new List<ArchiveFileInfo>();

            using (var br = new BinaryReaderX(input, true))
            {
                // Read header
                var header = br.ReadStruct<L7CAHeader>();
                var stringTableOffset = input.Length - header.stringTableSize;

                // Read strings
                br.BaseStream.Position = stringTableOffset;
                var strings = new Dictionary<int, string>();
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var offset = (int)(br.BaseStream.Position - stringTableOffset);
                    var stringLength = br.ReadByte();
                    var name = Encoding.UTF8.GetString(br.ReadBytes(stringLength));
                    strings.Add(offset, name);
                }

                // Read file system entries
                br.BaseStream.Position = header.fileInfoOffset;
                var entries = new Dictionary<int, L7CAFilesystemEntry>();
                for (int i = 0; i < header.filesystemEntries; i++)
                {
                    var entry = new L7CAFilesystemEntry(br.BaseStream);
                    entry.filename = strings[entry.folderNameOffset];
                    if (entry.id != -1)
                    {
                        entry.filename += "/" + strings[entry.fileNameOffset];
                        entries.Add(entry.id, entry);
                    }
                }

                // Read file information
                var files = new List<L7CAFileEntry>();
                for (int i = 0; i < header.files; i++)
                {
                    files.Add(br.ReadStruct<L7CAFileEntry>());
                }

                // Read chunk information
                var chunks = new List<L7CAChunkEntry>();
                for (int i = 0; i < header.chunks; i++)
                {
                    chunks.Add(br.ReadStruct<L7CAChunkEntry>());
                }

                // Add to files
                // TODO: Add decompression
                for (int i = 0; i < header.files; i++)
                {
                    var file = files[i];
                    var entry = entries[i];

                    var chunkRecords = new ChunkInfo[file.chunkCount];
                    var offset = file.offset;
                    for (int j = 0; j < chunkRecords.Length; j++)
                    {
                        var length = chunks[file.chunkIdx + j].chunkSize & 0xFFFFFF;
                        chunkRecords[j] = new ChunkInfo(offset, length);

                        var compMode = (chunks[file.chunkIdx + j].chunkSize >> 24) & 0xFF;
                        if (compMode > 0)
                            chunkRecords[j].Decoder = new L7cDecoder(compMode);

                        offset += length;
                    }

                    var chunkStream = new ChunkStream(input, file.rawFilesize, chunkRecords);

                    Files.Add(new L7cArchiveFileInfo
                    {
                        Entry = file,

                        State = ArchiveFileState.Archived,
                        FileName = entry.filename,
                        FileData = chunkStream
                    });
                }
            }
        }
    }
}
