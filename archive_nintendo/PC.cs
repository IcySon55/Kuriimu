using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace archive_nintendo.PC
{
    public sealed class PC
    {
        public List<PCFileInfo> Files = new List<PCFileInfo>();
        Stream _stream = null;

        private static Dictionary<string, string> _knownFiles = new Dictionary<string, string>
        {
            ["BCH"] = ".bch",
            ["CGFX"] = ".cgfx",
        };

        Header header;
        List<Entry> entries = new List<Entry>();

        public PC(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                header = br.ReadStruct<Header>();

                for (int i = 0; i < header.entryCount; i++)
                {
                    var offset = br.ReadInt32();
                    entries.Add(new Entry(offset, br.ReadInt32() - offset));
                    br.BaseStream.Position -= 4;
                }

                //Files
                for (int i = 0; i < header.entryCount; i++)
                {
                    br.BaseStream.Position = entries[i].offset;
                    var magS = br.ReadString(4);
                    var extension = _knownFiles.ContainsKey(magS) ? _knownFiles[magS] : ".bin";

                    Files.Add(new PCFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{i:00000000}" + extension,
                        FileData = new SubStream(br.BaseStream, entries[i].offset, entries[i].size)
                    });
                }
            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {
                var dataOffset = 4 + Files.Count * 4 + 4;
                dataOffset = dataOffset + 0x7f & ~0x7f;

                bw.WriteStruct(header);

                //Files
                bw.BaseStream.Position = dataOffset;
                for (int i = 0; i < Files.Count; i++)
                {
                    Files[i].FileData.CopyTo(bw.BaseStream);
                    bw.WritePadding(0x80);
                }

                //Offsetlist
                bw.BaseStream.Position = 4;
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.Write(dataOffset);
                    dataOffset = (dataOffset + (int)Files[i].FileSize) + 0x7f & ~0x7f;
                }
                bw.Write(dataOffset);
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
