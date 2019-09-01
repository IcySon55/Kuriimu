using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract.IO;

namespace archive_l7c
{
    class L7cArchiveFileInfo : ArchiveFileInfo
    {
        public L7CAFileEntry Entry { get; set; }

        public override long? FileSize => Entry.rawFilesize;
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
        public int unk4 = 5; // Number of sections??
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("{filename}")]
    class L7CAFilesystemEntry
    {
        public int id;
        public uint hash; // Hash of what?
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
    class L7CAFileEntry
    {
        public int compressedFilesize;
        public int rawFilesize;
        public int chunkIdx;
        public int chunkCount;
        public int offset;
        public uint crc32;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class L7CAChunkEntry
    {
        public int chunkSize;
        public ushort unk = 0;
        public ushort chunkId;
    }
}
