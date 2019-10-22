using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using archive_l7c.Compression;
using Kontract.Interface;
using Kontract.IO;

namespace archive_l7c
{
    public class L7C
    {
        private const int _fileSystemEntrySize = 0x18;

        public List<L7cArchiveFileInfo> Files { get; set; }

        private Stream _stream;
        private L7CAHeader _header;
        private byte[] _systemEntryData;
        private byte[] _stringData;

        public L7C(Stream input)
        {
            Files = new List<L7cArchiveFileInfo>();
            _stream = input;

            /* Order of sections:
             * Header
             * File data
             * File system entries
             * File infos
             * Chunk infos
             * Strings
             */

            using (var br = new BinaryReaderX(input, true))
            {
                // Read header
                _header = br.ReadStruct<L7CAHeader>();
                var stringTableOffset = input.Length - _header.stringTableSize;

                // Read strings
                br.BaseStream.Position = stringTableOffset;
                _stringData = br.ReadBytes(_header.stringTableSize);

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
                br.BaseStream.Position = _header.fileInfoOffset;
                _systemEntryData = br.ReadBytes(_header.filesystemEntries * _fileSystemEntrySize);

                br.BaseStream.Position = _header.fileInfoOffset;
                var entries = new Dictionary<int, L7CAFilesystemEntry>();
                for (int i = 0; i < _header.filesystemEntries; i++)
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
                for (int i = 0; i < _header.files; i++)
                {
                    files.Add(br.ReadStruct<L7CAFileEntry>());
                }

                // Read chunk information
                var chunks = new List<L7CAChunkEntry>();
                for (int i = 0; i < _header.chunks; i++)
                {
                    chunks.Add(br.ReadStruct<L7CAChunkEntry>());
                }

                // Add to files
                for (int i = 0; i < _header.files; i++)
                {
                    var file = files[i];
                    var t=files.Where(x => x.chunkIdx > 0).ToList();
                    var entry = entries[i];

                    Files.Add(new L7cArchiveFileInfo(new SubStream(input, file.offset, file.compressedFilesize), file.rawFilesize, chunks.Skip(file.chunkIdx).Take(file.chunkCount).ToArray(), file)
                    {
                        State = ArchiveFileState.Archived,
                        FileName = entry.filename
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            /* Todo for save
             * 1. Write all file data
             * 2. Write file system entries
             * 3. Write file infos
             * 4. Write chunks
             * 5. Write string table
             * 6. Write header
             */

            using (var bw = new BinaryWriterX(output))
            {
                // Write (compressed) file data and retrieve file and chunk infos
                var fileInfos = new List<L7CAFileEntry>();
                var chunks = new List<L7CAChunkEntry[]>();

                var chunkId = 0;
                output.Position = 0x200;
                var fileDataEnd = output.Position;
                foreach (var file in Files)
                {
                    var fileOffset = output.Position;
                    var fileChunks = file.WriteFile(output, out var crc32, out var compressedSize);
                    fileDataEnd = output.Position;
                    bw.WriteAlignment(0x200);

                    chunks.Add(fileChunks);

                    fileInfos.Add(new L7CAFileEntry
                    {
                        offset = (int)fileOffset,
                        chunkIdx = chunkId,
                        chunkCount = fileChunks.Length,
                        rawFilesize = (int)file.FileSize,
                        compressedFilesize = compressedSize,
                        crc32 = crc32
                    });

                    chunkId += fileChunks.Length;
                }

                bw.BaseStream.Position = fileDataEnd;

                // Write file system entries
                _header.fileInfoOffset = (int)fileDataEnd;
                bw.Write(_systemEntryData);

                // Write file infos
                foreach (var fileInfo in fileInfos)
                    bw.WriteStruct(fileInfo);

                // Write chunk infos
                _header.chunks = chunks.Sum(x => x.Length);
                foreach (var fileChunks in chunks)
                    foreach (var fileChunk in fileChunks)
                        bw.WriteStruct(fileChunk);

                // Write string data
                bw.Write(_stringData);

                // Updating remaining header
                _header.archiveSize = (int)output.Length;
                _header.fileInfoSize = _header.archiveSize - _header.fileInfoOffset;

                // Write header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(_header);
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
