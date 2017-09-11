using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace archive_nintendo.VIW
{
    class VIW
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        private Stream _infStream;
        private Stream _viwStream;
        private Stream _stream;

        private InfHeader Header;
        private List<InfEntry> Offsets;
        private List<InfMetaEntry> MetaEntries;
        private List<ViwEntry> Names;

        public VIW(Stream infInput, Stream viwInput, Stream input)
        {
            _infStream = infInput;
            _viwStream = viwInput;
            _stream = input;
            using (var br = new BinaryReaderX(infInput, true))
            {
                uint index = 0;

                Header = br.ReadStruct<InfHeader>();
                Offsets = br.ReadMultiple<InfEntry>(Header.FileCount);

                if (br.BaseStream.Length > Header.Table1Offset)
                    MetaEntries = br.ReadMultiple<InfMetaEntry>(Header.NameCount);
            }

            using (var br = new BinaryReaderX(viwInput, true))
            {
                Names = br.ReadMultiple<ViwEntry>(Header.NameCount);
            }

            using (var br = new BinaryReaderX(input, true))
            {
                Files.AddRange(Offsets.Select(o => new ArchiveFileInfo
                {
                    FileName = Names.FirstOrDefault(n => n.ID == MetaEntries[Offsets.IndexOf(o)].Unk1)?.Name,
                    FileData = new SubStream(input, o.Offset, o.CompressedSize),
                    State = ArchiveFileState.Archived
                }));
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {

                foreach (var info in Files)
                {
                }

                foreach (var info in Files)
                    info.FileData.CopyTo(bw.BaseStream);
            }
        }

        public void Close()
        {
            _infStream?.Dispose();
            _viwStream?.Dispose();
            _stream?.Dispose();
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _infStream = null;
            _viwStream = null;
            _stream = null;
        }
    }
}
