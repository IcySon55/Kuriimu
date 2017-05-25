using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_ctpk
{
    public sealed class CTPK
    {
        public List<CTPKFileInfo> Files = new List<CTPKFileInfo>();
        Stream _stream = null;

        public CTPK(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                var header = br.ReadStruct<CTPKHeader>();

                //TexEntries
                var entries = br.ReadMultiple<CTPKEntry>(header.texCount).ToList();

                //TexInfo List
                var texSizeList = br.ReadMultiple<int>(header.texCount).ToList();

                //Name List
                var nameList = entries.Select(_ => br.ReadCStringA()).ToList();

                //Hash List
                br.BaseStream.Position = header.crc32SecOffset;
                var crc32List = br.ReadMultiple<CTPKHashEntry>(header.texCount).ToList();

                //TexInfo List 2
                br.BaseStream.Position = header.texInfoOffset;
                var texInfoList = br.ReadMultiple<uint>(header.texCount).ToList();

                //Get FileData
                for (int i = 0; i < header.texCount; i++)
                    Files.Add(new CTPKFileInfo()
                    {
                        State = ArchiveFileState.Archived,
                        FileName = nameList[i],
                        FileData = new SubStream(br.BaseStream, entries[i].texOffset + header.texSecOffset, entries[i].texDataSize),
                        Entry = entries[i],
                        hashEntry = crc32List[i],
                        texInfo = texInfoList[i]
                    });
            }
        }

        public void Save(Stream output)
        {
            int Pad128(int n) => (n + 127) & ~127;

            using (var bw = new BinaryWriterX(output))
            {
                //get nameList Length
                int nameListLength = (Files.Sum(afi => afi.FileName.Length + 1) + 3) & ~3;

                //Offsets
                int nameOffset = (Files.Count + 1) * 0x20 + Files.Count * 0x4;

                //Header
                bw.WriteStruct(new CTPKHeader
                {
                    texCount = (short)Files.Count,
                    texSecOffset = Pad128(nameOffset + nameListLength + Files.Count * 12),
                    texSecSize = (int)Files.Sum(afi => afi.FileSize),
                    crc32SecOffset = nameOffset + nameListLength,
                    texInfoOffset = nameOffset + nameListLength + Files.Count * 0x8
                });

                //entryList
                int dataOffset = 0;
                foreach (var afi in Files)
                {
                    dataOffset = Pad128(dataOffset);
                    var entry = afi.Entry;
                    entry.texDataSize = (int)afi.FileData.Length;
                    entry.nameOffset = nameOffset;
                    entry.texOffset = dataOffset;
                    bw.WriteStruct(entry);
                    nameOffset += afi.FileName.Length + 1;
                    dataOffset += (int)afi.FileSize;
                }

                //texInfo 1 List
                foreach (var afi in Files) bw.Write((int)afi.FileData.Length);

                //nameList
                foreach (var afi in Files) { bw.WriteASCII(afi.FileName + '\0'); }
                while (bw.BaseStream.Position % 4 != 0) bw.BaseStream.Position++;

                //crc32List
                foreach (var afi in Files) { bw.Write(afi.hashEntry.crc32); bw.Write(afi.hashEntry.entryNr); }

                //texInfo 2 List
                foreach (var afi in Files) bw.Write(afi.texInfo);

                //Write data
                foreach (var afi in Files)
                {
                    bw.Write(new byte[Pad128((int)bw.BaseStream.Length) - (int)bw.BaseStream.Length]);
                    afi.FileData.CopyTo(bw.BaseStream);
                }
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
