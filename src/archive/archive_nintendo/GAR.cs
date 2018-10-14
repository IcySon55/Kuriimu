using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.GAR
{
    public sealed class GAR
    {
        public List<GARFileInfo> Files = new List<GARFileInfo>();
        Stream _stream = null;

        public Header _header;

        public List<ChunkEntry> entries = new List<ChunkEntry>();
        public List<List<int>> chunkFileIDs = new List<List<int>>();
        public List<string> chunkNames = new List<string>();
        public List<ChunkInfo> chunkInfos = new List<ChunkInfo>();
        public List<uint> offsets = new List<uint>();

        public List<SystemChunkEntry> sysEntries = new List<SystemChunkEntry>();
        public byte[] sysEntriesSubTable;
        public List<SystemChunkInfo> sysChunkInfos = new List<SystemChunkInfo>();

        public GAR(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                _header = br.ReadStruct<Header>();

                switch (_header.hold0)
                {
                    case "jenkins\0":
                        ParseZeldaGar(br);
                        break;

                    case "SYSTEM\0\0":
                        ParseSystemGar(br);
                        break;
                }
            }
        }

        private void ParseZeldaGar(BinaryReaderX br)
        {
            //FileEntries
            entries = br.ReadMultiple<ChunkEntry>(_header.fileChunks);

            //chunkIDs
            foreach (var chunk in entries)
            {
                chunkFileIDs.Add(new List<int>());
                chunkFileIDs.Last().AddRange(br.ReadMultiple<int>(chunk.fileCount));
                chunkNames.Add(br.ReadCStringA());
            }

            //chunkInfos
            br.BaseStream.Position = _header.chunkInfOffset;
            foreach (var chunk in entries)
            {
                for (int i = 0; i < chunk.fileCount; i++)
                    chunkInfos.Add(new ChunkInfo(br.BaseStream));
            }

            //fileData
            br.BaseStream.Position = _header.offset3;
            offsets.Add(br.ReadUInt32());
            while (br.BaseStream.Position < offsets[0])
                offsets.Add(br.ReadUInt32());

            int offsetID = 0;
            foreach (var chunkInfo in chunkInfos)
            {
                Files.Add(new GARFileInfo
                {
                    State = ArchiveFileState.Archived,
                    FileName = chunkInfo.fileName,
                    FileData = new SubStream(br.BaseStream, offsets[offsetID++], chunkInfo.fileSize),
                    ext = Path.GetExtension(chunkInfo.fileName)
                });
            }
        }

        private void ParseSystemGar(BinaryReaderX br)
        {
            //Chunk Entries
            for (int i = 0; i < _header.fileChunks; i++)
            {
                sysEntries.Add(new SystemChunkEntry(_stream));
                br.SeekAlignment(0x20);
            }

            //SubTable - unknown
            br.BaseStream.Position = sysEntries.Min(x => x.subTableOffset);
            sysEntriesSubTable = br.ReadBytes(_header.chunkInfOffset - (int)br.BaseStream.Position);

            //chunk Infos
            br.BaseStream.Position = _header.chunkInfOffset;
            foreach (var chunk in sysEntries)
            {
                for (int i = 0; i < chunk.fileCount; i++)
                {
                    sysChunkInfos.Add(new SystemChunkInfo(br.BaseStream));
                    sysChunkInfos.Last().ext = chunk.name;
                }
            }

            //File Data
            br.BaseStream.Position = _header.offset3;
            Files = sysChunkInfos.Select(x =>
                new GARFileInfo
                {
                    State = ArchiveFileState.Archived,
                    FileName = x.name + "." + x.ext,
                    FileData = new SubStream(_stream, x.fileOffset, x.fileSize),
                    ext = x.ext
                }).ToList();
        }

        public void Save(Stream output)
        {
            switch (_header.hold0)
            {
                case "jenkins\0":
                    SaveZeldaGar(output);
                    break;

                case "SYSTEM\0\0":
                    SaveSystemGar(output);
                    break;
            }
        }

        private void SaveZeldaGar(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {
                var files = Files.OrderBy(x => x.ext.Length).ToList();

                //get Extension and their count
                List<string> exts = new List<string>();
                List<int> extsCount = new List<int>();
                foreach (var file in files)
                    if (!exts.Contains(Path.GetExtension(file.FileName).Split('.')[1]))
                    {
                        exts.Add(Path.GetExtension(file.FileName).Split('.')[1]);
                        extsCount.Add(1);
                    }
                    else
                    {
                        extsCount[exts.FindIndex(x => x == Path.GetExtension(file.FileName).Split('.')[1])]++;
                    }

                //get offsets
                int chunkIDOffset = 0x20 + (exts.Count + 1) * 0x10;
                int chunkInfOffset = chunkIDOffset + 8 + files.Count * 0x4;
                for (int i = 0; i < exts.Count; i++)
                {
                    chunkInfOffset += (exts[i].Length + 1);
                    while (chunkInfOffset % 4 != 0) chunkInfOffset++;
                }
                int nameOffset = chunkInfOffset + files.Count * 0xc;
                int offListOffset = nameOffset;
                for (int i = 0; i < files.Count; i++)
                {
                    offListOffset += files[i].FileName.Length + 1;
                    offListOffset += files[i].FileName.Split('.')[0].Length + 1;
                    while (offListOffset % 4 != 0) offListOffset++;
                }
                int dataOffset = offListOffset + files.Count * 0x4;

                bw.BaseStream.Position = 0x20;

                //write chunkEntries
                int tmp = chunkIDOffset;
                for (int i = 0; i <= exts.Count; i++)
                    if (i == 0)
                    {
                        bw.Write(0);
                        bw.Write(0xFFFFFFFF);
                        bw.Write(tmp);
                        bw.Write(0xFFFFFFFF);
                        tmp += 8;
                    }
                    else
                    {
                        bw.Write(extsCount[i - 1]);
                        bw.Write(tmp);
                        bw.Write(tmp + extsCount[i - 1] * 4);
                        bw.Write(0xFFFFFFFF);

                        var padding = 0;
                        while ((exts[i - 1].Length + 1 + padding) % 4 != 0) padding++;
                        tmp += extsCount[i - 1] * 4 + exts[i - 1].Length + 1 + padding;
                    }

                //write chunkIDs and magics
                int id = 0;
                for (int i = 0; i <= exts.Count; i++)
                {
                    if (i == 0)
                    {
                        bw.WriteASCII("unknown");
                        bw.Write((byte)0);
                    }
                    else
                    {
                        for (int j = 0; j < extsCount[i - 1]; j++)
                            bw.Write(id++);

                        bw.WriteASCII(exts[i - 1]);
                        bw.Write((byte)0);
                        while (bw.BaseStream.Position % 4 != 0) bw.BaseStream.Position++;
                    }
                }

                //write chunkInfos
                tmp = nameOffset;
                foreach (var file in files)
                {
                    bw.Write((uint)file.FileSize);
                    bw.Write(tmp + file.FileName.Length + 1);
                    bw.Write(tmp);
                    tmp += file.FileName.Length + 1 + file.FileName.Split('.')[0].Length + 1;
                    while (tmp % 4 != 0) tmp++;
                }

                //write names
                foreach (var file in files)
                {
                    bw.WriteASCII(file.FileName);
                    bw.Write((byte)0);
                    bw.WriteASCII(file.FileName.Split('.')[0]);
                    bw.Write((byte)0);
                    while (bw.BaseStream.Position % 4 != 0) bw.BaseStream.Position++;
                }

                //write offsets
                tmp = dataOffset;
                foreach (var file in files)
                {
                    bw.Write(tmp);
                    tmp += (int)file.FileSize;
                }

                //write fileData
                foreach (var file in files)
                {
                    file.FileData.CopyTo(bw.BaseStream);
                }

                //Header
                bw.BaseStream.Position = 0;
                _header.fileSize = (uint)bw.BaseStream.Length;
                _header.fileChunks = (short)(exts.Count + 1);
                _header.fileCount = (short)files.Count;
                _header.chunkInfOffset = chunkInfOffset;
                _header.offset3 = offListOffset;
                bw.WriteStruct(_header);
            }
        }

        private void SaveSystemGar(Stream input)
        {
            int Align(int value, int align) => value + (align - 1) & ~(align - 1);

            using (var bw = new BinaryWriterX(input))
            {
                var files = Files.OrderBy(x => x.ext.Length).ToList();

                //get Extension and their count
                Dictionary<string, int> exts = new Dictionary<string, int>();
                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file.FileName).Replace(".", "");
                    if (!exts.ContainsKey(ext))
                        exts.Add(ext, 1);
                    else
                        exts[ext]++;
                }

                //get offsets
                int chunkExtNameOffset = _header.chunkEntryOffset + (exts.Count + 1) * 0x20;
                int chunkSubTableOffset = Align(chunkExtNameOffset + Encoding.ASCII.GetByteCount("unknown\0") + exts.Aggregate(0, (a, b) => a + Encoding.ASCII.GetByteCount(b.Key) + 1), 4);
                int chunkInfoOffset = chunkSubTableOffset + sysEntriesSubTable.Length;
                int chunkInfoNameOffset = chunkInfoOffset + Files.Count * 0x10;
                int dataOffset = Align(chunkInfoNameOffset + Files.Aggregate(0, (a, b) => a + Encoding.ASCII.GetByteCount(Path.GetFileName(b.FileName)) + 1), 0x80);

                bw.BaseStream.Position = 0x20;

                //Write chunkEntries
                int localChunkNameOffset = chunkExtNameOffset;

                //Add "unknown" chunk Entry
                bw.Write(0);
                bw.Write(0x4);
                bw.Write(0xFFFFFFFF);
                bw.Write(localChunkNameOffset);
                bw.Write(0xFFFFFFFF);
                bw.BaseStream.Position += 0xC;

                localChunkNameOffset += Encoding.ASCII.GetByteCount("unknown\0");

                //Add all other chunk entries
                int filesAdded = 0;
                foreach (var ext in exts)
                {
                    bw.Write(ext.Value);
                    bw.Write(sysEntries.FirstOrDefault(x => x.name == ext.Key)?.unk1 ?? 0x4);
                    bw.Write(filesAdded);
                    bw.Write(localChunkNameOffset);
                    bw.Write(sysEntries.FirstOrDefault(x => x.name == ext.Key)?.subTableOffset ?? 0x0);
                    bw.BaseStream.Position += 0xC;

                    filesAdded += ext.Value;
                    localChunkNameOffset += Encoding.ASCII.GetByteCount(ext.Key) + 1;
                }

                //Add chunk Extensions
                bw.WriteASCII("unknown\0");
                foreach (var ext in exts)
                    bw.WriteASCII(ext.Key + "\0");
                bw.WriteAlignment(4);

                //Add subtable
                bw.Write(sysEntriesSubTable);

                //Write chunkInfos and Files
                var localChunkInfoOffset = chunkInfoOffset;
                var localChunkInfoNameOffset = chunkInfoNameOffset;
                var localDataOffset = dataOffset;
                foreach (var ext in exts)
                {
                    var filesToAdd = Files.Where(x => x.ext == ext.Key);
                    foreach (var toAdd in filesToAdd)
                    {
                        bw.BaseStream.Position = localChunkInfoOffset;
                        bw.Write((int)toAdd.FileSize);
                        bw.Write(localDataOffset);
                        bw.Write(localChunkInfoNameOffset);
                        bw.Write(0xFFFFFFFF);

                        bw.BaseStream.Position = localChunkInfoNameOffset;
                        bw.WriteASCII(Path.GetFileNameWithoutExtension(toAdd.FileName) + "\0");

                        bw.BaseStream.Position = localDataOffset;
                        toAdd.FileData.CopyTo(bw.BaseStream);

                        localDataOffset += (int)toAdd.FileSize;
                        localChunkInfoNameOffset += Encoding.ASCII.GetByteCount(Path.GetFileNameWithoutExtension(toAdd.FileName)) + 1;
                        localChunkInfoOffset += 0x10;
                    }
                }

                //Header
                bw.BaseStream.Position = 0;
                _header.fileSize = (uint)bw.BaseStream.Length;
                _header.fileChunks = (short)(exts.Count + 1);
                _header.fileCount = (short)files.Count;
                _header.chunkInfOffset = chunkInfoOffset;
                _header.offset3 = dataOffset;
                bw.WriteStruct(_header);
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
