using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_pac_mario_party
{
    public sealed class PAC
    {
        public List<PacFileInfo> Files = new List<PacFileInfo>();
        private Stream _stream = null;

        Header header;
        List<AssetEntry> assets;
        List<Entry> entries;
        byte[] strings;

        const int assetSize = 0x30;

        public PAC(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true, ByteOrder.BigEndian))
            {
                header = br.ReadStruct<Header>();

                br.BaseStream.Position = header.assetOffset;
                assets = br.ReadMultiple<AssetEntry>(header.assetCount);

                br.BaseStream.Position = header.entryOffset;
                entries = br.ReadMultiple<Entry>(header.entryCount);

                br.BaseStream.Position = header.stringOffset;
                strings = br.ReadBytes(header.dataOffset - header.stringOffset);

                CheckCounts();

                using (var sr = new BinaryReaderX(new MemoryStream(strings)))
                    foreach (var asset in assets)
                    {
                        sr.BaseStream.Position = asset.stringOffset - header.stringOffset;
                        var assetName = sr.ReadCStringA();
                        for (int i = (asset.entryOffset - header.entryOffset) / assetSize; i < ((asset.entryOffset - header.entryOffset) / assetSize) +asset.count; i++)
                        {
                            sr.BaseStream.Position = entries[i].stringOffset - header.stringOffset;
                            var entryName = sr.ReadCStringA();
                            Files.Add(new PacFileInfo
                            {
                                State = ArchiveFileState.Archived,
                                FileName = Path.Combine(assetName, entryName),
                                FileData = new SubStream(br.BaseStream, entries[i].dataOffset, entries[i].compSize),
                                entry = entries[i]
                            });
                        }
                    }

                br.BaseStream.Position = header.dataOffset;
            }
        }

        private void CheckCounts()
        {
            if (assets.Count != header.assetCount)
                throw new Exception("Asset count doesn't match.");

            if (entries.Count != header.entryCount)
                throw new Exception("Entry count doesn't match.");

            var tmp = entries.Select(e => e.stringOffset).ToList();
            tmp.AddRange(entries.Select(e => e.extOffset));
            tmp.AddRange(assets.Select(a => a.stringOffset));
            tmp = tmp.Distinct().ToList();
            if (tmp.Count != header.stringCount)
                throw new Exception("String count doesn't match.");

            if (entries.Select(e => e.dataOffset).Distinct().Count() != header.fileDataCount)
                throw new Exception("Filedata count doesn't match.");
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {

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
