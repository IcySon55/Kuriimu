using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_aatri.aatri
{
    public class AATRI
    {
        public List<AatriFileInfo> Files = new List<AatriFileInfo>();
        private Stream _stream = null;

        private static Dictionary<string, string> _knownFiles = new Dictionary<string, string>
        {
            ["BCH"] = ".bch",
            [".ans"] = ".ans",
            ["FFNT"] = ".bffnt",
            ["mcol"] = ".mcol"
        };

        public AATRI(Stream incInput, Stream datInput)
        {
            _stream = datInput;
            var entryCount = 0;
            List<Entry> entries = new List<Entry>();
            using (var br = new BinaryReaderX(incInput))
            {
                entryCount = (int)br.BaseStream.Length / 0x14;
                entries = br.ReadMultiple<Entry>(entryCount);
            }

            using (var br = new BinaryReaderX(datInput, true))
            {
                for (int i = 0; i < entryCount; i++)
                {
                    br.BaseStream.Position = entries[i].offset + 5;
                    var mag = br.ReadString(4);
                    var extension = (_knownFiles.ContainsKey(mag)) ? _knownFiles[mag] : ".bin";

                    Files.Add(new AatriFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{i:00000000}" + extension,
                        FileData = new SubStream(br.BaseStream, entries[i].offset, entries[i].compSize),
                        Entry = entries[i]
                    });
                }
            }
        }

        public void Save(Stream incOutput, Stream datOutput)
        {
            using (var bw = new BinaryWriterX(incOutput))
            using (var bwd = new BinaryWriterX(datOutput))
            {
                for (int i = 0; i < Files.Count; i++)
                {
                    Files[i].Entry.offset = (uint)bwd.BaseStream.Position;
                    Files[i].Write(bwd.BaseStream);
                    bwd.WriteAlignment(4);
                    bw.WriteStruct(Files[i].Entry);
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
