using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_asg_fnt
{
    public class FNT
    {
        public List<FNTFileInfo> Files = new List<FNTFileInfo>();
        private Stream _stream = null;

        public FNT(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //FileCount
                var fileCount = br.ReadInt32();

                //Entries
                var offsets = br.ReadMultiple<int>(fileCount + 1);

                //Files
                var count = 0;
                for (int i = 0; i < offsets.Count - 1; i++)
                    Files.Add(new FNTFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{count++}.ctpk",
                        FileData = new SubStream(br.BaseStream, offsets[i], offsets[i + 1] - offsets[i]),
                        offset = offsets[i]
                    });
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //Files
                var offset = Files[0].offset;
                bw.BaseStream.Position = offset;
                foreach (var file in Files)
                    offset = file.Write(bw.BaseStream, offset);

                //Entries
                bw.BaseStream.Position = 0;
                bw.Write(Files.Count);
                foreach (var file in Files)
                    bw.Write(file.offset);
                bw.Write((uint)bw.BaseStream.Length);
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
