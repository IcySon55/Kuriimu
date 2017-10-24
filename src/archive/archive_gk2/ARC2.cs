using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;
using Kontract.Compression;

namespace archive_gk2.arc2
{
    public class ARC2
    {
        public List<Arc2FileInfo> Files = new List<Arc2FileInfo>();
        private Stream _stream = null;

        Dictionary<string, string> extensions = new Dictionary<string, string>
        {
            ["RECN"] = "ncer",
            ["RNAN"] = "nanr",
            ["RGCN"] = "ncgr",
            ["RLCN"] = "nclr",
        };

        public ARC2(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Entries
                var limit = br.ReadUInt32();

                var entries = new List<Entry>();
                var offset = limit;
                while (br.BaseStream.Position < limit)
                {
                    var offset2 = br.ReadUInt32();
                    entries.Add(new Entry
                    {
                        offset = offset,
                        size = offset2 - offset
                    });

                    offset = offset2;
                }
                entries.Add(new Entry
                {
                    offset = offset,
                    size = (uint)br.BaseStream.Length - offset
                });

                //Files
                var count = 0;
                foreach (var entry in entries)
                {
                    var ext = br.PeekString(entry.offset, 4);
                    Files.Add(new Arc2FileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = (extensions.ContainsKey(ext)) ? $"{count++:00000000}.{extensions[ext]}" : $"{count++:00000000}.bin",
                        FileData = new SubStream(br.BaseStream, entry.offset, entry.size),
                        entry = entry
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //Entries
                var dataOffset = Files.Count * 0x4;
                foreach (var file in Files)
                {
                    bw.Write(dataOffset);
                    dataOffset = (dataOffset + (int)file.FileSize + 0x3) & ~0x3;
                }

                //Files
                foreach (var file in Files)
                {
                    file.FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment(0x4);
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
