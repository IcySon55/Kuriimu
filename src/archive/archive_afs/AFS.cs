using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_afs
{
    public sealed class AFS
    {
        public List<AFSFileInfo> Files = new List<AFSFileInfo>();
        public List<NameEntry> nameList = new List<NameEntry>();

        public Header header;

        public AFS(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //EntryList
                List<Entry> entries = new List<Entry>();
                entries = br.ReadMultiple<Entry>(header.fileCount);

                //nameList
                uint nameListOffset = br.ReadUInt32();
                uint nameListSize = br.ReadUInt32();

                br.BaseStream.Position = nameListOffset;
                foreach (var entry in entries)
                {
                    nameList.Add(new NameEntry(br.BaseStream));
                }

                //Files
                var id = 0;
                foreach (var entry in entries)
                {
                    Files.Add(new AFSFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileData = new SubStream(br.BaseStream, entry.offset, entry.size),
                        FileName = nameList[id++].name
                    });
                }
            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {

            }
        }
    }
}
