using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.NCCH
{
    public sealed class NCCH
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        Stream _stream = null;

        const int mediaUnitSize = 0x200;

        Header ncchHeader;
        ExHeader exHeader = null;
        byte[] plainRegion;
        byte[] logoRegion;
        ExeFS exeFS;

        public NCCH(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //NCCH Header
                ncchHeader = br.ReadStruct<Header>();

                //ExtHeader
                if (ncchHeader.exHeaderSize != 0)
                {
                    var extHeaderOffset = br.BaseStream.Position;
                    exHeader = br.ReadStruct<ExHeader>();

                    Files.Add(new ArchiveFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = "ExtHeader.bin",
                        FileData = new SubStream(br.BaseStream, extHeaderOffset, ncchHeader.exHeaderSize)
                    });
                }

                //Plain Region
                if (ncchHeader.plainRegionOffset != 0 && ncchHeader.plainRegionSize != 0)
                {
                    br.BaseStream.Position = ncchHeader.plainRegionOffset * mediaUnitSize;
                    plainRegion = br.ReadBytes(ncchHeader.plainRegionSize * mediaUnitSize);

                    Files.Add(new ArchiveFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = "PlainRegion.bin",
                        FileData = new SubStream(br.BaseStream, ncchHeader.plainRegionOffset * mediaUnitSize, ncchHeader.plainRegionSize * mediaUnitSize)
                    });
                }

                //Logo
                if (ncchHeader.logoRegionOffset != 0 && ncchHeader.logoRegionSize != 0)
                {
                    br.BaseStream.Position = ncchHeader.logoRegionOffset * mediaUnitSize;
                    logoRegion = br.ReadBytes(ncchHeader.logoRegionSize * mediaUnitSize);

                    Files.Add(new ArchiveFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = "Logo.icn",
                        FileData = new SubStream(br.BaseStream, ncchHeader.logoRegionOffset * mediaUnitSize, ncchHeader.logoRegionSize * mediaUnitSize)
                    });
                }

                //ExeFS
                int exeFSHeaderSize = 0x200;
                if (ncchHeader.exeFSOffset != 0 && ncchHeader.exeFSSize != 0)
                {
                    br.BaseStream.Position = ncchHeader.exeFSOffset * mediaUnitSize;
                    exeFS = new ExeFS(br.BaseStream);

                    foreach (var exeFsFile in exeFS.fileHeader)
                    {
                        if (exeFsFile.offset == 0 && exeFsFile.size == 0)
                            break;
                        Files.Add(new ExeFSFileInfo
                        {
                            State = ArchiveFileState.Archived,
                            FileName = "ExeFS\\" + exeFsFile.name,
                            FileData = new SubStream(br.BaseStream, ncchHeader.exeFSOffset * mediaUnitSize + exeFSHeaderSize + exeFsFile.offset, exeFsFile.size),
                            compressed = exeFsFile.name == ".code" && (exHeader.sci.flag & 0x1) == 1
                        });
                    }
                }

                //RomFS
                if (ncchHeader.romFSOffset != 0 && ncchHeader.romFSSize != 0)
                {

                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {

            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
