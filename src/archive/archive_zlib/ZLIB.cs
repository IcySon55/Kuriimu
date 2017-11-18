using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_zlib
{
    public class ZLIB
    {
        public List<ZLIBFileInfo> Files = new List<ZLIBFileInfo>();
        private Stream _stream = null;
        private Import imports = new Import();

        public ZLIB(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true, ByteOrder.BigEndian))
            {
                var decompSize = br.ReadUInt32();
                Files.Add(new ZLIBFileInfo
                {
                    State = ArchiveFileState.Archived,
                    FileName = "00.bin",
                    FileData = new SubStream(br.BaseStream, 4, br.BaseStream.Length - 4),
                    decompSize = decompSize,
                    imports = imports
                });
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                Files[0].Write(bw.BaseStream);
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
