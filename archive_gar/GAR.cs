using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Contract;
using System.IO;
using Kuriimu.IO;

namespace archive_gar
{
    public sealed class GAR
    {
        public List<GARFileInfo> Files = new List<GARFileInfo>();

        public Header header;
        public List<ChunkEntry> entries = new List<ChunkEntry>();
        public List<List<int>> chunkFileIDs = new List<List<int>>();
        public List<string> chunkNames = new List<string>();
        public List<List<ChunkInfo>> chunkInfos = new List<List<ChunkInfo>>();
        public List<uint> offsets = new List<uint>();

        public GAR(Stream input)
        {
            using (var br=new BinaryReaderX(input,true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //FileEntries
                entries = br.ReadMultiple<ChunkEntry>(header.fileChunks);

                //chunkInfo
                foreach(var chunk in entries)
                {
                    chunkFileIDs.Add(new List<int>());
                    chunkFileIDs.Last().AddRange(br.ReadMultiple<int>(chunk.fileCount));
                    chunkNames.Add(br.ReadCStringA());

                    br.BaseStream.Position = chunk.chunkInfOffset+8;
                    chunkInfos.Add(new List<ChunkInfo>());
                    for (int i=0;i< chunk.fileCount;i++)
                        chunkInfos.Last().Add(new ChunkInfo(br.BaseStream));
                }

                //fileData
                br.BaseStream.Position = header.offsetList;
                offsets.Add(br.ReadUInt32());
                while (br.BaseStream.Position < offsets[0])
                    offsets.Add(br.ReadUInt32());

                int offsetID = 0;
                foreach(var chunkInfoList in chunkInfos)
                {
                    foreach(var chunkInfo in chunkInfoList)
                    {
                        Files.Add(new GARFileInfo
                        {
                            State = ArchiveFileState.Archived,
                            FileName = chunkInfo.fileName,
                            FileData = new SubStream(br.BaseStream, offsets[offsetID++], chunkInfo.fileSize),
                            ext=Path.GetExtension(chunkInfo.fileName)
                        });
                    }
                }
            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {
                var files = Files.OrderBy(x => x.ext).ToList();

                bw.BaseStream.Position = 0x20;

                //chunk Entries
                List<string> exts = new List<string>();
                List<int> extsCount = new List<int>();
                foreach (var file in files)
                    if (!exts.Contains(Path.GetExtension(file.FileName).Split('.')[1])) {
                        exts.Add(Path.GetExtension(file.FileName).Split('.')[1]);
                        extsCount.Add(1);
                    } else
                    {
                        extsCount[exts.FindIndex(x => x == Path.GetExtension(file.FileName).Split('.')[1])] ++;
                    }

                int chunkInfOffset = 0x20 + (exts.Count + 1) * 0x10;
                for (int i = 0; i <= exts.Count;i++)
                {
                    if (i == 0)
                    {
                        bw.Write(0);
                        bw.Write(0xFFFFFFFF);
                        bw.Write(chunkInfOffset);
                        bw.Write(0xFFFFFFFF);
                        chunkInfOffset += 8;
                    } else {
                        bw.Write(extsCount[i - 1]);
                        bw.Write(chunkInfOffset);
                        bw.Write(chunkInfOffset + extsCount[i - 1] * 4);
                        bw.Write(0xFFFFFFFF);
                        chunkInfOffset += extsCount[i - 1] * 4;
                    }
                }

                //chunkInfos
                int id = 0, id2=0;
                int nameOffset = (int)bw.BaseStream.Position + 8 + exts.Count * 8 + files.Count * 4 + files.Count * 0xc;
                for (int i = 0; i <= exts.Count; i++)
                {
                    if (i == 0)
                    {
                        bw.WriteASCII("unknown");
                        bw.Write((byte)0);
                    } else
                    {
                        for (int j=0;j< extsCount[i - 1];j++)
                            bw.Write(id++);

                        bw.WriteASCII(exts[i - 1]);
                        bw.Write((byte)0);
                        while (bw.BaseStream.Position % 4 != 0) bw.BaseStream.Position++;

                        for (int j = id2; j < id; j++)
                        {
                            long bk = bw.BaseStream.Position;
                            bw.BaseStream.Position = nameOffset;
                            bw.WriteASCII(files[j].FileName);
                            bw.Write((byte)0);
                            if (Path.GetDirectoryName(files[j].FileName) != "")
                                bw.WriteASCII(Path.GetDirectoryName(files[j].FileName) + "/" + Path.GetFileName(files[j].FileName).Split('.')[0]);
                            else
                                bw.WriteASCII(Path.GetFileName(files[j].FileName).Split('.')[0]);
                            bw.Write((byte)0);
                            while (bw.BaseStream.Position % 4 != 0) bw.BaseStream.Position++;
                            int tmpNameOffset = (int)bw.BaseStream.Position;
                            bw.BaseStream.Position = bk;

                            bw.Write((uint)files[j].FileSize);
                            int tmpOffset = nameOffset + files[j].FileName.Length;
                            bw.Write(nameOffset+files[j].FileName.Length+1);
                            bw.Write(nameOffset);

                            nameOffset += (tmpNameOffset-nameOffset);
                        }
                        id2 = id;
                    }
                }

                //offsetList
                bw.BaseStream.Position = nameOffset;
                int tmpOfflistOffset2=nameOffset+ files.Count * 4;
                foreach (var file in files)
                {
                    bw.Write(tmpOfflistOffset2);
                    tmpOfflistOffset2 += (int)file.FileSize;
                }

                //fileData
                foreach (var file in files)
                {
                    var data = new byte[(uint)file.FileSize];
                    file.FileData.Read(data, 0, (int)file.FileSize);
                    bw.Write(data);
                }

                //Header
                bw.BaseStream.Position = 0;
                header.fileSize = (uint)bw.BaseStream.Length;
                header.fileChunks = (short)(exts.Count+1);
                header.fileCount = (short)files.Count;
                header.chunkInfSize = files.Count * 4 + files.Count * 0xc + exts.Count * 8;
                header.offsetList = nameOffset;
                bw.WriteStruct(header);
            }
        }
    }
}
