using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

namespace archive_level5.ARCV
{
    public sealed class ARCV
    {
        //Padding to next 0x100 - pad byte 0xac
        public List<ARCVFileInfo> Files = new List<ARCVFileInfo>();
        Stream _stream = null;

        private Header header;
        private List<FileEntry> entries;

        public ARCV(Stream input)
        {
            _stream = input;
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //FileEntries
                entries = br.ReadMultiple<FileEntry>((int)header.fileCount);

                for (int i = 0; i < header.fileCount; i++)
                {
                    Files.Add(new ARCVFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{i:00000000}.bin",
                        FileData = new SubStream(br.BaseStream, entries[i].offset, entries[i].size),
                        entry = entries[i]
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (BinaryWriterX bw = new BinaryWriterX(output))
            {
                var files = Files.OrderBy(x => x.entry.crc32);
                uint offset = (uint)(((0xc + (uint)files.Count() * 0xc) + 0x7f) & ~0x7f);

                //entries
                bw.BaseStream.Position = 0xc;
                foreach (var file in files)
                {
                    var entry = new FileEntry
                    {
                        offset = offset,
                        size = (uint)file.FileSize,
                        crc32 = file.entry.crc32
                    };
                    bw.WriteStruct(entry);
                }
                bw.WriteAlignment(0x100, 0xac);

                //Files
                foreach (var file in files)
                {
                    file.FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment(0x100, 0xac);
                }

                //Header
                bw.BaseStream.Position = 0;
                header.fileSize = (uint)bw.BaseStream.Length;
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
