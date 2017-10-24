using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_fnt
{
    public class FNT
    {
        public List<FntFileInfo> Files = new List<FntFileInfo>();
        private Stream _stream = null;

        private static Dictionary<string, string> _knownFiles = new Dictionary<string, string>
        {
            ["CTPK"] = ".ctpk"
        };

        public FNT(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //get fileEntries
                var entryCount = br.ReadInt32();
                var entries = new List<Entry>();
                for (int i = 0; i < entryCount; i++)
                {
                    entries.Add(new Entry(br.BaseStream));
                    br.BaseStream.Position -= 4;
                }

                //FileData
                for (int i = 0; i < entries.Count; i++)
                {
                    br.BaseStream.Position = entries[i].offset;
                    var magic = br.ReadString(4);

                    var extension = _knownFiles.ContainsKey(magic) ? _knownFiles[magic] : ".bin";
                    Files.Add(new FntFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{i:00000000}" + extension,
                        FileData = new SubStream(br.BaseStream, entries[i].offset, entries[i].size)
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var dataOffset = Files.Count * 4 + 8;
                dataOffset = (dataOffset + 0x7f) & ~0x7f;
                var dataOffsetTmp = dataOffset;

                //fileEntries
                bw.Write(Files.Count);
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.Write(dataOffsetTmp);
                    dataOffsetTmp += (int)Files[i].FileSize;
                }
                bw.Write(dataOffsetTmp);

                //FileData
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.BaseStream.Position = dataOffset;
                    Files[i].FileData.CopyTo(bw.BaseStream);
                    dataOffset += (int)Files[i].FileSize;
                }
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
