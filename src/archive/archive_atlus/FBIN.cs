using Kontract.Interface;
using Kontract.IO;
using System.Collections.Generic;
using System.IO;

namespace archive_atlus.FBIN
{
    public class FBIN
    {
        public List<FbinFileInfo> Files = new List<FbinFileInfo>();
        private Stream _stream = null;

        public Header header;

        public FBIN(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                header = br.ReadStruct<Header>();
                var offsets = br.ReadMultiple<int>(header.fileCount);
                br.BaseStream.Position = br.BaseStream.Position + 0x0C; //0x0C is the size of the header struct
                for (int i = 0; i < header.fileCount - 1; i++)
                {
                    Files.Add(new FbinFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = "file"+ i + ".bin",
                        FileData = new SubStream(br.BaseStream, br.BaseStream.Position, offsets[i])
                    });
                    br.BaseStream.Position = (br.BaseStream.Position + offsets[i]);
                }
                
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                // TODO
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
