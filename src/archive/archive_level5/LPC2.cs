using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cetera.Hash;
using Kontract.Interface;
using Kontract.IO;
using Kontract.Compression;

namespace archive_level5.LPC2
{
    public sealed class LPC2
    {
        public List<LPC2FileInfo> Files = new List<LPC2FileInfo>();
        Stream _stream = null;

        public Header header;
        public List<FileEntry> entries;

        public LPC2(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(new MemoryStream(Nintendo.Decompress(input)), true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Entries
                br.BaseStream.Position = header.fileEntryOffset;
                entries = br.ReadMultiple<FileEntry>(header.fileCount);

                //Files
                foreach (var entry in entries)
                {
                    br.BaseStream.Position = header.nameOffset + entry.nameOffset;
                    var name = br.ReadCStringA();
                    Files.Add(new LPC2FileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = name,
                        FileData = new SubStream(br.BaseStream, header.dataOffset + entry.fileOffset, entry.fileSize),
                        entry = entry
                    });
                }
            }
        }

        public void Save(string filename)
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms))
            {
                var dataOffset = header.dataOffset;

                //Write Files
                bw.BaseStream.Position = dataOffset;
                int relNameOff = 0, relFileOff = 0;
                foreach (var file in Files)
                    (relNameOff, relFileOff) = file.Write(bw.BaseStream, relNameOff, relFileOff);

                //Write Names
                bw.BaseStream.Position = header.nameOffset;
                foreach (var file in Files)
                    bw.Write(Encoding.ASCII.GetBytes(file.FileName + "\0"));

                //Write Entries
                bw.BaseStream.Position = header.fileEntryOffset;
                foreach (var file in Files)
                    bw.WriteStruct(file.entry);

                //Write Header
                bw.BaseStream.Position = 0;
                header.fileSize = (int)bw.BaseStream.Length;
                bw.WriteStruct(header);

                //Compress and write to file
                bw.BaseStream.Position = 0;
                File.WriteAllBytes(filename, Nintendo.Compress(bw.BaseStream, Nintendo.Method.LZ10));
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
