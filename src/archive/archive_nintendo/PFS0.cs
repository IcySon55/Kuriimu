using Kontract.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract;
using Kontract.Interface;

namespace archive_nintendo.PFS0
{
    public class PFS0
    {
        public List<PFS0FileInfo> Files = new List<PFS0FileInfo>();

        PFS0Header _header;
        List<FileEntry> _fileEntries;

        Stream _stream;

        public PFS0(Stream input)
        {
            _stream = input;

            using (var br = new BinaryReaderX(input, true))
            {
                _header = br.ReadStruct<PFS0Header>();
                _fileEntries = br.ReadMultiple<FileEntry>(_header.fileCount);

                var stringTable = br.ReadBytes(_header.stringTableSize);
                using (var nameBr = new BinaryReaderX(new MemoryStream(stringTable)))
                {
                    foreach (var entry in _fileEntries)
                    {
                        entry.offset += 0x10 + 0x18 * _header.fileCount + _header.stringTableSize;

                        nameBr.BaseStream.Position = entry.stringOffset;
                        Files.Add(new PFS0FileInfo
                        {
                            entry = entry,
                            State = ArchiveFileState.Archived,
                            FileName = nameBr.ReadCStringA(),
                            FileData = new SubStream(input, entry.offset, entry.size)
                        });
                    }
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
