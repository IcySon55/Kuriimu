using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Komponent.IO;
using System.Linq;

namespace archive_gk2.arc1
{
    public class ARC1
    {
        public List<Arc1FileInfo> Files = new List<Arc1FileInfo>();
        private Stream _stream = null;
        private Import imports = new Import();

        Dictionary<string, string> extensions = new Dictionary<string, string>
        {
            ["RECN"] = "ncer",
            ["RNAN"] = "nanr",
            ["RGCN"] = "ncgr",
            ["RLCN"] = "nclr",
        };

        public ARC1(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Entries
                var entries = new List<Entry>();
                entries.Add(br.ReadStruct<Entry>());
                while (entries.Last().size != 0) entries.Add(br.ReadStruct<Entry>());

                //Files
                for (int i = 0; i < entries.Count; i++)
                    if (entries[i].size != 0)
                    {
                        var ext = br.PeekString(entries[i].offset, 4);
                        Files.Add(new Arc1FileInfo
                        {
                            State = ArchiveFileState.Archived,
                            FileName = (extensions.ContainsKey(ext)) ? $"{i:00000000}.{extensions[ext]}" : $"{i:00000000}.bin",
                            FileData = ((entries[i].size & 0x80000000) == 0)
                            ? new SubStream(br.BaseStream, entries[i].offset, entries[i].size & 0x7fffffff)
                            : new SubStream(br.BaseStream, entries[i].offset, (entries[i + 1].offset & 0x7fffffff) - (entries[i].offset & 0x7fffffff)),
                            entry = entries[i],
                            imports = imports
                        });
                    }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //Files
                uint dataOffset = ((uint)Files.Count + 1) * 0x8;
                foreach (var file in Files)
                {
                    file.Write(bw.BaseStream, dataOffset);
                    dataOffset = (uint)bw.BaseStream.Length;
                }

                //Entries
                bw.BaseStream.Position = 0;
                foreach (var file in Files) bw.WriteStruct(file.entry);
                bw.Write((uint)bw.BaseStream.Length);
                bw.Write((uint)0);
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
