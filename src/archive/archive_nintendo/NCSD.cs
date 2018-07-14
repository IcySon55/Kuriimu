using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System;
using System.Linq;

namespace archive_nintendo.DDSFS
{
    public class DDSFS
    {
        public List<NCSDFileInfo> Files = new List<NCSDFileInfo>();
        private Stream _stream = null;

        NCSDHeader ncsdHeader;
        CardInfoHeader cardInfoHeader;

        const int mediaUnitSize = 0x200;

        public DDSFS(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //NCSD
                ncsdHeader = new NCSDHeader(br.BaseStream);
                cardInfoHeader = br.ReadStruct<CardInfoHeader>();

                //NCCH Partitions
                for (int i = 0; i < ncsdHeader.partEntries.Count; i++)
                {
                    if (ncsdHeader.partEntries[i].offset != 0)
                    {
                        string name;
                        switch (i)
                        {
                            case 0:
                                name = "GameData.cxi";
                                break;
                            case 1:
                                name = "EManual.cfa";
                                break;
                            case 2:
                                name = "DLP.cfa";
                                break;
                            case 6:
                                name = "FirmwareUpdate.cfa";
                                break;
                            case 7:
                                name = "UpdateData.cfa";
                                break;
                            default:
                                name = "Unknown.cfa";
                                break;
                        }
                        Files.Add(new NCSDFileInfo
                        {
                            State = ArchiveFileState.Archived,
                            FileName = name,
                            FileData = new SubStream(br.BaseStream, ncsdHeader.partEntries[i].offset, ncsdHeader.partEntries[i].size),
                            PartitionID = i
                        });
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            ncsdHeader.rsa = new byte[0x100];
            ncsdHeader.shaHash = (Files.Count(f => f.PartitionID == 0) > 0) ? Kontract.Hash.SHA256.Create(Files.Where(f => f.PartitionID == 0).First().FileData, 0x200, 0x800) : new byte[0x20];
            ncsdHeader.cardInfoHeader.copyFirstNCCHHeader = new BinaryReaderX(new SubStream(Files.First().FileData, 0x100, 0x100), true).ReadAllBytes();

            using (var bw = new BinaryWriterX(output))
            {
                bw.BaseStream.Position = 0x4000;
                foreach (var file in Files)
                {
                    ncsdHeader.partEntries[file.PartitionID].offset = (uint)(bw.BaseStream.Position / mediaUnitSize);
                    ncsdHeader.partEntries[file.PartitionID].size = (uint)(file.FileSize / mediaUnitSize);

                    file.FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment(mediaUnitSize);
                }

                ncsdHeader.ncsdSize = (uint)(bw.BaseStream.Length / mediaUnitSize);

                bw.BaseStream.Position = 0;
                ncsdHeader.Write(bw.BaseStream);
                bw.WriteAlignment(0x4000, 0xFF);
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
