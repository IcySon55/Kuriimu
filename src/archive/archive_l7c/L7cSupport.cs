using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using archive_l7c.Compression;
using Kontract.Hash;
using Kontract.Interface;
using Kontract.IO;

namespace archive_l7c
{
    public class L7cArchiveFileInfo : ArchiveFileInfo
    {
        private L7CAChunkEntry[] _chunks;
        private ChunkStream _chunkStream;
        private bool _isDataSet;

        public L7cArchiveFileInfo(Stream fileData, int rawLength, L7CAChunkEntry[] chunks, L7CAFileEntry fileEntry)
        {
            base.FileData = fileData;
            Entry = fileEntry;

            _chunks = chunks;
            var chunkRecords = GetChunkRecords(chunks);
            _chunkStream = new ChunkStream(fileData, rawLength, chunkRecords);
        }

        private ChunkInfo[] GetChunkRecords(L7CAChunkEntry[] chunks)
        {
            var chunkRecords = new ChunkInfo[chunks.Length];
            var offset = 0;
            for (int j = 0; j < chunkRecords.Length; j++)
            {
                var length = chunks[j].chunkSize & 0xFFFFFF;
                chunkRecords[j] = new ChunkInfo(offset, length);

                var compMode = (chunks[j].chunkSize >> 24) & 0xFF;
                if (compMode > 0)
                    chunkRecords[j].Decoder = new L7cDecoder(compMode);

                offset += length;
            }

            return chunkRecords;
        }

        public L7CAFileEntry Entry { get; }

        public override Stream FileData
        {
            get => _isDataSet ? base.FileData : _chunkStream;
            set
            {
                _isDataSet = true;
                base.FileData = value;
            }
        }

        public override long? FileSize
        {
            get
            {
                if (!_isDataSet)
                    return Entry.rawFilesize;
                else
                    return base.FileData.Length;
            }
        }

        public L7CAChunkEntry[] WriteFile(Stream output, out uint crc32, out int compressedSize)
        {
            if (!_isDataSet)
            {
                // If we still have the original file, write that one and return its chunks

                crc32 = Entry.crc32;
                compressedSize = (int)base.FileData.Length;

                base.FileData.Position = 0;
                base.FileData.CopyTo(output);

                return _chunks;
            }

            // Otherwise compress the new file and return a single chunk description

            base.FileData.Position = 0;
            crc32 = Crc32.Create(new BinaryReaderX(base.FileData, true).ReadAllBytes());

            var parser = new NewOptimalParser(new TaikoLz80PriceCalculator(), 0,
                new BackwardLz77MatchFinder(2, 5, 1, 0x10),
                new BackwardLz77MatchFinder(3, 0x12, 1, 0x400),
                new BackwardLz77MatchFinder(4, 0x83, 1, 0x8000));
            var encoder = new TaikoLz80Encoder();

            base.FileData.Position = 0;
            var buffer = new byte[base.FileData.Length];
            base.FileData.Read(buffer, 0, buffer.Length);
            base.FileData.Position = 0;
            var matches = parser.ParseMatches(buffer, 0);
            var compressedFile = new MemoryStream();
            encoder.Encode(base.FileData, compressedFile, matches);

            compressedSize = (int)compressedFile.Length;
            compressedFile.Position = 0;
            compressedFile.CopyTo(output);

            return new[]
            {
                new L7CAChunkEntry
                {
                    chunkId = 0,
                    chunkSize = (int)(0x80000000 | ((int)compressedFile.Length & 0xFFFFFF))
                }
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class L7CAHeader
    {
        public uint magic = 0x4143374c; // L7CA
        public uint unk = 0x00010000; // Version? Must be 0x00010000
        public int archiveSize;
        public int fileInfoOffset;
        public int fileInfoSize;
        public uint unk2 = 0x00010000; // Chunk max size?
        public int filesystemEntries;
        public int folders;
        public int files;
        public int chunks;
        public int stringTableSize;
        public int unk4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("{filename}")]
    class L7CAFilesystemEntry
    {
        public int id;
        public uint hash; // Hash of filename
        public int folderNameOffset;
        public int fileNameOffset;
        public long timestamp;

        public string filename;

        public L7CAFilesystemEntry(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                id = br.ReadInt32();
                hash = br.ReadUInt32();
                folderNameOffset = br.ReadInt32();
                fileNameOffset = br.ReadInt32();
                timestamp = br.ReadInt64();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class L7CAFileEntry
    {
        public int compressedFilesize;
        public int rawFilesize;
        public int chunkIdx;
        public int chunkCount;
        public int offset;
        public uint crc32;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class L7CAChunkEntry
    {
        public int chunkSize;
        public ushort unk = 0;
        public ushort chunkId;
    }
}
