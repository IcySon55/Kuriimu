using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.Interface;
using Kontract.IO;

namespace archive_sony
{
    public sealed class PSARC
    {
        public List<PsarcFileInfo> Files = new List<PsarcFileInfo>();

        private const string ManifestName = "/psarc.manifest";
        public const ushort ZLibHeader = 0x78DA;
        //public const ushort LzmaHeader = 0x????;
        public const ushort SdatHeaderA = 0x0001;
        public const ushort SdatHeaderB = 0x0002;

        private Stream _stream;

        public Header Header;
        public int BlockLength = 1;
        public bool SdatEncryptedManifest;

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
                    var index = br.ReadInt32();
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
                            FirstBlockIndex = index,
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
                var blockSizes = new List<uint>();
                for (var i = 0; i < numBlocks; i++)
                    blockSizes.Add((uint)br.ReadBytes(BlockLength).Reverse().Select((x, j) => x << 8 * j).Sum());

                // Check for SDAT Encryption
                if (Files.Count > 0)
                {
                    br.BaseStream.Position = Files[0].Entry.Offset;
                    var compression = br.ReadUInt16();
                    br.BaseStream.Position -= 2;
                    SdatEncryptedManifest = compression == SdatHeaderA;
                }

                // Files
                for (var i = 0; i < Header.TocEntryCount; i++)
                {
                    Files[i].BlockSize = Header.BlockSize;
                    Files[i].BlockSizes = blockSizes;
                    Files[i].FileData = br.BaseStream;
                }

                // Load Filenames
                if (!SdatEncryptedManifest)
                {
                    using (var brNames = new StreamReader(Files[0].FileData, Encoding.UTF8))
                        for (var i = 1; i < Header.TocEntryCount; i++)
                            Files[i].FileName = brNames.ReadLine() ?? Files[i].FileName;
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
