using System.Collections.Generic;
using System.IO;
using System.Text;
using Kontract.IO;

namespace text_srtz
{
    public class SRVC
    {
        public const int HashTableLength = 0x138;

        public Header Header = null;
        private byte[] HashTable = null;
        private byte[] Block = null;
        private List<EntryData> Metas = new List<EntryData>();
        public List<Entry> Entries = new List<Entry>();

        private Encoding shift = Encoding.GetEncoding("shift-jis");

        public SRVC(Stream input)
        {
            using (var br = new BinaryReaderX(input, false))
            {
                Header = br.ReadStruct<Header>();
                HashTable = br.ReadBytes(HashTableLength);
                int blockSize = Header.AugmentA * 8 + Header.AugmentB * 4 + Header.AugmentC * 8 + 4;
                Block = br.ReadBytes(blockSize);

                Metas = br.ReadMultiple<EntryData>(Header.StringCount);

                for (int i = 0; i < Header.StringCount; i++)
                {
                    var bytes = new List<byte>();
                    byte current = 0xFF;
                    while (current != 0x0)
                    {
                        current = br.ReadByte();
                        if (current != 0x0)
                            bytes.Add(current);
                    }
                    Entries.Add(new Entry
                    {
                        ID = Metas[i].ID,
                        Text = shift.GetString(bytes.ToArray())
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                bw.WriteStruct(Header);
                bw.Write(HashTable);
                bw.Write(Block);

                if (Metas.Count > 0)
                {
                    bw.WriteStruct(Metas[0]);

                    uint runningTotal = 0;
                    for (int i = 1; i < Metas.Count; i++)
                    {
                        var bytes = shift.GetBytes(Entries[i - 1].Text);
                        Metas[i].Offset = runningTotal + (uint)bytes.Length + 1;
                        runningTotal += (uint)bytes.Length + 1;
                        bw.WriteStruct(Metas[i]);
                    }
                }

                if (Entries.Count > 0)
                {
                    foreach(var entry in Entries)
                    {
                        bw.Write(shift.GetBytes(entry.Text));
                        bw.Write((byte)0x0);
                    }
                }

                bw.WriteAlignment();
            }
        }
    }
}
