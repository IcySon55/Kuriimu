using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace archive_sony
{
    public sealed class PSAR
    {
        public List<PsarcFileInfo> Files = new List<PsarcFileInfo>();

        private const string ManifestName = "/psarc.manifest";
        private const ushort ZLibHeader = 0x78DA;
        private const ushort SdatHeader = 0x0001;

        private Stream _stream;

        public Header Header;
        public int BlockType = 1;
        public bool SdatEncrypted;

        public PSAR(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true, ByteOrder.BigEndian))
            {
                Header = br.ReadStruct<Header>();

                // Determine BlockType
                uint j = 256;
                do
                {
                    j *= 256;
                    BlockType = (ushort)(BlockType + 1);
                } while (j < Header.BlockSize);

                // Entries
                for (var i = 0; i < Header.TocEntryCount; i++)
                {
                    var md5Hash = br.ReadBytes(16);
                    var index = br.ReadUInt32();
                    var length = br.ReadUInt64() >> 24;
                    br.BaseStream.Position -= 6;
                    var offset = br.ReadUInt64() & 0xFFFFFFFFFF;

                    Files.Add(new PsarcFileInfo
                    {
                        ID = i,
                        Entry = new Entry
                        {
                            MD5Hash = md5Hash,
                            Index = index,
                            Size = (long)length,
                            Offset = (long)offset
                        }
                    });
                }

                // Manifest Filename
                if (Files.Count > 0)
                    Files[0].FileName = ManifestName;

                // Blocks
                var numBlocks = (Header.TocSize - (int)br.BaseStream.Position) / BlockType;
                var blocks = new List<uint>();
                for (var i = 0; i < numBlocks; i++)
                    blocks.Add(BitConverter.ToUInt32(br.ReadBytes(BlockType).ToArray(), 0));

                // Manifest
                ReadBlocks(br, 0, blocks);

                // Load Filenames
                if (!SdatEncrypted)
                {
                    using (var brNames = new StreamReader(Files[0].FileData))
                        for (var i = 1; i < Header.TocEntryCount; i++)
                            Files[i].FileName = brNames.ReadLine();
                }
                else
                {
                    for (var i = 1; i < Header.TocEntryCount; i++)
                        Files[i].FileName = "/" + i.ToString("00000000") + ".bin";
                }
            }
        }

        public void ReadBlocks(BinaryReaderX br, int entryIndex, List<uint> blocks)
        {
            var entry = Files[entryIndex].Entry;
            if (entry.Size == 0) return;

            br.BaseStream.Position = entry.Offset;
            var index = entry.Index;
            long subSize = 0;

            // Check for SDAT Encryption
            if (entryIndex == 0)
            {
                var header = br.ReadUInt16();
                SdatEncrypted = header == SdatHeader;
            }
            if (SdatEncrypted) return;

            do
            {
                if (blocks[(int)index] == 0)
                {
                    subSize += Header.BlockSize;
                }
                else
                {
                    var compression = br.ReadUInt16();
                    br.BaseStream.Position -= 2;

                    if (compression == ZLibHeader)
                    {
                        var size = entry.Size - (index - entry.Index) * Header.BlockSize;
                        subSize += size < Header.BlockSize ? size : Header.BlockSize;
                    }
                    else
                    {
                        subSize += (int)blocks[(int)index];
                    }
                }
                index++;
            } while (subSize < entry.Size);

            // Set FileData
            Files[entryIndex].FileData = new SubStream(br.BaseStream, entry.Offset, subSize);
        }

        public void Save(Stream output)
        {
            // TODO: Saving... some day.
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
