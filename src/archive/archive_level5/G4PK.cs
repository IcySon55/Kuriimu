using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.Hash;
using Kontract.Interface;
using Kontract.IO;

namespace archive_level5.G4PK
{
    public sealed class G4PK
    {
        private Stream _stream;
        private List<short> _unkIds;

        public Header header;
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();

        public G4PK(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                // Header
                header = br.ReadStruct<Header>();
                br.BaseStream.Position = header.headerSize;

                // Sanity check
                if (header.headerSize + header.contentSize != input.Length)
                    throw new InvalidOperationException("Header information doesn't match file length.");

                // Entry information
                var fileOffsets = br.ReadMultiple<int>(header.fileCount);
                var fileSizes = br.ReadMultiple<int>(header.fileCount);
                var hashes = br.ReadMultiple<uint>(header.table2EntryCount);

                _unkIds = br.ReadMultiple<short>(header.table3EntryCount / 2);
                br.BaseStream.Position = (br.BaseStream.Position + 3) & ~3;
                var stringOffset = br.BaseStream.Position;
                var stringOffsets = br.ReadMultiple<short>(header.table3EntryCount / 2);

                //Files
                for (var i = 0; i < header.fileCount; i++)
                {
                    br.BaseStream.Position = stringOffset + stringOffsets[i];
                    var name = br.ReadCStringA();

                    // Sanity check
                    if (hashes[i] != Kontract.Hash.Crc32.Create(name))
                        throw new InvalidOperationException("Hash mismatch.");

                    Files.Add(new ArchiveFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = name,
                        FileData = new SubStream(br.BaseStream, header.headerSize + (fileOffsets[i] << 2), fileSizes[i])
                    });
                }
            }
        }

        public void Save(string filename)
        {
            // Write file data and remember the relative offsets
            var fileData = new MemoryStream();
            var fileOffsets = new List<int>();
            foreach (var file in Files)
            {
                fileData.Position = (fileData.Position + 0x1F) & ~0x1F;

                fileOffsets.Add((int)fileData.Position);

                file.FileData.Position = 0;
                file.FileData.CopyTo(fileData);
            }

            // It seems the file data portion in itself is aligned to 0x20
            while (fileData.Position % 0x20 != 0)
                fileData.WriteByte(0);

            using (var bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                // Write table information
                bw.BaseStream.Position = header.headerSize;

                // Write file offsets last, due to their relative property to the following tables
                bw.BaseStream.Position += Files.Count * 0x4;
                foreach (var file in Files)
                    bw.Write((int)file.FileSize);

                foreach (var file in Files)
                    bw.Write(Crc32.Create(file.FileName));

                foreach (var unkId in _unkIds)
                    bw.Write(unkId);
                bw.WriteAlignment(4);

                var stringRelativeOffset = (Files.Count * 2 + 3) & ~3;
                foreach (var file in Files)
                {
                    bw.Write((short)stringRelativeOffset);
                    stringRelativeOffset += Encoding.ASCII.GetByteCount(file.FileName) + 1;
                }
                bw.WriteAlignment(4);

                foreach (var file in Files)
                    bw.WriteASCII(file.FileName + '\0');
                bw.WriteAlignment();

                bw.BaseStream.Position = 0x40;
                foreach (var offset in fileOffsets)
                    bw.Write((int)((offset + bw.BaseStream.Length - 0x40) >> 2));

                // Write file data
                bw.BaseStream.Position = bw.BaseStream.Length;
                fileData.Position = 0;
                fileData.CopyTo(bw.BaseStream);

                // Create header
                var newHeader = new Header
                {
                    contentSize = (int)bw.BaseStream.Length - 0x40,
                    fileCount = Files.Count,
                    table2EntryCount = (short)Files.Count,
                    table3EntryCount = (short)(Files.Count * 2),
                    unk2 = header.unk2,
                    unk3 = header.unk3
                };
                bw.BaseStream.Position = 0;
                bw.WriteStruct(newHeader);
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
