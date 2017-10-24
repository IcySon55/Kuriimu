using System.Collections.Generic;
using System.IO;
using Kontract.IO;
using Kontract.Interface;

namespace archive_nintendo.VIW
{
    class VIW
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        private Stream _viwStream;
        private Stream _infStream;
        private Stream _stream;

        private InfHeader Header;
        private List<InfEntry> Offsets;
        private List<InfMetaEntry> MetaEntries;
        private List<ViwEntry> Names;

        public VIW(Stream viwInput, Stream infInput, Stream input)
        {
            _infStream = infInput;
            _viwStream = viwInput;
            _stream = input;

            using (var br = new BinaryReaderX(infInput, true))
            {
                Header = br.ReadStruct<InfHeader>();
                Offsets = br.ReadMultiple<InfEntry>(Header.FileCount);

                if (Header.MetaEntryCount > 0)
                    MetaEntries = br.ReadMultiple<InfMetaEntry>(Header.MetaEntryCount);
            }

            using (var br = new BinaryReaderX(viwInput, true))
            {
                Names = new List<ViwEntry>();
                while (br.BaseStream.Position < br.BaseStream.Length)
                    Names.Add(br.ReadStruct<ViwEntry>());
            }

            foreach (var off in Offsets)
            {
                var afi = new ViwFileInfo
                {
                    FileName = (Offsets.IndexOf(off) + Names[0].ID).ToString("X4") + ".bin",
                    FileData = new SubStream(input, off.Offset, off.CompressedSize),
                    State = ArchiveFileState.Archived
                };

                Files.Add(afi);
            }
        }

        public void Save(Stream viwOutput, Stream infOutput, Stream output)
        {
            using (output)
                for (var i = 0; i < Files.Count; i++)
                {
                    Offsets[i].Offset = (int)output.Position;
                    Offsets[i].CompressedSize = ((ViwFileInfo)Files[i]).Write(output);
                }

            using (var bw = new BinaryWriterX(infOutput))
            {
                bw.WriteStruct(Header);

                foreach (var offset in Offsets)
                    bw.WriteStruct(offset);

                if (Header.MetaEntryCount > 0)
                    foreach (var entry in MetaEntries)
                        bw.WriteStruct(entry);
            }

            using (var bw = new BinaryWriterX(viwOutput))
            {
                foreach (var name in Names)
                    bw.WriteStruct(name);
            }
        }

        public void Close()
        {
            _viwStream?.Dispose();
            _infStream?.Dispose();
            _stream?.Dispose();
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _viwStream = null;
            _infStream = null;
            _stream = null;
        }
    }
}
