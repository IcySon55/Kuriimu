using System.Collections.Generic;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_dc1
{
    public class DC1
    {
        public List<Dc1FileInfo> Files = new List<Dc1FileInfo>();
        private Stream _stream = null;

        private static Dictionary<string, string> _knownFiles = new Dictionary<string, string>
        {
            ["OBJ"] = ".obj"
        };

        Header header;
        List<Entry> entries = new List<Entry>();

        public DC1(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                header = br.ReadStruct<Header>();

                var objCount = (br.ReadUInt32() - 0x10) / 4;
                br.BaseStream.Position -= 4;

                //Entrylist
                for (int i = 0; i < objCount - 1; i++)
                {
                    var offset = br.ReadUInt32();
                    entries.Add(new Entry
                    {
                        offset = offset,
                        size = br.ReadUInt32() - offset
                    });
                    br.BaseStream.Position -= 4;
                }

                //Files
                for (int i = 0; i < objCount - 1; i++)
                {
                    br.BaseStream.Position = entries[i].offset;
                    var mag = br.ReadString(4);
                    var extension = (_knownFiles.ContainsKey(mag)) ? _knownFiles[mag] : ".bin";

                    Files.Add(new Dc1FileInfo
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
                bw.BaseStream.Position = 0x10;
                int dataOffset = Files.Count * 4 + 0x14;

                //Write entries
                for (int i=0;i<Files.Count;i++)
                {
                    bw.Write(dataOffset);
                    dataOffset += (int)Files[i].FileSize;
                }
                bw.Write(dataOffset);

                //Write FileData
                for (int i = 0; i < Files.Count; i++)
                {
                    Files[i].FileData.CopyTo(bw.BaseStream);
                }

                //Header
                bw.BaseStream.Position = 0;
                header.fileSize = (uint)bw.BaseStream.Length;
                bw.WriteStruct(header);
            }
        }

        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
