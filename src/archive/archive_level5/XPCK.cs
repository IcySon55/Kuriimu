using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Compression;
using Kuriimu.Kontract;
using Kuriimu.IO;
using System;
using Cetera.Hash;

namespace archive_level5.XPCK
{
    public sealed class XPCK
    {
        public List<XPCKFileInfo> Files = new List<XPCKFileInfo>();
        Stream _stream = null;

        Header header;
        List<FileInfoEntry> entries = new List<FileInfoEntry>();
        byte[] compNameTable;

        public XPCK(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Entries
                br.BaseStream.Position = header.fileInfoOffset;
                entries.AddRange(br.ReadMultiple<FileInfoEntry>(header.fileCount));

                //Filenames
                br.BaseStream.Position = header.filenameTableOffset;
                compNameTable = br.ReadBytes(header.filenameTableSize);
                var decNames = new MemoryStream(Level5.Decompress(new MemoryStream(compNameTable)));

                //Files
                using (var nameList = new BinaryReaderX(decNames))
                    for (int i = 0; i < header.fileCount; i++)
                    {
                        nameList.BaseStream.Position = entries[i].nameOffset;
                        Files.Add(new XPCKFileInfo
                        {
                            State = ArchiveFileState.Archived,
                            FileName = nameList.ReadCStringA(),
                            FileData = new SubStream(br.BaseStream, header.dataOffset + entries[i].fileOffset, entries[i].fileSize),
                            Entry = entries[i]
                        });
                    }
            }
        }

        public void Save(Stream xpck)
        {
            using (BinaryWriterX bw = new BinaryWriterX(xpck))
            {
                uint absDataOffset = header.dataOffset;

                //Files
                Files = Files.OrderBy(x => x.Entry.fileOffset).ToList();
                uint relDataOffset = 0;
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.BaseStream.Position = absDataOffset;
                    Files[i].FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment(4);
                    if (i != Files.Count - 1) bw.Write(0);

                    //Update entry
                    Files[i].Entry.tmp = (ushort)(((relDataOffset + 0x3 & ~0x3) >> 2) & 0xffff);
                    Files[i].Entry.tmpZ = (byte)((((relDataOffset + 0x3 & ~0x3) >> 2) & 0xff0000) >> 16);
                    Files[i].Entry.tmp2 = (ushort)(Files[i].FileSize & 0xffff);
                    Files[i].Entry.tmp2Z = (byte)((Files[i].FileSize & 0xff0000) >> 16);
                    if (i != Files.Count - 1) relDataOffset += (uint)(Files[i].FileSize + 0x3 & ~0x3) + 4;
                    if (i != Files.Count - 1) absDataOffset += (uint)(Files[i].FileSize + 0x3 & ~0x3) + 4;
                }

                //Entries
                Files = Files.OrderBy(x => x.Entry.crc32).ToList();
                bw.BaseStream.Position = 0x14;
                foreach (var file in Files)
                {
                    bw.WriteStruct(file.Entry);
                }

                //Namelist
                bw.Write(compNameTable);

                //Header
                header.tmp6 = (uint)(bw.BaseStream.Length - header.dataOffset) >> 2;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
