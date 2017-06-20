using System.Collections.Generic;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_pak
{
    public class PAK
    {
        public List<PakFileInfo> Files = new List<PakFileInfo>();
        private Stream _stream = null;

        List<List<Node>> nodes = new List<List<Node>>();

        Header header;
        List<string> filenames = new List<string>();
        List<FileEntry> entries = new List<FileEntry>();

        //just backup things
        byte[] filenamePart;

        public PAK(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                header = br.ReadStruct<Header>();

                //FileNameTable
                br.BaseStream.Position = header.filenameTableOffset;
                while (br.BaseStream.Position < header.fileTableOffset - 2)
                {
                    var fileCount = br.ReadUInt16();
                    nodes.Add(new List<Node>());
                    nodes[nodes.Count - 1].AddRange(br.ReadMultiple<Node>(fileCount));
                }
                GetFilenames(br);

                //File Entries
                br.BaseStream.Position = header.fileTableOffset;
                entries = br.ReadMultiple<FileEntry>(header.fileCount);

                //Files
                for (int i = 0; i < header.fileCount; i++)
                {
                    Files.Add(new PakFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = filenames[i],
                        FileData = new SubStream(br.BaseStream, entries[i].offset, entries[i].size)
                    });
                }

                br.BaseStream.Position = 8;
                filenamePart = br.ReadBytes((int)header.fileTableOffset - 0x8);
            }
        }

        public void GetFilenames(BinaryReaderX br, int index = 0, string curName = "")
        {
            for (int i = 0; i < nodes[index].Count; i++)
            {
                if ((nodes[index][i].flags & 0x1) == 0)
                {
                    br.BaseStream.Position = nodes[index][i].stringOffset;
                    GetFilenames(br, index + 1, curName + br.ReadCStringA());
                }
                else
                {
                    br.BaseStream.Position = nodes[index][i].stringOffset;
                    filenames.Add(curName + br.ReadCStringA());
                }
            }
            nodes.Remove(nodes[index]);
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //Header
                bw.WriteStruct(header);

                //filenames
                bw.Write(filenamePart);

                //FileEntries
                uint dataOffset = header.fileTableOffset + (uint)Files.Count * 8;
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.Write((uint)Files[i].FileSize);
                    bw.Write(dataOffset);
                    dataOffset += (uint)Files[i].FileSize;
                }

                //Files
                for (int i = 0; i < Files.Count; i++)
                {
                    Files[i].FileData.CopyTo(bw.BaseStream);
                }
            }
        }

        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
