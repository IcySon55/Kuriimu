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
        //private const ushort LzmaHeader = 0x????;
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
                            Files[i].FileName = brNames.ReadLine() ?? "/" + i.ToString("00000000") + ".bin";
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
            var afi = Files[entryIndex];
            var entry = Files[entryIndex].Entry;
            if (entry.Size == 0) return;

            br.BaseStream.Position = entry.Offset;
            var index = entry.Index;
            long size = 0;

            // Check for SDAT Encryption
            if (entryIndex == 0)
            {
                var compression = br.ReadUInt16();
                br.BaseStream.Position -= 2;
                SdatEncrypted = compression == SdatHeader;
            }

            do
            {
                if (blocks[(int)index] == 0)
                {
                    size += Header.BlockSize;
                    afi.Blocks.Add(new Block
                    {
                        Size = Header.BlockSize,
                        Compression = Compression.None
                    });
                }
                else
                {
                    var compression = br.ReadUInt16();
                    br.BaseStream.Position -= 2;

                    var localIndex = index - entry.Index;

                    if (compression == ZLibHeader)
                    {
                        var blockSize = entry.Size - localIndex * Header.BlockSize;
                        size += blockSize < Header.BlockSize ? blockSize : Header.BlockSize;
                        afi.Blocks.Add(new Block
                        {
                            Size = blockSize < Header.BlockSize ? blockSize : Header.BlockSize,
                            Compression = Compression.ZLib
                        });
                    }
                    else if (compression == SdatHeader)
                    {
                        var blockSize = entry.Size - localIndex * Header.BlockSize;
                        size += blockSize < Header.BlockSize ? blockSize : Header.BlockSize;
                        afi.Blocks.Add(new Block
                        {
                            Size = blockSize < Header.BlockSize ? blockSize : Header.BlockSize,
                            Compression = Compression.Sdat
                        });
                    }
                    else
                    {
                        size += blocks[(int)index];
                        afi.Blocks.Add(new Block
                        {
                            Size = blocks[(int)index],
                            Compression = Compression.None
                        });
                    }
                }
                index++;
            } while (size < entry.Size);

            // Set FileData - Math.Min is a temporary fix for the last file just so that the UI can load the manifest.
            afi.FileData = new SubStream(br.BaseStream, entry.Offset, Math.Min(size, br.BaseStream.Length - entry.Offset));
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
