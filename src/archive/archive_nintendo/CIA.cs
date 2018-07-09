using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.CIA
{
    public sealed class CIA
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        Stream _stream = null;

        int alignement = 0x40;

        Header ciaHeader;
        CertChain certChain;
        Ticket ticket;
        TMD tmd;
        Meta meta = null;

        List<SubStream> partitions;

        public CIA(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //CIA Header
                ciaHeader = br.ReadStruct<Header>();
                br.SeekAlignment(alignement);

                //CertChain
                certChain = new CertChain(br.BaseStream);
                br.SeekAlignment(alignement);

                //Ticket
                ticket = new Ticket(br.BaseStream);
                br.SeekAlignment(alignement);

                //TMD
                tmd = new TMD(br.BaseStream);
                br.SeekAlignment(alignement);

                //Meta
                if (ciaHeader.metaSize != 0)
                {
                    meta = br.ReadStruct<Meta>();
                    br.SeekAlignment(alignement);
                }

                //List NCCH's
                partitions = new List<SubStream>();
                var partitionOffset = br.BaseStream.Position;
                for (int i = 0; i < tmd.contentChunkRecord.Count; i++)
                {
                    partitions.Add(new SubStream(br.BaseStream, partitionOffset, tmd.contentChunkRecord[i].contentSize));
                    partitionOffset += tmd.contentChunkRecord[i].contentSize;
                }
                
                foreach (var sub in partitions)
                {
                    sub.Position = 0x188;
                    byte[] flags = new byte[8];
                    sub.Read(flags, 0, 8);
                    sub.Position = 0;

                    Files.Add(new ArchiveFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = GetPartitionName(flags[5]),
                        FileData = sub
                    });
                }
            }
        }

        private string GetPartitionName(byte typeFlag)
        {
            string ext = ((typeFlag & 0x1) == 1 && ((typeFlag >> 1) & 0x1) == 1) ? ".cxi" : ".cfa";

            string fileName = "";
            if ((typeFlag & 0x1) == 1 && ((typeFlag >> 1) & 0x1) == 1)
                fileName = "GameData";
            else if ((typeFlag & 0x1) == 1 && ((typeFlag >> 2) & 0x1) == 1 && ((typeFlag >> 3) & 0x1) == 1)
                fileName = "DownloadPlay";
            else if ((typeFlag & 0x1) == 1 && ((typeFlag >> 2) & 0x1) == 1)
                fileName = "3DSUpdate";
            else if ((typeFlag & 0x1) == 1 && ((typeFlag >> 3) & 0x1) == 1)
                fileName = "Manual";
            else if ((typeFlag & 0x1) == 1 && ((typeFlag >> 4) & 0x1) == 1)
                fileName = "Trial";
            
            return fileName + ext;
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {

            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
