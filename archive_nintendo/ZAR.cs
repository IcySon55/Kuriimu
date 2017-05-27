using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_nintendo.ZAR
{
    public sealed class ZAR
    {
        public List<ZARFileInfo> Files = new List<ZARFileInfo>();
        Stream _stream = null;

        public Header header;
        public List<ChunkEntry> entries = new List<ChunkEntry>();
        public List<List<int>> chunkFileIDs = new List<List<int>>();
        public List<string> chunkNames = new List<string>();
        public List<ChunkInfo> chunkInfos = new List<ChunkInfo>();
        public List<uint> offsets = new List<uint>();

        public ZAR(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //FileEntries
                entries = br.ReadMultiple<ChunkEntry>(header.fileChunks);

                //chunkIDs
                foreach (var chunk in entries)
                {
                    chunkFileIDs.Add(new List<int>());
                    chunkFileIDs.Last().AddRange(br.ReadMultiple<int>(chunk.fileCount));
                    chunkNames.Add(br.ReadCStringA());
                }

                //chunkInfos
                br.BaseStream.Position = header.chunkInfOffset;
                foreach (var chunk in entries)
                {
                    for (int i = 0; i < chunk.fileCount; i++)
                        chunkInfos.Add(new ChunkInfo(br.BaseStream));
                }

                //fileData
                br.BaseStream.Position = header.offsetList;
                offsets.Add(br.ReadUInt32());
                while (br.BaseStream.Position < offsets[0])
                    offsets.Add(br.ReadUInt32());

                int offsetID = 0;
                foreach (var chunkInfo in chunkInfos)
                {
                    Files.Add(new ZARFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = chunkInfo.fileName,
                        FileData = new SubStream(br.BaseStream, offsets[offsetID++], chunkInfo.fileSize),
                        ext = Path.GetExtension(chunkInfo.fileName)
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var files = Files.OrderBy(x => x.ext.Length).ToList();

                //get Extension and there count
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
                for (int i=0;i<exts.Count;i++)
                {
                    chunkInfOffset += (exts[i].Length + 1);
                    while (chunkInfOffset % 4 != 0) chunkInfOffset++;
                }
                int nameOffset = chunkInfOffset + files.Count * 0x8;
                int offListOffset = nameOffset;
                for (int i=0;i<files.Count;i++)
                {
                    offListOffset += files[i].FileName.Length+1;
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
                        while ((exts[i - 1].Length + 1 + padding)%4!=0) padding++;
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
                    bw.Write(tmp);
                    tmp += file.FileName.Length+1;
                    while (tmp % 4 != 0) tmp++;
                }

                //write names
                foreach (var file in files)
                {
                    bw.WriteASCII(file.FileName);
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
                header.fileSize = (uint)bw.BaseStream.Length;
                header.fileChunks = (short)(exts.Count + 1);
                header.fileCount = (short)files.Count;
                header.chunkInfOffset = chunkInfOffset;
                header.offsetList = offListOffset;
                bw.WriteStruct(header);
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
