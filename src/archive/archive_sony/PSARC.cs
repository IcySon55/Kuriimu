using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Kontract.Interface;
using Kontract.IO;

namespace archive_sony.PSARC
{
    public sealed class PSARC
    {
        public List<PsarcFileInfo> Files = new List<PsarcFileInfo>();

        private const string ManifestName = "/psarc.manifest";
        private const int HeaderSize = 0x20;

        public const ushort ZLibHeader = 0x78DA;
        //public const ushort LzmaHeader = 0x????;
        public const ushort AllStarsEncryptionA = 0x0001;
        public const ushort AllStarsEncryptionB = 0x0002;

        private Stream _stream;

        public Header Header;
        private int BlockLength = 1;
        private List<int> CompressedBlockSizes = new List<int>();
        public bool AllStarsEncryptedArchive;

        public PSARC(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true, ByteOrder.BigEndian))
            {
                Header = br.ReadStruct<Header>();

                // Determine BlockLength
                uint blockIterator = 256;
                do
                {
                    blockIterator *= 256;
                    BlockLength = (ushort)(BlockLength + 1);
                } while (blockIterator < Header.BlockSize);

                // Entries
                for (var i = 0; i < Header.TocEntryCount; i++)
                {
                    var md5Hash = br.ReadBytes(16);
                    var firstBlockIndex = br.ReadInt32();
                    var length = br.ReadUInt64() >> 24;
                    br.BaseStream.Position -= 6;
                    var offset = br.ReadUInt64() & 0xFFFFFFFFFF;

                    Files.Add(new PsarcFileInfo
                    {
                        ID = i,
                        FileName = "/" + i.ToString("00000000") + ".bin",
                        Entry = new Entry
                        {
                            MD5Hash = md5Hash,
                            FirstBlockIndex = firstBlockIndex,
                            UncompressedSize = (long)length,
                            Offset = (long)offset
                        },
                        State = ArchiveFileState.Archived
                    });
                }

                // Manifest Filename
                if (Files.Count > 0)
                    Files[0].FileName = ManifestName;

                // Blocks
                var numBlocks = (Header.TocSize - (int)br.BaseStream.Position) / BlockLength;
                for (var i = 0; i < numBlocks; i++)
                    CompressedBlockSizes.Add(br.ReadBytes(BlockLength).Reverse().Select((x, j) => x << 8 * j).Sum());

                // Check for SDAT Encryption
                if (Files.Count > 0)
                {
                    br.BaseStream.Position = Files[0].Entry.Offset;
                    var compression = br.ReadUInt16();
                    br.BaseStream.Position -= 2;
                    AllStarsEncryptedArchive = compression == AllStarsEncryptionA || compression == AllStarsEncryptionB;
                }

                // Files
                for (var i = 0; i < Header.TocEntryCount; i++)
                {
                    Files[i].BlockSize = Header.BlockSize;
                    Files[i].BlockSizes = CompressedBlockSizes;
                    Files[i].FileData = br.BaseStream;
                }

                // Load Filenames
                if (!AllStarsEncryptedArchive)
                {
                    using (var brNames = new BinaryReaderX(Files[0].FileData, Encoding.UTF8))
                        for (var i = 1; i < Header.TocEntryCount; i++)
                            Files[i].FileName = Encoding.UTF8.GetString(brNames.ReadBytesUntil(0x0, (byte)'\n')) ?? Files[i].FileName;
                }
                else
                {
                    for (var i = 1; i < Header.TocEntryCount; i++)
                        Files[i].FileName = "/" + i.ToString("00000000") + ".bin";
                }
            }
        }

        public void Save(Stream output)
        {
            // TODO: Saving... today.

            using (var bw = new BinaryWriterX(output, ByteOrder.BigEndian))
            {
                // Create Manifest
                var filePaths = new List<string>();
                for (var i = 1; i < Header.TocEntryCount; i++)
                {
                    var afi = Files[i];
                    switch (Header.ArchiveFlags)
                    {
                        case ArchiveFlags.RelativePaths:
                            filePaths.Add(afi.FileName.TrimStart('/'));
                            break;
                        case ArchiveFlags.IgnoreCasePaths:
                        case ArchiveFlags.AbsolutePaths:
                            filePaths.Add(afi.FileName);
                            break;
                    }
                }
                var manifest = new MemoryStream(Encoding.ASCII.GetBytes(string.Join("\n", filePaths)));

                // Update Block Count and Size
                var compressedBlocksOffset = HeaderSize + Header.TocEntryCount * Header.TocEntrySize;
                var compressedBlockCount = 0;
                foreach (var afi in Files)
                {
                    switch (afi.State)
                    {
                        case ArchiveFileState.Archived:
                        case ArchiveFileState.Renamed:
                            compressedBlockCount += (int)Math.Ceiling((double)afi.Entry.UncompressedSize / Header.BlockSize);
                            break;
                        case ArchiveFileState.Added:
                        case ArchiveFileState.Replaced:
                            compressedBlockCount += (int)Math.Ceiling((double)afi.FileData.Length / Header.BlockSize);
                            break;
                        case ArchiveFileState.Empty:
                        case ArchiveFileState.Deleted:
                            break;
                    }
                }
                bw.BaseStream.Position = Header.TocSize = compressedBlocksOffset + compressedBlockCount * BlockLength;

                // Writing Files
                var compressedBlocks = new List<int>();
                var lastPosition = bw.BaseStream.Position;

                // Write Generated Manifest File
                WriteFile(bw, Files[0], null, compressedBlocks, ref lastPosition);

                // Write All Other Files
                for (var i = 1; i < Header.TocEntryCount; i++)
                    WriteFile(bw, Files[i], null, compressedBlocks, ref lastPosition);

                // Write Updated Entries
                bw.BaseStream.Position = HeaderSize;
                foreach (var entry in Files.Select(e => e.Entry))
                {
                    bw.Write(entry.MD5Hash);
                    bw.Write((uint)entry.FirstBlockIndex);
                    bw.Write(BitConverter.GetBytes(entry.UncompressedSize).Take(5).Reverse().ToArray());
                    bw.Write(BitConverter.GetBytes(entry.Offset).Take(5).Reverse().ToArray());
                }

                // Write Updated Compressed Blocks
                foreach (var block in compressedBlocks)
                    bw.Write(BitConverter.GetBytes((uint)block).Take(BlockLength).Reverse().ToArray());

                // Header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(Header);
            }
        }

        private void WriteFile(BinaryWriterX bw, PsarcFileInfo afi, Stream @override, List<int> compressedBlocks, ref long lastPosition)
        {
            var entry = afi.Entry;

            if (afi.State == ArchiveFileState.Archived && @override == null)
            {
                var originalBlockIndex = entry.FirstBlockIndex;
                var originalOffset = entry.Offset;

                // Update Entry
                entry.FirstBlockIndex = compressedBlocks.Count;
                entry.Offset = lastPosition;

                // Write File Chunks and Add Blocks
                using (var br = new BinaryReaderX(afi.BaseFileData, true))
                {
                    br.BaseStream.Position = originalOffset;
                    for (var i = originalBlockIndex; i < originalBlockIndex + Math.Ceiling((double)entry.UncompressedSize / Header.BlockSize); i++)
                    {
                        if (CompressedBlockSizes[i] == 0)
                            bw.Write(br.ReadBytes(Header.BlockSize));
                        else
                            bw.Write(br.ReadBytes(CompressedBlockSizes[i]));
                        compressedBlocks.Add(CompressedBlockSizes[i]);
                    }
                    lastPosition = bw.BaseStream.Position;
                }
            }
            else
            {
                var input = @override ?? afi.FileData;

                // Update Entry
                entry.UncompressedSize = (int)input.Length;
                entry.FirstBlockIndex = compressedBlocks.Count;
                entry.Offset = lastPosition;

                // Write File Chunks and Add Blocks
                using (var br = new BinaryReaderX(input, true))
                    for (var i = 0; i < Math.Ceiling((double)input.Length / Header.BlockSize); i++)
                    {
                        if (Header.Compression == "zlib")
                        {
                            bw.Write(ZLibHeader);
                            var readLength = (int)Math.Min(Header.BlockSize, br.BaseStream.Length - (Header.BlockSize * i));
                            using (var ds = new DeflateStream(bw.BaseStream, CompressionLevel.Optimal, true))
                                ds.Write(br.ReadBytes(readLength), 0, readLength);
                        }
                        else if (Header.Compression == "lzma")
                        {
                            // TODO: Implement LZMA support when we find a file that uses it.
                        }

                        compressedBlocks.Add((int)(bw.BaseStream.Position - lastPosition));
                        lastPosition = bw.BaseStream.Position;
                    }
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
