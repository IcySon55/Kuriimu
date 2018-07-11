using System.Collections.Generic;
using System;
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
        const int ncchHeaderSize = 0x200;

        Header ncchHeader;
        ExHeader exHeader = null;
        byte[] plainRegion = null;
        byte[] logoRegion = null;
        ExeFS exeFS = null;
        RomFS romFS = null;

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
                    var exHeaderOffset = br.BaseStream.Position;
                    exHeader = br.ReadStruct<ExHeader>();

                    Files.Add(new ArchiveFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = "ExHeader.bin",
                        FileData = new SubStream(br.BaseStream, exHeaderOffset, ncchHeader.exHeaderSize * 2)
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

                //Logo Region
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
                    br.BaseStream.Position = ncchHeader.romFSOffset * mediaUnitSize;
                    romFS = new RomFS(br.BaseStream);
                    foreach (var romFSFile in romFS.files)
                    {
                        Files.Add(new ArchiveFileInfo
                        {
                            State = ArchiveFileState.Archived,
                            FileName = "RomFS" + romFSFile.fileName,
                            FileData = new SubStream(br.BaseStream, romFSFile.fileOffset, romFSFile.fileSize)
                        });
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            void Align(Stream input) => input.Position = (input.Position + (mediaUnitSize - 1)) & ~(mediaUnitSize - 1);
            void RomFSAlign(Stream input) => input.Position = (input.Position + (0x1000 - 1)) & ~(0x1000 - 1);

            output.Seek(ncchHeaderSize, SeekOrigin.Begin);

            if (exHeader != null)
            {
                var exHFile = Files.Where(f => f.FileName == "ExHeader.bin").First();
                ncchHeader.exHeaderSize = (int)exHFile.FileSize;
                ncchHeader.exHeaderHash = Kontract.Hash.SHA256.Create(exHFile.FileData);

                exHFile.FileData.CopyTo(output);
                Align(output);
            }

            if (plainRegion != null)
            {
                var plRFile = Files.Where(f => f.FileName == "PlainRegion.bin").First();
                ncchHeader.plainRegionOffset = (int)Math.Ceiling((double)output.Position / mediaUnitSize);
                ncchHeader.plainRegionSize = (int)Math.Ceiling((double)plRFile.FileSize / mediaUnitSize);

                plRFile.FileData.CopyTo(output);
                Align(output);
            }

            if (logoRegion != null)
            {
                var loRFile = Files.Where(f => f.FileName == "Logo.icn").First();
                ncchHeader.plainRegionOffset = (int)Math.Ceiling((double)output.Position / mediaUnitSize);
                ncchHeader.plainRegionSize = (int)Math.Ceiling((double)loRFile.FileSize / mediaUnitSize);

                loRFile.FileData.CopyTo(output);
                Align(output);
            }

            if (exeFS != null)
            {
                var exeFSOffset = output.Position;
                var exeFSSize = ExeFSBuilder.Rebuild(output, Files.Where(f => f.FileName.StartsWith("ExeFS\\")).Select(f => (ExeFSFileInfo)f).ToList(), "ExeFS\\");

                ncchHeader.exeFSOffset = (int)Math.Ceiling((double)exeFSOffset / mediaUnitSize);
                ncchHeader.exeFSSize = (int)Math.Ceiling((double)exeFSSize / mediaUnitSize);
                ncchHeader.exeFSSuperBlockHash = Kontract.Hash.SHA256.Create(output, exeFSOffset, ExeFS.exeFSHeaderSize);

                RomFSAlign(output);
            }

            if (romFS != null)
            {
                var romFSOffset = output.Position;
                var romFSSize = RomFSBuilder.Rebuild(output, romFSOffset, Files.Where(f => f.FileName.StartsWith("RomFS\\")).ToList(), "RomFS\\");

                ncchHeader.romFSOffset = (int)Math.Ceiling((double)romFSOffset / mediaUnitSize);
                ncchHeader.romFSSize = (int)Math.Ceiling((double)romFSSize / mediaUnitSize);
                ncchHeader.romFSHashRegSize = (int)Math.Ceiling((double)RomFSBuilder.SuperBlockSize / mediaUnitSize);
                ncchHeader.romFSSuperBlockHash = RomFSBuilder.SuperBlockHash;

                output.Position = romFSOffset + romFSSize;
                Align(output);
            }

            //Header
            output.Seek(0, SeekOrigin.Begin);
            using (var bw = new BinaryWriterX(output))
                bw.WriteStruct(ncchHeader);
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
