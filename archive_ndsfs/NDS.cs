using System.Collections.Generic;
using System.IO;
using System.Text;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_ndsfs
{
    public sealed class NDS
    {
        public List<NDSFileInfo> Files = new List<NDSFileInfo>();
        Stream _stream = null;

        Header header;

        public NDS(Stream input)
        {
            _stream = input;
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                // ROM Header
                header = new Header(br.BaseStream);

                // Add ARM9 file
                Files.Add(new NDSFileInfo
                {
                    State = ArchiveFileState.Archived,
                    FileName = "arm9.bin",
                    FileData = new SubStream(br.BaseStream, header.ARM9romOffset, header.ARM9size)
                });

                // Add ARM7 file
                Files.Add(new NDSFileInfo
                {
                    State = ArchiveFileState.Archived,
                    FileName = "arm7.bin",
                    FileData = new SubStream(br.BaseStream, header.ARM7romOffset, header.ARM7size)
                });

                // Name Table
                br.BaseStream.Position = header.fileNameTableOffset;
                List<NameTableEntry> fntEntries = new List<NameTableEntry>();
                fntEntries.Add(new NameTableEntry(br.BaseStream));
                for (int i = 0; i < (int)fntEntries[0].subTableOffset / 8 - 1; i++) fntEntries.Add(new NameTableEntry(br.BaseStream));

                foreach (var nameTableEntry in fntEntries)
                {
                    br.BaseStream.Position = header.fileNameTableOffset + nameTableEntry.subTableOffset;
                    byte inf = br.ReadByte();
                    var fileId = nameTableEntry.idFirstFile;

                    while (inf != 0x0)
                    {
                        if (inf < 0x80)
                        {
                            nameTableEntry.subTable.files.Add(new sFile
                            {
                                name = Encoding.GetEncoding("SJIS").GetString(br.ReadBytes(inf)),
                                id = fileId++
                            });
                        }
                        else
                        {
                            nameTableEntry.subTable.folders.Add(new sFolder
                            {
                                name = Encoding.GetEncoding("SJIS").GetString(br.ReadBytes(inf - 0x80)),
                                id = br.ReadUInt16()
                            });
                        }

                        inf = br.ReadByte();
                    }
                }

                // Parse Filenames
                var nameList = NDSSupport.parseFileNames(fntEntries);

                // File Data
                br.BaseStream.Position = header.FAToffset;
                foreach (var name in nameList)
                {
                    uint fileOffset = br.ReadUInt32();
                    Files.Add(new NDSFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = name,
                        FileData = new SubStream(br.BaseStream, fileOffset, br.ReadUInt32() - fileOffset)
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (BinaryWriterX bw = new BinaryWriterX(output))
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
