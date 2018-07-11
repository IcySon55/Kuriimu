using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.DDSFS
{
    public class DDSFS
    {
        public List<NCSDFileInfo> Files = new List<NCSDFileInfo>();
        private Stream _stream = null;

        NCSDHeader ncsdHeader;
        CardInfoHeader cardInfoHeader;

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
            using (var bw = new BinaryWriterX(output))
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
