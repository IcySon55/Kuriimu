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

                int partCount = 0;
                foreach (var sub in partitions)
                    Files.Add(new ArchiveFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = "Partition " + partCount++,
                        FileData = sub
                    });
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
            _stream?.Close();
            _stream = null;
        }
    }
}
