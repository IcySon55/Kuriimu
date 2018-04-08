using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                uint blockIterator = 256;
                do
                {
                    blockIterator *= 256;
                    BlockType = (ushort)(BlockType + 1);
                } while (blockIterator < Header.BlockSize);

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
                        },
                        State = ArchiveFileState.Archived
                    });
                }

                // Manifest Filename
                if (Files.Count > 0)
                    Files[0].FileName = ManifestName;

                // Blocks
                var numBlocks = (Header.TocSize - (int)br.BaseStream.Position) / BlockType;
                var blocks = new List<uint>();
                for (var i = 0; i < numBlocks; i++)
                {
                    var bytes = br.ReadBytes(BlockType).ToList();
                    for (var j = 0; j < BlockType % 4; j++)
                        bytes.Add(0);
                    blocks.Add(BitConverter.ToUInt32(bytes.ToArray(), 0));
                }

                // Manifest
                ReadBlocks(br, 0, blocks);

                // Files
                for (var i = 1; i < Header.TocEntryCount; i++)
                    ReadBlocks(br, i, blocks);

                // Load Filenames
                if (!SdatEncrypted)
                {
                    using (var brNames = new StreamReader(Files[0].FileData, Encoding.UTF8))
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
                br.BaseStream.Position -= 2;
                SdatEncrypted = header == SdatHeader;
            }
            if (SdatEncrypted) return;

            do
            {
                if (blocks[(int)index] == 0)
                {
                    subSize += Header.BlockSize;
                    Files[entryIndex].Blocks.Add(new Block { Size = Header.BlockSize });
                }
                else
                {
                    var compression = br.ReadUInt16();
                    br.BaseStream.Position -= 2;

                    if (compression == ZLibHeader)
                    {
                        var size = entry.Size - (index - entry.Index) * Header.BlockSize;
                        subSize += size < Header.BlockSize ? size : Header.BlockSize;
                        Files[entryIndex].Blocks.Add(new Block
                        {
                            Size = size < Header.BlockSize ? size : Header.BlockSize,
                            Compression = Compression.ZLib
                        });
                    }
                    else
                    {
                        subSize += blocks[(int)index];
                        Files[entryIndex].Blocks.Add(new Block
                        {
                            Size = blocks[(int)index],
                            Compression = Compression.None
                        });
                    }
                }
                index++;
            } while (subSize < entry.Size);

            // Set FileData
            Files[entryIndex].FileData = new SubStream(br.BaseStream, entry.Offset, Math.Min(subSize, br.BaseStream.Length - entry.Offset));
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
