using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_bam
{
    public class BAM
    {
        public List<BAMFileInfo> Files = new List<BAMFileInfo>();
        private Stream _stream = null;

        public Header header;
        public byte[] headerCont;
        public FileHeader fileHeader;

        public BAM(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();
                br.BaseStream.Position = 0x14;
                var fileHeaderOffset = br.ReadUInt32();

                //Header Content
                br.BaseStream.Position = 0x8;
                headerCont = br.ReadBytes((int)fileHeaderOffset - 0x8);

                //FileHeader
                fileHeader = br.ReadStruct<FileHeader>();
                br.BaseStream.Position = (br.BaseStream.Position + 0x7f) & ~0x7f;

                //File
                Files.Add(new BAMFileInfo
                {
                    State = ArchiveFileState.Archived,
                    FileName = "00.bin",
                    FileData = new SubStream(br.BaseStream, br.BaseStream.Position, fileHeader.size)
                });
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //Header Content
                bw.BaseStream.Position = 0x8;
                bw.Write(headerCont);

                //FileHeader
                fileHeader.size = (uint)Files[0].FileSize;
                bw.WriteStruct(fileHeader);
                bw.WriteAlignment(0x80);

                //File
                Files[0].FileData.CopyTo(bw.BaseStream);

                //Header
                header.fileSize = (uint)bw.BaseStream.Length;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
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
