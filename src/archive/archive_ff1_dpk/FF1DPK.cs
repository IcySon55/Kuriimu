using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract.IO;

namespace archive_ff1_dpk
{
    class FF1DPK
    {
        public List<FF1DpkArchiveFileInfo> Files { get; set; }

        private Stream _stream;

        public FF1DPK(Stream input)
        {
            _stream = input;
            Files = new List<FF1DpkArchiveFileInfo>();

            using (var br = new BinaryReaderX(input, true))
            {
                var fileCount = br.ReadInt32();
                var archiveSize = br.ReadInt32();

                var baseOffset = 0x80;
                var fileEntries = new FileEntry[fileCount];
                for (int i = 0; i < fileCount; i++)
                {
                    br.BaseStream.Position = baseOffset + i * baseOffset;
                    fileEntries[i] = ReadFileEntry(br);
                    fileEntries[i].fileName = fileEntries[i].fileName.TrimEnd('\0');
                }

                var wp16 = new Wp16();
                foreach (var fileEntry in fileEntries)
                {
                    Files.Add(new FF1DpkArchiveFileInfo(new SubStream(input, fileEntry.fileOffset, fileEntry.compressedSize), fileEntry)
                    {
                        State = ArchiveFileState.Archived,
                        FileName = fileEntry.fileName
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                // Collect all file entries
                var fileEntries = new Tuple<FileEntry, FF1DpkArchiveFileInfo>[Files.Count];
                var fileEntryPosition = 0;
                foreach (var file in Files)
                {
                    var fileName = file.FileName.Substring(0, Math.Min(0x16, file.FileName.Length));
                    var fileNameSum = fileName.Sum(x => (byte)x);

                    var fileEntry = new FileEntry
                    {
                        fileName = fileName,
                        nameSum = (short)fileNameSum,
                        uncompressedSize = (int)file.FileSize
                    };
                    fileEntries[fileEntryPosition++] = new Tuple<FileEntry, FF1DpkArchiveFileInfo>(fileEntry, file);
                }

                // Write file data ordered file entries name sums
                fileEntries = fileEntries.OrderBy(x => x.Item1.nameSum).ToArray();
                var filesOffset = 0x80 + Files.Count * 0x80;
                foreach (var fileEntryTuple in fileEntries)
                {
                    bw.BaseStream.Position = filesOffset;
                    fileEntryTuple.Item2.WriteFile(output);

                    fileEntryTuple.Item1.fileOffset = filesOffset;
                    fileEntryTuple.Item1.compressedSize = (int) bw.BaseStream.Position - filesOffset;

                    bw.WriteAlignment(0x80);
                    filesOffset = (int)bw.BaseStream.Position;
                }

                // Write file entries ordered by file sum
                bw.BaseStream.Position = 0x80;
                foreach (var fileEntry in fileEntries)
                {
                    WriteFileEntry(bw, fileEntry.Item1);
                    bw.WriteAlignment(0x80);
                }

                // Header
                bw.BaseStream.Position = 0;
                bw.Write(Files.Count);
                bw.Write((int)bw.BaseStream.Length);
            }
        }

        private FileEntry ReadFileEntry(BinaryReaderX br)
        {
            var entry = new FileEntry
            {
                fileName = Encoding.ASCII.GetString(br.ReadBytes(0x16)).TrimEnd('\0'),
                nameSum = br.ReadInt16(),
                fileOffset = br.ReadInt32(),
                compressedSize = br.ReadInt32(),
                uncompressedSize = br.ReadInt32()
            };

            return entry;
        }

        private void WriteFileEntry(BinaryWriterX bw, FileEntry entry)
        {
            bw.WriteASCII(entry.fileName.PadRight(0x16, '\0'));
            bw.Write(entry.nameSum);
            bw.Write(entry.fileOffset);
            bw.Write(entry.compressedSize);
            bw.Write(entry.uncompressedSize);
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
