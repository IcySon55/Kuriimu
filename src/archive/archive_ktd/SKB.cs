using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Komponent.IO;
using Kontract.Interface;

namespace archive_skb
{
    public sealed class SKB
    {
        public List<SKBFileInfo> Files = new List<SKBFileInfo>();
        Stream _stream = null;

        byte[] unk1;
        List<Entry> entries = new List<Entry>();

        public SKB(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                unk1 = br.ReadBytes(0x18);
                br.BaseStream.Position = 0x20;

                //Entries
                var entryCount = br.ReadUInt32();
                for (int i = 0; i < entryCount; i++)
                {
                    entries.Add(new Entry
                    {
                        offset = br.ReadUInt32()
                    });
                }
                for (int i = 0; i < entryCount; i++)
                {
                    entries[i].size = br.ReadUInt32();
                }

                //Files
                for (int i = 0; i < entryCount; i++)
                {
                    Files.Add(new SKBFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{i:00000000}.bin",
                        FileData = new SubStream(br.BaseStream, entries[i].offset, entries[i].size)
                    });
                }
            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {
                bw.Write(unk1);
                bw.BaseStream.Position = 0x20;
                bw.Write(Files.Count);

                var dataOffset = 0x80;
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.Write(dataOffset);
                    dataOffset = (dataOffset + (int)Files[i].FileSize) + 0x7f & ~0x7f;
                }
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.Write((int)Files[i].FileSize);
                }

                bw.BaseStream.Position = 0x80;
                for (int i = 0; i < Files.Count; i++)
                {
                    Files[i].FileData.CopyTo(bw.BaseStream);
                    bw.BaseStream.Position = bw.BaseStream.Position + 0x7f & ~0x7f;
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
