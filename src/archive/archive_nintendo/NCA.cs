using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.NCA
{
    public sealed class NCA
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        Stream _stream = null;

        NCAHeader ncaHeader;
        List<Section> sections;

        public NCA(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                ncaHeader = new NCAHeader(input);
                sections = br.ReadMultiple<Section>(ncaHeader.sectionEntries.Count(s => s.mediaStartOffset != 0));

                int index = 0;
                foreach (var section in sections)
                {
                    switch (section.fsType)
                    {
                        case 2:     //PFS0
                            var pfs0Block = new BinaryReaderX(new MemoryStream(section.superBlock)).ReadStruct<PFS0SuperBlock>();
                            var pfs0 = new PFS0(input, pfs0Block, ncaHeader.sectionEntries[index].mediaStartOffset * 0x200L);
                            foreach (var file in pfs0.files)
                                Files.Add(new ArchiveFileInfo
                                {
                                    State = ArchiveFileState.Archived,
                                    FileName = Path.Combine($"PFS0 (Section {index})", file.name),
                                    FileData = new SubStream(_stream, file.fileEntry.offset, file.fileEntry.size)
                                });
                            break;
                        case 3:     //RomFS
                            var romFSBlock = new BinaryReaderX(new MemoryStream(section.superBlock)).ReadStruct<RomFSSuperBlock>();
                            var romFS = new RomFS(input, romFSBlock, ncaHeader.sectionEntries[index].mediaStartOffset * 0x200L, $"RomFS (Section {index})");
                            foreach (var file in romFS.files)
                            {
                                Files.Add(new ArchiveFileInfo
                                {
                                    State = ArchiveFileState.Archived,
                                    FileName = file.fileName,
                                    FileData = new SubStream(_stream, file.fileOffset, file.fileSize)
                                });
                            }
                            break;
                        default:
                            throw new InvalidDataException("Filesystem Type of a NCA section can only be 2 or 3");
                    }
                    index++;
                }
            }
        }

        public void Save(Stream output)
        {

        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
