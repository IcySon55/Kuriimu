using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.IO;
using Kuriimu.Contract;
using System.Text;

namespace Cetera.Archive
{
    public sealed class DARC
    {
        public List<DarcFileInfo> Files;

        public class DarcFileInfo : ArchiveFileInfo
        {
            public string SingleName;
            public Entry Entry;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class Header
        {
            public Magic magic = "darc";
            public ByteOrder byteOrder = ByteOrder.LittleEndian;
            public short headerSize = 0x1C;
            public int version = 0x1000000;
            public int fileSize;
            public int tableOffset = 0x1C;
            public int tableLength;
            public int dataOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Entry
        {
            public int filenameOffset;
            public int fileOffset;
            public int size;

            public bool IsFolder => (filenameOffset >> 24) == 1;
        }

        public DARC(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                var header = br.ReadStruct<Header>();

                //EntryList
                var entries = br.ReadMultiple<Entry>(1).ToList();
                entries.AddRange(br.ReadMultiple<Entry>(entries[0].size - 1));

                //FileData + names
                Files = entries.Select(entry => new DarcFileInfo
                {
                    Entry = entry,
                    SingleName = br.ReadCStringW(),
                    FileData = entry.IsFolder ? null : new SubStream(br.BaseStream, entry.fileOffset, entry.size),
                    State = ArchiveFileState.Archived
                }).ToList();

                for (int i = 0; i < Files.Count; i++)
                {
                    var arcPath = Files[i].SingleName;
                    if (Files[i].Entry.IsFolder)
                    {
                        for (int j = i; j < Files[i].Entry.size; j++)
                        {
                            Files[j].FileName += arcPath + '/';
                        }
                    }
                    else
                    {
                        Files[i].FileName += arcPath;
                    }
                }
            }

        }

        int Pad(int pos, string filename)
        {
            if (!dicPadding.TryGetValue(Path.GetExtension(filename), out int padding))
            {
                padding = 4;
            }
            return (pos + padding - 1) & -padding;
        }

        public static Dictionary<string, int> dicPadding = new Dictionary<string, int>
        {
            [".bclim"] = 0x80,
            [".bcfnt"] = 0x2000,
        };

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {
                var header = new Header { tableLength = Files.Count * 12 + Files.Sum(afi => afi.SingleName.Length + 1) * 2 };
                int dataOffset = Pad(header.tableLength + 0x1C, "");
                header.dataOffset = dataOffset;
                bw.WriteStruct(header);

                foreach (var afi in Files.Where(afi => !afi.Entry.IsFolder).OrderBy(afi => afi.Entry.fileOffset))
                {
                    dataOffset = Pad(dataOffset, afi.FileName);
                    afi.Entry.fileOffset = dataOffset;
                    afi.Entry.size = (int)afi.FileData.Length;
                    dataOffset += afi.Entry.size;
                }

                //Write Entry table
                foreach (var afi in Files)
                    bw.WriteStruct(afi.Entry);

                //Write names
                foreach (var afi in Files)
                    bw.Write(Encoding.Unicode.GetBytes(afi.SingleName + '\0'));

                //Write padding
                while (bw.BaseStream.Position % 4 != 0)
                    bw.Write((byte)0);

                //Write FileData
                foreach (var afi in Files.Where(afi => !afi.Entry.IsFolder).OrderBy(afi => afi.Entry.fileOffset))
                {
                    bw.Write(new byte[Pad((int)bw.BaseStream.Length, afi.FileName) - (int)bw.BaseStream.Length]); // padding
                    afi.FileData.CopyTo(bw.BaseStream);
                }

                bw.BaseStream.Position = 0;
                header.fileSize = (int)bw.BaseStream.Length;
                bw.WriteStruct(header);
            }
        }
    }
}