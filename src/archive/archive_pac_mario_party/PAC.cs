using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Text;

namespace archive_pac_mario_party
{
    public sealed class PAC
    {
        public List<PacFileInfo> Files = new List<PacFileInfo>();
        private Stream _stream = null;

        Header header;

        const int assetSize = 0x10;
        const int entrySize = 0x30;
        const int align = 0x40;

        public PAC(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true, ByteOrder.BigEndian))
            {
                header = br.ReadStruct<Header>();

                br.BaseStream.Position = header.assetOffset;
                var assets = br.ReadMultiple<AssetEntry>(header.assetCount);

                br.BaseStream.Position = header.entryOffset;
                var entries = br.ReadMultiple<Entry>(header.entryCount);

                br.BaseStream.Position = header.stringOffset;
                var strings = br.ReadBytes(header.dataOffset - header.stringOffset);

                CheckCounts(assets, entries);

                using (var sr = new BinaryReaderX(new MemoryStream(strings)))
                    foreach (var asset in assets)
                    {
                        sr.BaseStream.Position = asset.stringOffset - header.stringOffset;
                        var assetName = sr.ReadCStringA();
                        for (int i = (asset.entryOffset - header.entryOffset) / entrySize; i < ((asset.entryOffset - header.entryOffset) / entrySize) + asset.count; i++)
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

        private void CheckCounts(List<AssetEntry> assets, List<Entry> entries)
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
            //Sort out doubled files and connect ArchiveFileInfo by ID
            var connectedID = 0;
            while (Files.Count(f => f.connectedID == -1) > 0)
            {
                var filesToCheck = Files.Where(f => f.connectedID == -1);
                if (filesToCheck.Count() == 1)
                {
                    filesToCheck.ElementAt(0).connectedID = connectedID;
                    continue;
                }

                var file = filesToCheck.ElementAt(0);
                foreach (var fileInList in filesToCheck.Where((f, i) => i > 0))
                {
                    file.Equals(fileInList, connectedID);
                }

                connectedID++;
            }

            //Files into assets
            var assets = new List<(string, IEnumerable<PacFileInfo>)>();
            foreach (var assetName in Files.Select(f => f.FileName.Split('\\').First()).Distinct())
                assets.Add((assetName, Files.Where(f => f.FileName.Split('\\').First() == assetName)));

            //Create distinct stringList + update entries
            Dictionary<string, int> distinctStrings = new Dictionary<string, int>();
            var offset = 0;
            foreach (var file in Files)
            {
                var fileName = file.FileName.Split('\\').Last();
                file.entry.unk1 = Kontract.Hash.FNV.FNV132.Create(fileName);
                if (!distinctStrings.ContainsKey(fileName))
                {
                    distinctStrings.Add(fileName, offset);
                    file.entry.stringOffset = offset + header.stringOffset;
                    offset += Encoding.ASCII.GetByteCount(fileName) + 1;
                }
                else
                {
                    file.entry.stringOffset = distinctStrings[fileName] + header.stringOffset;
                }
            }

            foreach (var asset in assets)
            {
                if (!distinctStrings.ContainsKey(asset.Item1))
                {
                    distinctStrings.Add(asset.Item1, offset);
                    offset += Encoding.ASCII.GetByteCount(asset.Item1) + 1;
                }

                foreach (var file in asset.Item2)
                {
                    if (!distinctStrings.ContainsKey(Path.GetExtension(file.FileName)))
                    {
                        distinctStrings.Add(Path.GetExtension(file.FileName), offset);
                        file.entry.extOffset = offset + header.stringOffset;
                        offset += Encoding.ASCII.GetByteCount(Path.GetExtension(file.FileName)) + 1;
                    }
                    else
                    {
                        file.entry.extOffset = distinctStrings[Path.GetExtension(file.FileName)] + header.stringOffset;
                    }
                }
            }

            //Write stuff
            using (var bw = new BinaryWriterX(input, ByteOrder.BigEndian))
            {
                //Header
                header.assetCount = assets.Count;
                header.entryCount = Files.Count;
                header.fileDataCount = Files.Select(f => f.connectedID).Distinct().Count();
                header.stringCount = Files.Select(f => f.FileName.Split('\\').Last()).Distinct().Count() + assets.Count + Files.Select(f => Path.GetExtension(f.FileName)).Distinct().Count();

                header.entryOffset = (header.assetOffset + assets.Count * assetSize + (align - 1)) & ~(align - 1);
                header.stringOffset = (header.entryOffset + Files.Count * entrySize + (align - 1)) & ~(align - 1);
                header.fileDataOffset = (header.stringOffset + distinctStrings.OrderByDescending(ds => ds.Value).First().Value +
                    Encoding.ASCII.GetByteCount(distinctStrings.OrderByDescending(ds => ds.Value).First().Key) + 1 + (align - 1)) & ~(align - 1);

                //Assets
                bw.BaseStream.Position = header.assetOffset;
                var entryOffset = header.entryOffset;
                foreach (var asset in assets)
                {
                    bw.WriteStruct(new AssetEntry
                    {
                        stringOffset = distinctStrings[asset.Item1] + header.stringOffset,
                        unk1 = Kontract.Hash.FNV.FNV132.Create(asset.Item1),
                        count = asset.Item2.Count(),
                        entryOffset = entryOffset
                    });
                    entryOffset += asset.Item2.Count() * entrySize;
                }

                //Strings
                foreach (var str in distinctStrings)
                {
                    bw.BaseStream.Position = header.stringOffset + str.Value;
                    bw.WriteASCII(str.Key);
                    bw.Write((byte)0);
                }

                //Files
                bw.BaseStream.Position = header.dataOffset;
                foreach (var asset in assets)
                    foreach (var fileInfo in asset.Item2)
                        if (fileInfo.connectedID != -1)
                        {
                            var cid = fileInfo.connectedID;
                            fileInfo.Write(bw.BaseStream);
                            foreach (var file in Files.Where(f => f.connectedID == fileInfo.connectedID))
                                file.Update(fileInfo.entry);
                        }

                //Entries
                bw.BaseStream.Position = header.entryOffset;
                foreach (var asset in assets)
                    foreach (var fileInfo in asset.Item2)
                        bw.WriteStruct(fileInfo.entry);

                //Header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }

            //Reset connections for further save operations
            foreach (var file in Files)
                file.connectedID = -1;
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
