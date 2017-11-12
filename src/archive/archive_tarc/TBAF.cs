using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Text;

namespace archive_tarc
{
    public class TBAF
    {
        public List<TBAFFileInfo> Files = new List<TBAFFileInfo>();
        private Stream _stream = null;

        public Header header;

        public TBAF(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Entries
                br.BaseStream.Position = header.entryOffset;
                var entries = new List<Entry>();
                for (int i = 0; i < header.fileCount; i++)
                {
                    entries.Add(br.ReadStruct<Entry>());
                    br.BaseStream.Position = (br.BaseStream.Position + 0xf) & ~0xf;
                }

                //Files
                for (int i = 0; i < header.fileCount; i++)
                {
                    br.BaseStream.Position = entries[i].nameOffset;
                    Files.Add(new TBAFFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = br.ReadCStringA(),
                        FileData = new SubStream(br.BaseStream, entries[i].dataOffset, entries[i].dataSize),
                        entry = entries[i]
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //Entries
                bw.BaseStream.Position = 0x30;
                uint dataOffset = (uint)((header.nameOffset + header.nameSecOffset + 0xf) & ~0xf);
                foreach (var file in Files)
                {
                    file.entry.dataOffset = dataOffset;
                    file.entry.dataSize = (uint)file.FileSize;

                    bw.WriteStruct(file.entry);
                    bw.WriteAlignment();

                    dataOffset = (uint)((dataOffset + file.FileSize + 0xf) & ~0xf);
                }

                //Names
                foreach (var file in Files)
                {
                    bw.Write(Encoding.ASCII.GetBytes(file.FileName));
                    bw.Write((byte)0);
                }
                bw.WriteAlignment();

                //Files
                foreach (var file in Files)
                {
                    bw.WriteAlignment();
                    file.FileData.CopyTo(bw.BaseStream);
                }

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
