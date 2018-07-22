using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Text;

namespace archive_fbcf
{
    public class FBCF
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        private Stream _stream;

        Header header;
        int headerSize = 0x40;

        public FBCF(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(_stream, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Tables
                br.BaseStream.Position = header.fileOffsetTableOffset;
                var fileOffsets = br.ReadMultiple<int>(header.entryCount);

                br.BaseStream.Position = header.fileSizeOffsetTableOffset;
                var fileSizeOffsets = br.ReadMultiple<int>(header.entryCount);

                br.BaseStream.Position = header.fileNameOffsetTableOffset;
                var fileNameOffsets = br.ReadMultiple<int>(header.entryCount);

                Files.AddRange(Enumerable.Range(0, header.entryCount).Select(i =>
                {
                    br.BaseStream.Position = fileSizeOffsets[i] + header.fileSizeOffsetTableOffset;
                    var size = br.ReadInt32();
                    br.BaseStream.Position = fileNameOffsets[i] + header.fileNameOffsetTableOffset;
                    return new ArchiveFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = br.ReadCStringA(),
                        FileData = new SubStream(br.BaseStream, fileOffsets[i] + header.fileOffsetTableOffset, size)
                    };
                }));
            }
        }

        public void Save(Stream output)
        {
            var dataOffset = headerSize + Align(Files.Count*4, 0x20) * 3;
            header.entryCount = (short)Files.Count;

            using (var bw = new BinaryWriterX(output))
            {
                bw.BaseStream.Position = dataOffset;

                foreach (var file in Files)
                    file.FileData.CopyTo(bw.BaseStream);
                bw.WriteAlignment(0x20, 0x30);

                var fileSizeOffset = bw.BaseStream.Position;
                foreach (var file in Files)
                {
                    bw.Write((int)file.FileSize);
                    bw.WriteAlignment(0x20, 0x30);
                }

                var fileNameOffset = bw.BaseStream.Position;
                foreach (var file in Files)
                {
                    bw.Write(Encoding.ASCII.GetBytes(file.FileName + "\0"));
                    bw.WriteAlignment(0x20, 0x30);
                }

                bw.BaseStream.Position = headerSize;
                var offset = dataOffset - headerSize;
                foreach (var file in Files)
                {
                    bw.Write(offset);
                    offset += (int)file.FileSize;
                }
                bw.WriteAlignment(0x20, 0x30);

                header.fileSizeOffsetTableOffset = (int)bw.BaseStream.Position;
                header.fileSizeOffset = (int)fileSizeOffset - header.fileSizeOffsetTableOffset;
                offset = header.fileSizeOffset;
                foreach (var file in Files)
                {
                    bw.Write(offset);
                    offset += 0x20;
                }
                bw.WriteAlignment(0x20, 0x30);

                header.fileNameOffsetTableOffset = (int)bw.BaseStream.Position;
                header.fileNameOffset = (int)fileNameOffset - header.fileNameOffsetTableOffset;
                offset = header.fileNameOffset;
                foreach (var file in Files)
                {
                    bw.Write(offset);
                    offset += Align(Encoding.ASCII.GetByteCount(file.FileName) + 1, 0x20);
                }
                bw.WriteAlignment(0x20, 0x30);

                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }

        int Align(int toAlign, int alignment) => (toAlign + (alignment - 1)) & ~(alignment - 1);

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
