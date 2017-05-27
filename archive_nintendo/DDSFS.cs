using System.Collections.Generic;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_3dsfs
{
    public class DDSFS
    {
        public List<DDSFSFileInfo> Files = new List<DDSFSFileInfo>();
        private Stream _stream = null;

        NCSDHeader ncsdHeader;
        IVFCHeader ivfcHeader;
        List<NCCHHeader> ncchHeaders;

        public DDSFS(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //NCSD
                ncsdHeader = new NCSDHeader(br.BaseStream);

                //NCCH Partitions
                ncchHeaders = new List<NCCHHeader>();
                for (int i=0;i<8;i++)
                {
                    if (ncsdHeader.partEntries[i].offset != 0)
                    {
                        br.BaseStream.Position = ncsdHeader.partEntries[i].offset;
                        var ncchOffset = br.BaseStream.Position;
                        ncchHeaders.Add(new NCCHHeader(br.BaseStream));

                        switch (i)
                        {
                            //Executable content
                            case 0:
                                //ExHeader+Access
                                Files.Add(new DDSFSFileInfo
                                {
                                    State = ArchiveFileState.Archived,
                                    FileName = "ExHeader.bin",
                                    FileData = new SubStream(br.BaseStream, br.BaseStream.Position, ncchHeaders[i].exHeaderSize)
                                });
                                Files.Add(new DDSFSFileInfo
                                {
                                    State = ArchiveFileState.Archived,
                                    FileName = "AccessDescriptor.bin",
                                    FileData = new SubStream(br.BaseStream, br.BaseStream.Position + ncchHeaders[i].exHeaderSize, ncchHeaders[i].exHeaderSize)
                                });

                                //Logo
                                if (ncchHeaders[i].logoRegOffset != 0)
                                {
                                    Files.Add(new DDSFSFileInfo
                                    {
                                        State = ArchiveFileState.Archived,
                                        FileName = "Logo.lz11",
                                        FileData = new SubStream(br.BaseStream, ncchOffset + ncchHeaders[i].logoRegOffset, ncchHeaders[i].logoRegSize)
                                    });
                                }

                                //PlainRegion
                                if (ncchHeaders[i].plainRegOffset != 0)
                                {
                                    Files.Add(new DDSFSFileInfo
                                    {
                                        State = ArchiveFileState.Archived,
                                        FileName = "PlainRegion.bin",
                                        FileData = new SubStream(br.BaseStream, ncchOffset + ncchHeaders[i].plainRegOffset, ncchHeaders[i].plainRegSize)
                                    });
                                }

                                //ExeFS
                                if (ncchHeaders[i].exeFsOffset != 0)
                                {
                                    br.BaseStream.Position = ncchOffset + ncchHeaders[i].exeFsOffset;

                                    List<ExeFsFileEntry> exefsFileEntries = new List<ExeFsFileEntry>();
                                    for (int j = 0; j < 10; j++) exefsFileEntries.Add(new ExeFsFileEntry(br.BaseStream));

                                    br.ReadBytes(0x20);

                                    List<byte[]> hashes = new List<byte[]>();
                                    for (int j = 0; j < 10; j++) hashes.Add(br.ReadBytes(0x20));

                                    var exefsOffset = br.BaseStream.Position;
                                    for (int j = 0; j < 10; j++)
                                        if (exefsFileEntries[j].size != 0)
                                        {
                                            Files.Add(new DDSFSFileInfo
                                            {
                                                State = ArchiveFileState.Archived,
                                                FileName = "ExeFS/" + exefsFileEntries[j].filename + ".bin",
                                                FileData = new SubStream(br.BaseStream, exefsOffset + exefsFileEntries[j].offset, exefsFileEntries[j].size)
                                            });
                                            br.BaseStream.Position += exefsFileEntries[j].size;
                                            while (br.BaseStream.Position % 0x400 != 0) br.BaseStream.Position++;
                                        }
                                }

                                //RomFS
                                if (ncchHeaders[i].romFsOffset != 0)
                                {
                                    //IVFC Header
                                    br.BaseStream.Position = ncchOffset + ncchHeaders[i].romFsOffset;
                                    ivfcHeader = new IVFCHeader(br.BaseStream);

                                    while (br.BaseStream.Position % 0x10 != 0) br.BaseStream.Position++;
                                    br.BaseStream.Position += ivfcHeader.masterHashSize;
                                    var blockSize = 1 << (int)ivfcHeader.lv3BlockSize;
                                    while (br.BaseStream.Position % blockSize != 0) br.BaseStream.Position++;

                                    //Level 3

                                }

                                break;
                        }
                    }
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
            _stream?.Dispose();
            _stream = null;
        }
    }
}
