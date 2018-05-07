using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_ChibiQp
{
    public class QP
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        private Stream _stream = null;

        public Header header;

        public QP(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, ByteOrder.BigEndian))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Entries
                br.BaseStream.Position = header.entryDataOffset;
                var unk1 = br.ReadInt32();
                var unk2 = br.ReadInt32();
                var entryCount = br.ReadInt32() - 1;
                var entries = br.ReadMultiple<Entry>(entryCount);

                //Names
                var nameData = br.ReadBytes((int)(header.entryDataSize - br.BaseStream.Position));

                //Files
                using (var nameBr = new BinaryReaderX(new MemoryStream(nameData)))
                    foreach (var entry in entries)
                    {
                        try
                        {
                            nameBr.BaseStream.Position = entry.relNameOffset;
                            Files.Add(new ArchiveFileInfo
                            {
                                State = ArchiveFileState.Archived,
                                FileName = nameBr.ReadCStringA(),
                                FileData = new SubStream(br.BaseStream, entry.fileOffset, entry.fileSize)
                            });
                        }
                        catch
                        {
                            ;
                        }
                    }
            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input, ByteOrder.BigEndian))
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
