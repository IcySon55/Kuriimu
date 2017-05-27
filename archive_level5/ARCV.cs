using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_level5.ARCV
{
    public sealed class ARCV
    {
        public List<ARCVFileInfo> Files = new List<ARCVFileInfo>();
        Stream _stream = null;

        private Header header;
        private List<Entry> entries;

        public ARCV(Stream input)
        {
            _stream = input;
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                //everywhere padding to the next 0x100 with 0xac

                //Header
                header = br.ReadStruct<Header>();

                //FileEntries
                entries = br.ReadMultiple<Entry>(header.fileCount);

                for (int i = 0; i < header.fileCount; i++)
                {
                    Files.Add(new ARCVFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"0x{entries[i].hash:X08}",
                        FileData = new SubStream(br.BaseStream, entries[i].offset, entries[i].size),
                        hash = entries[i].hash
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (BinaryWriterX bw = new BinaryWriterX(output))
            {
                var files = Files.OrderBy(x => x.hash);

                //entryList and FileData
                uint offset = 0xc + (uint)files.Count() * 0xc;
                while (offset % 0x80 != 0) offset++;
                bw.BaseStream.Position = 0xc;
                foreach (var file in files)
                {
                    bw.Write(offset);
                    bw.Write((uint)file.FileSize);
                    bw.Write(file.hash);

                    long bk = bw.BaseStream.Position;
                    bw.BaseStream.Position = offset;
                    file.FileData.CopyTo(bw.BaseStream);
                    while (bw.BaseStream.Position % 0x80 != 0) bw.Write((byte)0xac);
                    offset = (uint)bw.BaseStream.Position;
                    bw.BaseStream.Position = bk;
                }

                while (bw.BaseStream.Position % 0x80 != 0) bw.Write((byte)0xac);

                //Header
                bw.BaseStream.Position = 0;
                bw.WriteASCII("ARCV");
                bw.Write(Files.Count);
                bw.Write((uint)bw.BaseStream.Length);
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
